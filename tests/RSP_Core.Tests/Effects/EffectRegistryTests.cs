using Xunit;
using RSP_Core.Effects;
using RSP_Core.Models;

namespace RSP_Core.Tests.Effects
{
    public class EffectRegistryTests
    {
        [Fact]
        public void EffectRegistry_AttackDamage_DealsCorrectDamage()
        {
            // Arrange
            var registry = new EffectRegistry();
            var snapshot = new BattleSnapshot
            {
                Enemy = new EnemyState { HP = 100, MaxHP = 100 }
            };
            var card = new CardInstance("test", new CardDefinition { BaseValue = 10 });
            var context = new EffectContext(snapshot, card) { Value = 15 };

            // Act
            var success = registry.TryExecuteEffect("attack_damage", context, out var error);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.Equal(85, snapshot.Enemy.HP);
        }

        [Fact]
        public void EffectRegistry_DefensePercent_AddsDefense()
        {
            // Arrange
            var registry = new EffectRegistry();
            var snapshot = new BattleSnapshot
            {
                Player = new PlayerState { DefensePercent = 0 }
            };
            var card = new CardInstance("test", new CardDefinition());
            var context = new EffectContext(snapshot, card) { Value = 30 };

            // Act
            var success = registry.TryExecuteEffect("defense_percent", context, out var error);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.Equal(30, snapshot.Player.DefensePercent);
        }

        [Fact]
        public void EffectRegistry_SkillDraw_DrawsCards()
        {
            // Arrange
            var registry = new EffectRegistry();
            var snapshot = new BattleSnapshot
            {
                Player = new PlayerState()
            };
            snapshot.Player.Deck.Add(new CardInstance("1", new CardDefinition()));
            snapshot.Player.Deck.Add(new CardInstance("2", new CardDefinition()));
            snapshot.Player.Deck.Add(new CardInstance("3", new CardDefinition()));

            var card = new CardInstance("test", new CardDefinition());
            var context = new EffectContext(snapshot, card) { Value = 2 };

            // Act
            var success = registry.TryExecuteEffect("skill_draw", context, out var error);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.Equal(2, snapshot.Player.Hand.Count);
            Assert.Single(snapshot.Player.Deck);
        }

        [Fact]
        public void EffectRegistry_StatusBleed_AppliesStacks()
        {
            // Arrange
            var registry = new EffectRegistry();
            var snapshot = new BattleSnapshot
            {
                Enemy = new EnemyState()
            };
            var card = new CardInstance("test", new CardDefinition());
            var context = new EffectContext(snapshot, card) { Value = 3 };

            // Act
            var success = registry.TryExecuteEffect("status_bleed", context, out var error);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.True(snapshot.Enemy.StatusEffects.ContainsKey("bleed"));
            Assert.Equal(3, snapshot.Enemy.StatusEffects["bleed"]);
        }

        [Fact]
        public void EffectRegistry_ExternalEffect_CanBeRegistered()
        {
            // Arrange
            var registry = new EffectRegistry();
            bool wasExecuted = false;

            registry.RegisterEffect("custom_effect", ctx =>
            {
                wasExecuted = true;
                ctx.Snapshot.Player.HP += 10;
            });

            var snapshot = new BattleSnapshot
            {
                Player = new PlayerState { HP = 50 }
            };
            var card = new CardInstance("test", new CardDefinition());
            var context = new EffectContext(snapshot, card);

            // Act
            var success = registry.TryExecuteEffect("custom_effect", context, out var error);

            // Assert
            Assert.True(success);
            Assert.True(wasExecuted);
            Assert.Equal(60, snapshot.Player.HP);
        }

        [Fact]
        public void EffectRegistry_UnknownEffect_ReturnsErrorTag()
        {
            // Arrange
            var registry = new EffectRegistry();
            var snapshot = new BattleSnapshot();
            var card = new CardInstance("test", new CardDefinition());
            var context = new EffectContext(snapshot, card);

            // Act
            var success = registry.TryExecuteEffect("unknown_effect", context, out var error);

            // Assert
            Assert.False(success);
            Assert.Equal("EffectNotFound:unknown_effect", error);
        }

        [Fact]
        public void EffectRegistry_RegisterEffect_NullEffectId_ThrowsException()
        {
            // Arrange
            var registry = new EffectRegistry();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                registry.RegisterEffect(null, ctx => { }));
        }

        [Fact]
        public void EffectRegistry_RegisterEffect_EmptyEffectId_ThrowsException()
        {
            // Arrange
            var registry = new EffectRegistry();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                registry.RegisterEffect("", ctx => { }));
        }

        [Fact]
        public void EffectRegistry_RegisterEffect_NullHandler_ThrowsException()
        {
            // Arrange
            var registry = new EffectRegistry();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                registry.RegisterEffect("test_effect", null));
        }
    }
}
