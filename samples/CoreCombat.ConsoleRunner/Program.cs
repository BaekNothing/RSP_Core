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

            Console.WriteLine("Battle initialized!");
            PrintSnapshot(engine.GetSnapshot());

            // Simulate a few turns
            for (int turn = 1; turn <= 3; turn++)
            {
                Console.WriteLine($"\n=== TURN {turn} ===");
                var snapshot = engine.GetSnapshot();

                // Play cards in available slots
                int cardsPlayed = 0;
                while (snapshot.SlotIndex < snapshot.MaxSlotsPerTurn && snapshot.Player.Hand.Count > 0)
                {
                    var card = snapshot.Player.Hand.FirstOrDefault(c =>
                        c.Definition.Cost <= snapshot.Player.Energy);

                    if (card == null)
                    {
                        Console.WriteLine("No playable cards remaining (insufficient energy)");
                        break;
                    }

                    Console.WriteLine($"\nSlot {snapshot.SlotIndex + 1}:");
                    Console.WriteLine($"Playing: {card.Definition.Name} ({card.Definition.SymbolType})");
                    Console.WriteLine($"Cost: {card.Definition.Cost} | Energy: {snapshot.Player.Energy}");

                    var result = engine.ResolveCard(new ResolveRequest(card.InstanceId, snapshot.SlotIndex));

                    Console.WriteLine($"Enemy plays: {result.EnemySymbol}");
                    Console.WriteLine($"Result: {result.Outcome}!");
                    Console.WriteLine($"Damage dealt: {result.DamageDealt} | Damage taken: {result.DamageTaken}");

                    if (result.EventTags.Count > 0)
                    {
                        Console.WriteLine($"Events: {string.Join(", ", result.EventTags)}");
                    }

                    Console.WriteLine($"Player HP: {result.Snapshot.Player.HP}/{result.Snapshot.Player.MaxHP}");
                    Console.WriteLine($"Enemy HP: {result.Snapshot.Enemy.HP}/{result.Snapshot.Enemy.MaxHP}");

                    snapshot = result.Snapshot;
                    cardsPlayed++;

                    // Check if battle ended
                    if (snapshot.Player.HP <= 0 || snapshot.Enemy.HP <= 0)
                    {
                        break;
                    }
                }

                snapshot = engine.GetSnapshot();
                if (snapshot.Player.HP <= 0)
                {
                    Console.WriteLine("\n=== DEFEAT ===");
                    Console.WriteLine("Player was defeated!");
                    break;
                }
                else if (snapshot.Enemy.HP <= 0)
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
            initData.InitialPlayerState.HP = 100;
            initData.InitialPlayerState.MaxHP = 100;
            initData.InitialPlayerState.Energy = 10;
            initData.InitialPlayerState.MaxEnergy = 10;

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
            initData.InitialEnemyState.HP = 80;
            initData.InitialEnemyState.MaxHP = 80;
            initData.InitialEnemyState.AttackValue = 10;

            // Add enemy cards with varied symbols
            initData.InitialEnemyState.Deck.Add(new EnemyCard("enemy_1", SymbolType.Square, 12));
            initData.InitialEnemyState.Deck.Add(new EnemyCard("enemy_2", SymbolType.Triangle, 10));
            initData.InitialEnemyState.Deck.Add(new EnemyCard("enemy_3", SymbolType.Circle, 11));
            initData.InitialEnemyState.Deck.Add(new EnemyCard("enemy_4", SymbolType.Square, 13));
            initData.InitialEnemyState.Deck.Add(new EnemyCard("enemy_5", SymbolType.Triangle, 9));

            return initData;
        }

        static void AddPlayerCard(BattleInitData initData, string name, SymbolType symbol,
            CardRole role, int cost, int baseValue,
            (string effectId, int value)[] baseEffects,
            (string effectId, int value)[] winEffects)
        {
            var cardDef = new CardDefinition
            {
                CardId = $"{name.ToLower().Replace(" ", "_")}_{initData.InitialPlayerState.Deck.Count}",
                Name = name,
                SymbolType = symbol,
                CardRole = role,
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

            initData.InitialPlayerState.Deck.Add(
                new CardInstance($"inst_{initData.InitialPlayerState.Deck.Count}", cardDef)
            );
        }

        static void PrintSnapshot(BattleSnapshot snapshot)
        {
            Console.WriteLine($"Turn: {snapshot.TurnNumber} | Slot: {snapshot.SlotIndex}/{snapshot.MaxSlotsPerTurn}");
            Console.WriteLine($"Player: {snapshot.Player.HP}/{snapshot.Player.MaxHP} HP | {snapshot.Player.Energy} Energy");
            Console.WriteLine($"Enemy: {snapshot.Enemy.HP}/{snapshot.Enemy.MaxHP} HP");
            Console.WriteLine($"Hand: {snapshot.Player.Hand.Count} cards | Deck: {snapshot.Player.Deck.Count} cards");
        }
    }
}
