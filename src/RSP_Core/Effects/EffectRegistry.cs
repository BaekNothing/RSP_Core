using System;
using System.Collections.Generic;

namespace RSP_Core.Effects
{
    /// <summary>
    /// Registry for card effects with support for external registration
    /// </summary>
    public class EffectRegistry
    {
        private readonly Dictionary<string, CardEffectDelegate> effects;

        public EffectRegistry()
        {
            effects = new Dictionary<string, CardEffectDelegate>();
            RegisterBuiltInEffects();
        }

        /// <summary>
        /// Register an effect handler
        /// </summary>
        public void RegisterEffect(string effectId, CardEffectDelegate handler)
        {
            if (string.IsNullOrEmpty(effectId))
            {
                throw new ArgumentException("Effect ID cannot be null or empty", nameof(effectId));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "Effect handler cannot be null");
            }

            effects[effectId] = handler;
        }

        /// <summary>
        /// Execute an effect by ID
        /// </summary>
        public bool TryExecuteEffect(string effectId, EffectContext context, out string errorTag)
        {
            if (effects.TryGetValue(effectId, out var handler))
            {
                try
                {
                    handler(context);
                    errorTag = string.Empty;
                    return true;
                }
                catch (Exception ex)
                {
                    errorTag = $"EffectError:{effectId}:{ex.Message}";
                    return false;
                }
            }

            errorTag = $"EffectNotFound:{effectId}";
            return false;
        }

        private void RegisterBuiltInEffects()
        {
            // Attack damage effect
            RegisterEffect("attack_damage", ctx =>
            {
                int damage = ctx.Value;
                ctx.Snapshot.Enemy.HP -= damage;
                if (ctx.Snapshot.Enemy.HP < 0) ctx.Snapshot.Enemy.HP = 0;
            });

            // Attack bonus damage (for win effects)
            RegisterEffect("attack_bonus_damage", ctx =>
            {
                int damage = ctx.Value;
                ctx.Snapshot.Enemy.HP -= damage;
                if (ctx.Snapshot.Enemy.HP < 0) ctx.Snapshot.Enemy.HP = 0;
            });

            // Defense percent (damage reduction for this turn)
            RegisterEffect("defense_percent", ctx =>
            {
                ctx.Snapshot.Player.DefensePercent += ctx.Value;
                if (ctx.Snapshot.Player.DefensePercent > 100)
                    ctx.Snapshot.Player.DefensePercent = 100;
            });

            // Draw cards
            RegisterEffect("skill_draw", ctx =>
            {
                int count = ctx.Value;
                for (int i = 0; i < count; i++)
                {
                    if (ctx.Snapshot.Player.Deck.Count == 0)
                    {
                        // Reshuffle discard into deck
                        if (ctx.Snapshot.Player.Discard.Count > 0)
                        {
                            ctx.Snapshot.Player.Deck.AddRange(ctx.Snapshot.Player.Discard);
                            ctx.Snapshot.Player.Discard.Clear();
                        }
                        else
                        {
                            break; // No more cards
                        }
                    }

                    if (ctx.Snapshot.Player.Deck.Count > 0)
                    {
                        var card = ctx.Snapshot.Player.Deck[0];
                        ctx.Snapshot.Player.Deck.RemoveAt(0);
                        ctx.Snapshot.Player.Hand.Add(card);
                    }
                }
            });

            // Apply bleed status
            RegisterEffect("status_bleed", ctx =>
            {
                int stacks = ctx.Value;
                if (ctx.Snapshot.Enemy.StatusEffects.ContainsKey("bleed"))
                {
                    ctx.Snapshot.Enemy.StatusEffects["bleed"] += stacks;
                }
                else
                {
                    ctx.Snapshot.Enemy.StatusEffects["bleed"] = stacks;
                }
            });
        }
    }
}
