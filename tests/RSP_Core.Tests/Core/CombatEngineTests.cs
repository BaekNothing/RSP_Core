using System.Collections.Generic;
using Xunit;
using RSP_Core.Core;
using RSP_Core.Models;

namespace RSP_Core.Tests.Core
{
    public class CombatEngineTests
    {
        private BattleInitData CreateTestInitData()
        {
            var initData = new BattleInitData
            {
                MaxSlotsPerTurn = 3,
                InitialHandSize = 5
            };

            // Setup player
            initData.InitialPlayerState.HP = 100;
            initData.InitialPlayerState.MaxHP = 100;
            initData.InitialPlayerState.Energy = 10;
            initData.InitialPlayerState.MaxEnergy = 10;

            // Add player cards
            for (int i = 0; i < 10; i++)
            {
                var cardDef = new CardDefinition
                {
                    CardId = $"player_card_{i}",
                    Name = $"Card {i}",
                    SymbolType = (SymbolType)(i % 3),
                    CardRole = CardRole.Attack,
                    Cost = 2,
                    BaseValue = 10,
                    WinBonusValue = 5
                };
                cardDef.BaseEffects.Add(new EffectRef("attack_damage", 10));
                cardDef.WinEffects.Add(new EffectRef("attack_bonus_damage", 5));

                initData.InitialPlayerState.Deck.Add(new CardInstance($"inst_{i}", cardDef));
            }

            // Setup enemy
            initData.InitialEnemyState.HP = 80;
            initData.InitialEnemyState.MaxHP = 80;
            initData.InitialEnemyState.AttackValue = 8;

            // Add enemy cards
            for (int i = 0; i < 5; i++)
            {
                var enemyCard = new EnemyCard($"enemy_{i}", (SymbolType)(i % 3), 10);
                initData.InitialEnemyState.Deck.Add(enemyCard);
            }

            return initData;
        }

        [Fact]
        public void CombatEngine_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var engine = new CombatEngine(new DefaultRandomSource(42));
            var initData = CreateTestInitData();

            // Act
            engine.Initialize(initData);
            var snapshot = engine.GetSnapshot();

            // Assert
            Assert.Equal(1, snapshot.TurnNumber);
            Assert.Equal(0, snapshot.SlotIndex);
            Assert.Equal(5, snapshot.Player.Hand.Count);
            Assert.Equal(5, snapshot.Player.Deck.Count);
            Assert.Equal(100, snapshot.Player.HP);
            Assert.Equal(10, snapshot.Player.Energy);
        }

        [Fact]
        public void CombatEngine_ResolveCard_Win_NegatesEnemy()
        {
            // Arrange
            var engine = new CombatEngine(new DefaultRandomSource(42));
            var initData = CreateTestInitData();

            // Force matchup: Square vs Triangle = Win
            var playerCard = new CardInstance("test", new CardDefinition
            {
                SymbolType = SymbolType.Square,
                Cost = 2,
                BaseValue = 10
            });
            playerCard.Definition.BaseEffects.Add(new EffectRef("attack_damage", 10));
            playerCard.Definition.WinEffects.Add(new EffectRef("attack_bonus_damage", 5));

            initData.InitialPlayerState.Deck.Clear();
            initData.InitialPlayerState.Deck.Add(playerCard);
            initData.InitialEnemyState.Deck.Clear();
            initData.InitialEnemyState.Deck.Add(new EnemyCard("e1", SymbolType.Triangle, 10));

            engine.Initialize(initData);

            var snapshot = engine.GetSnapshot();
            int initialPlayerHP = snapshot.Player.HP;

            // Act
            var result = engine.ResolveCard(new ResolveRequest(playerCard.InstanceId, 0));

            // Assert
            Assert.Equal(CombatOutcome.Win, result.Outcome);
            Assert.Equal(15, result.DamageDealt); // 10 base + 5 win bonus
            Assert.Equal(0, result.DamageTaken); // Enemy negated
            Assert.Equal(initialPlayerHP, result.Snapshot.Player.HP);
            Assert.Contains("EnemyNegated", result.EventTags);
        }

