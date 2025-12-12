using System;
using System.Collections.Generic;
using System.Linq;
using RSP_Core.Models;
using RSP_Core.Effects;
using RSP_Core.Systems;

namespace RSP_Core.Core
{
    /// <summary>
    /// Main combat engine implementation
    /// </summary>
    public class CombatEngine : ICombatEngine
    {
        private BattleSnapshot currentSnapshot;
        private readonly IRandomSource randomSource;
        private readonly EffectRegistry effectRegistry;
        private bool isInitialized;

        public CombatEngine(IRandomSource randomSource = null, EffectRegistry effectRegistry = null)
        {
            this.randomSource = randomSource ?? new DefaultRandomSource();
            this.effectRegistry = effectRegistry ?? new EffectRegistry();
            isInitialized = false;
        }

        public void Initialize(BattleInitData initData)
        {
            currentSnapshot = new BattleSnapshot
            {
                Player = ClonePlayerState(initData.InitialPlayerState),
                Enemy = CloneEnemyState(initData.InitialEnemyState),
                TurnNumber = 1,
                SlotIndex = 0,
                MaxSlotsPerTurn = initData.MaxSlotsPerTurn
            };

            // Shuffle decks
            ShufflePlayerDeck();
            ShuffleEnemyDeck();

            // Draw initial hands
            DrawCards(initData.InitialHandSize);

            isInitialized = true;
        }

        public BattleSnapshot GetSnapshot()
        {
            if (!isInitialized)
                throw new InvalidOperationException("Combat engine not initialized");

            return CloneSnapshot(currentSnapshot);
        }

        public ResolveResult ResolveCard(ResolveRequest request)
        {
            if (!isInitialized)
                throw new InvalidOperationException("Combat engine not initialized");

            var result = new ResolveResult();

            // Find the card in hand
            var card = currentSnapshot.Player.Hand.FirstOrDefault(c => c.InstanceId == request.CardInstanceId);
            if (card == null)
            {
                throw new InvalidOperationException($"Card {request.CardInstanceId} not found in hand");
            }

            // Check energy
            if (currentSnapshot.Player.Energy < card.Definition.Cost)
            {
                result.Snapshot = CloneSnapshot(currentSnapshot);
                result.EventTags.Add("EnergyInsufficient");
                return result;
            }

            // Draw enemy card if needed
            if (currentSnapshot.Enemy.Deck.Count == 0)
            {
                RefillEnemyDeck();
            }

            EnemyCard enemyCard = null;
            if (currentSnapshot.Enemy.Deck.Count > 0)
            {
                enemyCard = currentSnapshot.Enemy.Deck[0];
                currentSnapshot.Enemy.Deck.RemoveAt(0);
                currentSnapshot.Enemy.Discard.Add(enemyCard);
            }

            SymbolType enemySymbol = enemyCard?.SymbolType ?? SymbolType.Square;
            int enemyAttack = enemyCard?.AttackValue ?? currentSnapshot.Enemy.AttackValue;

            // Determine outcome
            result.PlayerSymbol = card.Definition.SymbolType;
            result.EnemySymbol = enemySymbol;
            result.Outcome = SymbolMatchup.DetermineOutcome(result.PlayerSymbol, result.EnemySymbol);

            // Consume player card
            currentSnapshot.Player.Hand.Remove(card);
            currentSnapshot.Player.Energy -= card.Definition.Cost;
            currentSnapshot.Player.Discard.Add(card);

            // Reset defense for this resolution
            currentSnapshot.Player.DefensePercent = 0;

            // Execute effects based on outcome
            var eventTags = new List<string>();
            int damageDealt = 0;
            int damageTaken = 0;

            var effectContext = new EffectContext(currentSnapshot, card)
            {
                Outcome = result.Outcome
            };

            if (result.Outcome == CombatOutcome.Win)
            {
                // Execute base effects
                damageDealt += ExecuteEffects(card.Definition.BaseEffects, effectContext, eventTags);
                
                // Execute win effects
                damageDealt += ExecuteEffects(card.Definition.WinEffects, effectContext, eventTags);
                
                // Enemy attack is negated
                eventTags.Add("EnemyNegated");
            }
            else if (result.Outcome == CombatOutcome.Draw)
            {
                // Execute base effects
                damageDealt += ExecuteEffects(card.Definition.BaseEffects, effectContext, eventTags);
                
                // Enemy attacks
                damageTaken = ApplyEnemyAttack(enemyAttack);
                eventTags.Add("EnemyAttacked");
            }
            else // Lose
            {
                // Player effects do NOT execute
                eventTags.Add("PlayerEffectsNegated");
                
                // Enemy attacks
                damageTaken = ApplyEnemyAttack(enemyAttack);
                eventTags.Add("EnemyAttacked");
            }

            result.DamageDealt = damageDealt;
            result.DamageTaken = damageTaken;
            result.EventTags = eventTags;
            result.Snapshot = CloneSnapshot(currentSnapshot);

            // Advance slot
            currentSnapshot.SlotIndex++;

            return result;
        }

