using RSP_Core.Core;
using RSP_Core.Models;
using System;
using System.Linq;

namespace CoreCombat.ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== RSP_Core Combat Engine Demo ===\n");

            // Create engine with seed for reproducibility
            var engine = new CombatEngine(new DefaultRandomSource(42));

            // Initialize battle
            var initData = CreateBattleInitData();
            engine.Initialize(initData);
            var maxSlotsPerTurn = initData.MaxSlotsPerTurn;

            Console.WriteLine("Battle initialized!");
            PrintSnapshot(engine.GetSnapshot());

            // Simulate a few turns
            for (int turn = 1; turn <= 3; turn++)
            {
                Console.WriteLine($"\n=== TURN {turn} ===");
                var snapshot = engine.GetSnapshot();
                int slotIndex = 0;

                // Play cards in available slots
                int cardsPlayed = 0;
                while (slotIndex < maxSlotsPerTurn && snapshot.Player.Hand.Count > 0)
                {
                    var card = snapshot.Player.Hand.FirstOrDefault(c =>
                        c.Definition.Cost <= snapshot.Player.Energy);

                    if (card == null)
                    {
                        Console.WriteLine("No playable cards remaining (insufficient energy)");
                        break;
                    }

                    Console.WriteLine($"\nSlot {slotIndex + 1}:");
                    Console.WriteLine($"Playing: {card.Definition.Name} ({card.Definition.Symbol})");
                    Console.WriteLine($"Cost: {card.Definition.Cost} | Energy: {snapshot.Player.Energy}");

                    var result = engine.ResolveCard(new ResolveRequest(card.InstanceId, slotIndex));

                    Console.WriteLine($"Enemy plays: {result.EnemySymbol}");
                    Console.WriteLine($"Result: {result.Outcome}!");
                    Console.WriteLine($"Damage dealt: {result.DamageDealtToEnemy} | Damage taken: {result.DamageDealtToPlayer}");

                    if (result.EventTags.Count > 0)
                    {
                        Console.WriteLine($"Events: {string.Join(", ", result.EventTags)}");
                    }

                    Console.WriteLine($"Player HP: {result.Snapshot.Player.Hp}/{result.Snapshot.Player.MaxHp}");
                    Console.WriteLine($"Enemy HP: {result.Snapshot.Enemy.Hp}/{result.Snapshot.Enemy.MaxHp}");

                    snapshot = result.Snapshot;
                    cardsPlayed++;

                    // Check if battle ended
                    if (snapshot.IsBattleEnded)
                    {
                        break;
                    }

                    slotIndex++;
                }

                snapshot = engine.GetSnapshot();
                if (snapshot.IsPlayerDead)
                {
                    Console.WriteLine("\n=== DEFEAT ===");
                    Console.WriteLine("Player was defeated!");
                    break;
                }
                else if (snapshot.IsEnemyDead)
                {
                    Console.WriteLine("\n=== VICTORY ===");
                    Console.WriteLine("Enemy was defeated!");
                    break;
                }

                // End turn
                Console.WriteLine($"\nEnding turn... (played {cardsPlayed} cards)");
                var turnResult = engine.NextTurn();
                Console.WriteLine($"Energy restored to {turnResult.Snapshot.Player.MaxEnergy}");
                Console.WriteLine($"Drew 2 cards | Hand size: {turnResult.Snapshot.Player.Hand.Count}");
            }

            Console.WriteLine("\n=== Battle Complete ===");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static BattleInitData CreateBattleInitData()
        {
            var initData = new BattleInitData
            {
                MaxSlotsPerTurn = 3,
                InitialHandSize = 5
            };

            // Setup player
            initData.PlayerState.Hp = 100;
            initData.PlayerState.MaxHp = 100;
            initData.PlayerState.Energy = 10;
            initData.PlayerState.MaxEnergy = 10;

            // Create diverse card set
            AddPlayerCard(initData, "Strike", SymbolType.Square, CardRole.Attack, 2, 12,
                new[] { ("attack_damage", 12) },
                new[] { ("attack_bonus_damage", 6) });

            AddPlayerCard(initData, "Pierce", SymbolType.Triangle, CardRole.Attack, 2, 10,
                new[] { ("attack_damage", 10) },
                new[] { ("attack_bonus_damage", 5) });

            AddPlayerCard(initData, "Slash", SymbolType.Circle, CardRole.Attack, 2, 11,
                new[] { ("attack_damage", 11) },
                new[] { ("attack_bonus_damage", 5) });

            AddPlayerCard(initData, "Block", SymbolType.Square, CardRole.Defense, 1, 50,
                new[] { ("defense_percent", 50) },
                new (string, int)[] { });

            AddPlayerCard(initData, "Parry", SymbolType.Triangle, CardRole.Defense, 1, 40,
                new[] { ("defense_percent", 40) },
                new (string, int)[] { });

            AddPlayerCard(initData, "Draw Power", SymbolType.Circle, CardRole.Skill, 1, 2,
                new[] { ("skill_draw", 2) },
                new[] { ("skill_draw", 1) });

            AddPlayerCard(initData, "Bleed Strike", SymbolType.Square, CardRole.Attack, 3, 8,
                new[] { ("attack_damage", 8), ("status_bleed", 2) },
                new[] { ("status_bleed", 2) });

            // Duplicate some cards
            for (int i = 0; i < 3; i++)
            {
                AddPlayerCard(initData, "Strike", SymbolType.Square, CardRole.Attack, 2, 12,
                    new[] { ("attack_damage", 12) },
                    new[] { ("attack_bonus_damage", 6) });
            }

            // Setup enemy
            initData.EnemyState.Hp = 80;
            initData.EnemyState.MaxHp = 80;
            initData.EnemyState.AttackValue = 10;

            // Add enemy cards with varied symbols
            initData.EnemyState.Deck.Add(new EnemyCard("enemy_1", SymbolType.Square, 12));
            initData.EnemyState.Deck.Add(new EnemyCard("enemy_2", SymbolType.Triangle, 10));
            initData.EnemyState.Deck.Add(new EnemyCard("enemy_3", SymbolType.Circle, 11));
            initData.EnemyState.Deck.Add(new EnemyCard("enemy_4", SymbolType.Square, 13));
            initData.EnemyState.Deck.Add(new EnemyCard("enemy_5", SymbolType.Triangle, 9));

            return initData;
        }

        static void AddPlayerCard(BattleInitData initData, string name, SymbolType symbol,
            CardRole role, int cost, int baseValue,
            (string effectId, int value)[] baseEffects,
            (string effectId, int value)[] winEffects)
        {
            var cardDef = new CardDefinition
            {
                Id = $"{name.ToLower().Replace(" ", "_")}_{initData.PlayerState.Deck.Count}",
                Name = name,
                Symbol = symbol,
                Role = role,
                Cost = cost,
                BaseValue = baseValue,
                WinBonusValue = 0
            };

            foreach (var (effectId, value) in baseEffects)
            {
                cardDef.BaseEffects.Add(new EffectRef(effectId, value));
            }

            foreach (var (effectId, value) in winEffects)
            {
                cardDef.WinEffects.Add(new EffectRef(effectId, value));
            }

            initData.PlayerState.Deck.Add(
                new CardInstance($"inst_{initData.PlayerState.Deck.Count}", cardDef)
            );
        }

        static void PrintSnapshot(BattleSnapshot snapshot)
        {
            Console.WriteLine($"Turn: {snapshot.TurnNumber}");
            Console.WriteLine($"Player: {snapshot.Player.Hp}/{snapshot.Player.MaxHp} HP | {snapshot.Player.Energy} Energy");
            Console.WriteLine($"Enemy: {snapshot.Enemy.Hp}/{snapshot.Enemy.MaxHp} HP");
            Console.WriteLine($"Hand: {snapshot.Player.Hand.Count} cards | Deck: {snapshot.Player.Deck.Count} cards");
        }
    }
}