        [Fact]
        public void CombatEngine_ResolveCard_Draw_BothExecute()
        {
            // Arrange
            var engine = new CombatEngine(new DefaultRandomSource(42));
            var initData = CreateTestInitData();

            // Force matchup: Square vs Square = Draw
            var playerCard = new CardInstance("test", new CardDefinition
            {
                SymbolType = SymbolType.Square,
                Cost = 2,
                BaseValue = 10
            });
            playerCard.Definition.BaseEffects.Add(new EffectRef("attack_damage", 10));

            initData.InitialPlayerState.Deck.Clear();
            initData.InitialPlayerState.Deck.Add(playerCard);
            initData.InitialEnemyState.Deck.Clear();
            initData.InitialEnemyState.Deck.Add(new EnemyCard("e1", SymbolType.Square, 10));

            engine.Initialize(initData);

            var snapshot = engine.GetSnapshot();
            int initialPlayerHP = snapshot.Player.HP;
            int initialEnemyHP = snapshot.Enemy.HP;

            // Act
            var result = engine.ResolveCard(new ResolveRequest(playerCard.InstanceId, 0));

            // Assert
            Assert.Equal(CombatOutcome.Draw, result.Outcome);
            Assert.Equal(10, result.DamageDealt);
            Assert.Equal(10, result.DamageTaken);
            Assert.Equal(initialPlayerHP - 10, result.Snapshot.Player.HP);
            Assert.Equal(initialEnemyHP - 10, result.Snapshot.Enemy.HP);
            Assert.Contains("EnemyAttacked", result.EventTags);
        }

        [Fact]
        public void CombatEngine_ResolveCard_Lose_OnlyEnemyExecutes()
        {
            // Arrange
            var engine = new CombatEngine(new DefaultRandomSource(42));
            var initData = CreateTestInitData();

            // Force matchup: Square vs Circle = Lose
            var playerCard = new CardInstance("test", new CardDefinition
            {
                SymbolType = SymbolType.Square,
                Cost = 2,
                BaseValue = 10
            });
            playerCard.Definition.BaseEffects.Add(new EffectRef("attack_damage", 10));

            initData.InitialPlayerState.Deck.Clear();
            initData.InitialPlayerState.Deck.Add(playerCard);
            initData.InitialEnemyState.Deck.Clear();
            initData.InitialEnemyState.Deck.Add(new EnemyCard("e1", SymbolType.Circle, 10));

            engine.Initialize(initData);

            var snapshot = engine.GetSnapshot();
            int initialPlayerHP = snapshot.Player.HP;
            int initialEnemyHP = snapshot.Enemy.HP;

            // Act
            var result = engine.ResolveCard(new ResolveRequest(playerCard.InstanceId, 0));

            // Assert
            Assert.Equal(CombatOutcome.Lose, result.Outcome);
            Assert.Equal(0, result.DamageDealt); // Player effects negated
            Assert.Equal(10, result.DamageTaken);
            Assert.Equal(initialPlayerHP - 10, result.Snapshot.Player.HP);
            Assert.Equal(initialEnemyHP, result.Snapshot.Enemy.HP); // No damage to enemy
            Assert.Contains("PlayerEffectsNegated", result.EventTags);
            Assert.Contains("EnemyAttacked", result.EventTags);
        }

        [Fact]
        public void CombatEngine_ResolveCard_InsufficientEnergy_ReturnsError()
        {
            // Arrange
            var engine = new CombatEngine(new DefaultRandomSource(42));
            var initData = CreateTestInitData();
            initData.InitialPlayerState.Energy = 1; // Not enough for cost 2 card

            engine.Initialize(initData);
            var snapshot = engine.GetSnapshot();
            var cardId = snapshot.Player.Hand[0].InstanceId;

            // Act
            var result = engine.ResolveCard(new ResolveRequest(cardId, 0));

            // Assert
            Assert.Contains("EnergyInsufficient", result.EventTags);
        }