        public NextTurnResult NextTurn()
        {
            if (!isInitialized)
                throw new InvalidOperationException("Combat engine not initialized");

            var result = new NextTurnResult();

            // Reset energy
            currentSnapshot.Player.Energy = currentSnapshot.Player.MaxEnergy;

            // Reset defense
            currentSnapshot.Player.DefensePercent = 0;

            // Draw cards (2 cards per turn start)
            DrawCards(2);

            // Advance turn
            currentSnapshot.TurnNumber++;
            currentSnapshot.SlotIndex = 0;

            result.EventTags.Add("TurnStart");
            result.Snapshot = CloneSnapshot(currentSnapshot);

            return result;
        }

        /// <summary>
        /// Register external effect
        /// </summary>
        public void RegisterEffect(string effectId, CardEffectDelegate handler)
        {
            effectRegistry.RegisterEffect(effectId, handler);
        }

        private int ExecuteEffects(List<EffectRef> effects, EffectContext context, List<string> eventTags)
        {
            int totalDamage = 0;

            foreach (var effectRef in effects)
            {
                int oldEnemyHP = context.Snapshot.Enemy.HP;

                context.Value = effectRef.ValueOverride ?? context.SourceCard.Definition.BaseValue;
                context.Formula = effectRef.Formula ?? string.Empty;

                if (!effectRegistry.TryExecuteEffect(effectRef.EffectId, context, out string errorTag))
                {
                    eventTags.Add(errorTag);
                }

                // Track damage dealt
                int newEnemyHP = context.Snapshot.Enemy.HP;
                if (newEnemyHP < oldEnemyHP)
                {
                    totalDamage += (oldEnemyHP - newEnemyHP);
                }
            }

            return totalDamage;
        }

        private int ApplyEnemyAttack(int attack)
        {
            int defensePercent = currentSnapshot.Player.DefensePercent;
            int actualDamage = attack * (100 - defensePercent) / 100;
            if (actualDamage < 0) actualDamage = 0;

            currentSnapshot.Player.HP -= actualDamage;
            if (currentSnapshot.Player.HP < 0) currentSnapshot.Player.HP = 0;

            return actualDamage;
        }

        private void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (currentSnapshot.Player.Deck.Count == 0)
                {
                    if (currentSnapshot.Player.Discard.Count > 0)
                    {
                        currentSnapshot.Player.Deck.AddRange(currentSnapshot.Player.Discard);
                        currentSnapshot.Player.Discard.Clear();
                        ShufflePlayerDeck();
                    }
                    else
                    {
                        break;
                    }
                }

                if (currentSnapshot.Player.Deck.Count > 0)
                {
                    var card = currentSnapshot.Player.Deck[0];
                    currentSnapshot.Player.Deck.RemoveAt(0);
                    currentSnapshot.Player.Hand.Add(card);
                }
            }
        }

        private void ShufflePlayerDeck()
        {
            int n = currentSnapshot.Player.Deck.Count;
            while (n > 1)
            {
                n--;
                int k = randomSource.Next(0, n + 1);
                var temp = currentSnapshot.Player.Deck[k];
                currentSnapshot.Player.Deck[k] = currentSnapshot.Player.Deck[n];
                currentSnapshot.Player.Deck[n] = temp;
            }
        }

        private void ShuffleEnemyDeck()
        {
            int n = currentSnapshot.Enemy.Deck.Count;
            while (n > 1)
            {
                n--;
                int k = randomSource.Next(0, n + 1);
                var temp = currentSnapshot.Enemy.Deck[k];
                currentSnapshot.Enemy.Deck[k] = currentSnapshot.Enemy.Deck[n];
                currentSnapshot.Enemy.Deck[n] = temp;
            }
        }

        private void RefillEnemyDeck()
        {
            if (currentSnapshot.Enemy.Discard.Count > 0)
            {
                currentSnapshot.Enemy.Deck.AddRange(currentSnapshot.Enemy.Discard);
                currentSnapshot.Enemy.Discard.Clear();
                ShuffleEnemyDeck();
            }
        }

        private BattleSnapshot CloneSnapshot(BattleSnapshot source)
        {
            return new BattleSnapshot
            {
                Player = ClonePlayerState(source.Player),
                Enemy = CloneEnemyState(source.Enemy),
                TurnNumber = source.TurnNumber,
                SlotIndex = source.SlotIndex,
                MaxSlotsPerTurn = source.MaxSlotsPerTurn
            };
        }

        private PlayerState ClonePlayerState(PlayerState source)
        {
            return new PlayerState
            {
                HP = source.HP,
                MaxHP = source.MaxHP,
                Energy = source.Energy,
                MaxEnergy = source.MaxEnergy,
                DefensePercent = source.DefensePercent,
                Deck = new List<CardInstance>(source.Deck),
                Hand = new List<CardInstance>(source.Hand),
                Discard = new List<CardInstance>(source.Discard),
                StatusEffects = new Dictionary<string, int>(source.StatusEffects)
            };
        }

        private EnemyState CloneEnemyState(EnemyState source)
        {
            return new EnemyState
            {
                HP = source.HP,
                MaxHP = source.MaxHP,
                AttackValue = source.AttackValue,
                LastSymbol = source.LastSymbol,
                Deck = new List<EnemyCard>(source.Deck),
                Discard = new List<EnemyCard>(source.Discard),
                StatusEffects = new Dictionary<string, int>(source.StatusEffects)
            };
        }
    }
}