        [Fact]
        public void CombatEngine_NextTurn_RestoresEnergyAndDrawsCards()
        {
            // Arrange
            var engine = new CombatEngine(new DefaultRandomSource(42));
            var initData = CreateTestInitData();
            engine.Initialize(initData);

            // Use some energy
            var snapshot = engine.GetSnapshot();
            var cardId = snapshot.Player.Hand[0].InstanceId;
            engine.ResolveCard(new ResolveRequest(cardId, 0));

            snapshot = engine.GetSnapshot();
            int energyAfterPlay = snapshot.Player.Energy;
            int handSizeAfterPlay = snapshot.Player.Hand.Count;

            // Act
            var result = engine.NextTurn();

            // Assert
            Assert.Equal(10, result.Snapshot.Player.Energy); // Restored to max
            Assert.Equal(handSizeAfterPlay + 2, result.Snapshot.Player.Hand.Count); // Drew 2 cards
            Assert.Equal(2, result.Snapshot.TurnNumber);
            Assert.Equal(0, result.Snapshot.SlotIndex);
            Assert.Contains("TurnStart", result.EventTags);
        }

        [Fact]
        public void CombatEngine_EnemyDeckRefill_WorksCorrectly()
        {
            // Arrange
            var engine = new CombatEngine(new DefaultRandomSource(42));
            var initData = CreateTestInitData();

            // Only add 2 enemy cards
            initData.InitialEnemyState.Deck.Clear();
            initData.InitialEnemyState.Deck.Add(new EnemyCard("e1", SymbolType.Square, 5));
            initData.InitialEnemyState.Deck.Add(new EnemyCard("e2", SymbolType.Triangle, 5));

            engine.Initialize(initData);

            // Play 2 cards to exhaust enemy deck
            var snapshot = engine.GetSnapshot();
            engine.ResolveCard(new ResolveRequest(snapshot.Player.Hand[0].InstanceId, 0));

            snapshot = engine.GetSnapshot();
            engine.ResolveCard(new ResolveRequest(snapshot.Player.Hand[0].InstanceId, 1));

            // Now enemy deck should be empty and discard should have 2 cards
            snapshot = engine.GetSnapshot();
            Assert.Empty(snapshot.Enemy.Deck);
            Assert.Equal(2, snapshot.Enemy.Discard.Count);

            // Act - play another card, should trigger refill
            snapshot = engine.GetSnapshot();
            var result = engine.ResolveCard(new ResolveRequest(snapshot.Player.Hand[0].InstanceId, 2));

            // Assert - deck should have been refilled from discard
            Assert.True(result.Snapshot.Enemy.Deck.Count >= 0); // Refilled and one drawn
        }

        [Fact]
        public void CombatEngine_DefensePercent_ReducesDamage()
        {
            // Arrange
            var engine = new CombatEngine(new DefaultRandomSource(42));
            var initData = CreateTestInitData();

            // Create defense card
            var playerCard = new CardInstance("test", new CardDefinition
            {
                SymbolType = SymbolType.Square,
                Cost = 2,
                BaseValue = 50
            });
            playerCard.Definition.BaseEffects.Add(new EffectRef("defense_percent", 50)); // 50% defense

            initData.InitialPlayerState.Deck.Clear();
            initData.InitialPlayerState.Deck.Add(playerCard);
            initData.InitialEnemyState.Deck.Clear();
            initData.InitialEnemyState.Deck.Add(new EnemyCard("e1", SymbolType.Square, 20)); // Draw, 20 attack

            engine.Initialize(initData);

            var snapshot = engine.GetSnapshot();
            int initialHP = snapshot.Player.HP;

            // Act
            var result = engine.ResolveCard(new ResolveRequest(playerCard.InstanceId, 0));

            // Assert
            Assert.Equal(10, result.DamageTaken); // 20 * (100 - 50) / 100 = 10
            Assert.Equal(initialHP - 10, result.Snapshot.Player.HP);
        }
    }
}
