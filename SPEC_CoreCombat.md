# RSP_Core Combat Engine Specification

## 1. Design Goals

The RSP_Core Combat Engine is a **headless, deterministic, turn-based combat system** designed for card games. It implements a symbol-based matchup mechanic (rock-paper-scissors) with an extensible effect system.

### Primary Goals
- **Headless architecture**: No UI, rendering, or presentation logic
- **Deterministic behavior**: Reproducible combat outcomes with controlled randomness
- **Unity-independent**: Pure .NET Standard 2.1, no UnityEngine dependencies
- **Extensible effects**: Plugin-style effect system for gameplay customization
- **Stateless API**: Snapshot/request/result pattern for clean integration

### Target Platform
- .NET Standard 2.1 for Unity 6.x compatibility
- DLL deployment to Unity's Assets/Plugins folder
- Also compatible with console applications and standalone C# projects

---

## 2. Core Combat Loop

### Turn Structure
A turn consists of:
1. **Turn Start**: Energy restoration, card draw, defense reset
2. **Card Resolution Phase**: Player plays 1-N cards in sequential slots
3. **Turn End**: Transition to next turn via `NextTurn()`

Each turn allows up to `MaxSlotsPerTurn` card plays (configurable, default: 3).

### Combat Flow
```
Initialize(BattleInitData)
  ↓
[Turn 1]
  ResolveCard(slot=0) → combat outcome → state mutation
  ResolveCard(slot=1) → combat outcome → state mutation
  ResolveCard(slot=2) → combat outcome → state mutation
  NextTurn() → advance to Turn 2
[Turn 2]
  ...
```

### State Inspection
- `GetSnapshot()` returns immutable state copy at any time
- Snapshots contain: player HP/energy/hand/deck, enemy HP/deck, turn number, slot index

---

## 3. Symbol System

### Symbol Types
Three symbols form a cyclic dominance relationship:
- **Square (□)**
- **Triangle (△)**
- **Circle (○)**

### Matchup Rules
```
Square > Triangle
Triangle > Circle
Circle > Square
```

When player and enemy symbols match: **Draw**

### Matchup Implementation
`SymbolMatchup.DetermineOutcome(playerSymbol, enemySymbol)` returns:
- `CombatOutcome.Win` - Player symbol beats enemy symbol
- `CombatOutcome.Draw` - Symbols match
- `CombatOutcome.Lose` - Enemy symbol beats player symbol

---

## 4. Combat Resolution Rules

### Win (Critical)
- Player's **base effects** execute
- Player's **win effects** execute
- Enemy action is **negated** (no damage taken)
- Result tags: `["EnemyNegated"]`

### Draw (Normal)
- Player's **base effects** execute
- Player's **win effects** do NOT execute
- Enemy attacks player (damage reduced by defense%)
- Result tags: `["EnemyAttacked"]`

### Lose (Failure)
- Player's **base effects** do NOT execute
- Player's **win effects** do NOT execute
- Enemy attacks player (damage reduced by defense%)
- Result tags: `["PlayerEffectsNegated", "EnemyAttacked"]`

### Defense Calculation
Actual damage = `AttackValue * (100 - DefensePercent) / 100`

Defense persists across card plays within the same turn, resets at turn boundary.

---

## 5. Card Model

### CardDefinition (Static Data)
Immutable blueprint shared by all instances:
- `CardId`: Unique identifier
- `Name`: Display name
- `SymbolType`: Square/Triangle/Circle
- `CardRole`: Attack/Defense/Skill (metadata)
- `Cost`: Energy cost to play
- `BaseValue`: Default numeric value for effects
- `WinBonusValue`: Bonus value (metadata, not used by engine)
- `BaseEffects`: List of `EffectRef` (executed on Win/Draw)
- `WinEffects`: List of `EffectRef` (executed only on Win)

### CardInstance (Runtime State)
Mutable instance-specific data:
- `InstanceId`: Unique runtime identifier (e.g., "inst_0")
- `Definition`: Reference to `CardDefinition`
- `Level`: Upgrade level (default: 1)
- `UpgradeCount`: Number of upgrades applied (default: 0)

### EffectRef (Effect Invocation)
- `EffectId`: String identifier (e.g., "attack_damage")
- `ValueOverride`: Optional value override (null = use BaseValue)
- `Formula`: Optional formula string (reserved for future use)

---

## 6. Effect System

### Effect Registry
The `EffectRegistry` maps `effectId` strings to handler functions:
```
effectId (string) → CardEffectDelegate (function)
```

### Built-in Effects
- `attack_damage`: Deal `value` damage to enemy HP
- `attack_bonus_damage`: Deal `value` damage to enemy HP (for win bonuses)
- `defense_percent`: Add `value`% to player defense (stacks within turn)
- `skill_draw`: Draw `value` cards from deck
- `status_bleed`: Apply `value` stacks of bleed status to enemy

### Effect Execution
Effects receive an `EffectContext`:
- `Snapshot`: Current battle state (mutable)
- `SourceCard`: Card that triggered the effect
- `Value`: Effective value (override or BaseValue)
- `Formula`: Formula string (if provided)
- `Outcome`: Combat outcome (Win/Draw/Lose)

Effect handlers mutate `Snapshot` directly (e.g., reduce enemy HP, add cards to hand).

### Missing Effect Handling
If `effectId` is not registered:
- Effect is **skipped** (not executed)
- Error tag added to result: `"EffectNotFound:{effectId}"`
- Combat continues normally

---

## 7. External Effect Injection

### Registration API
```csharp
public void RegisterEffect(string effectId, CardEffectDelegate handler)
```

### Use Cases
- Modding support: Add custom card abilities
- Game-specific effects: Define mechanics unique to your game
- Testing: Inject mock effects for unit tests

### Constraints
- `effectId` must be non-null, non-empty
- `handler` must be non-null
- Later registrations overwrite earlier ones (last wins)

### Example
```csharp
engine.RegisterEffect("lifesteal", ctx => {
    int damage = ctx.Value;
    ctx.Snapshot.Enemy.HP -= damage;
    ctx.Snapshot.Player.HP += damage / 2;
});
```

---

## 8. Deterministic Behavior

### Randomness Injection
The engine uses `IRandomSource` for all RNG:
- Deck shuffling
- Card drawing (when deck needs shuffle)

Default implementation: `DefaultRandomSource` wraps `System.Random`.

Deterministic testing:
```csharp
var engine = new CombatEngine(new DefaultRandomSource(seed: 42));
```

### State Immutability
- `GetSnapshot()` returns a **deep copy** of state
- External code cannot corrupt engine state
- Multiple `GetSnapshot()` calls return identical data (until mutation)

---

## 9. Energy Management

### Energy Model
- Player has `Energy` (current) and `MaxEnergy` (cap)
- Playing a card consumes `Cost` energy
- Energy restored to `MaxEnergy` at turn start
- Insufficient energy: Card play rejected with `"EnergyInsufficient"` tag

### Validation
`ResolveCard` checks:
1. Card exists in hand (else: `InvalidOperationException`)
2. Sufficient energy (else: return result with error tag, state unchanged)

---

## 10. Enemy Deck Management

### Deck Consumption
- One enemy card drawn per `ResolveCard` call
- Drawn card moved from `Deck` to `Discard`
- Enemy symbol and attack value used for combat resolution

### Refill Policy
When `Deck` is empty:
1. Move all cards from `Discard` to `Deck`
2. Shuffle `Deck`
3. Draw from replenished deck

If both `Deck` and `Discard` are empty: Use default `AttackValue` and `Square` symbol.

---

## 11. Explicit Non-Goals

The engine explicitly **does not** provide:
- **Unity UI**: No UI components, no MonoBehaviours
- **VFX/SFX**: No visual or audio effects (only EventTags for mapping)
- **Persistence**: No save/load, serialization, or database logic
- **Networking**: No multiplayer, matchmaking, or synchronization
- **AI**: No enemy decision-making (enemy deck is pre-defined)
- **Card generation**: No procedural card creation
- **Balance tuning**: No built-in balancing or difficulty scaling
- **Localization**: No text translation or i18n support

The engine is a **pure logic layer**. Host applications (Unity, console) handle presentation, persistence, and user interaction.

---

## 12. Error Handling Policy

### Exceptions
The engine throws exceptions for:
- Invalid initialization (null `BattleInitData`)
- Card not in hand (`InvalidOperationException`)
- Uninitialized engine access (`InvalidOperationException`)
- Invalid `RegisterEffect` arguments (`ArgumentException`, `ArgumentNullException`)

### Graceful Degradation
The engine returns error tags (no exceptions) for:
- Insufficient energy: `"EnergyInsufficient"` in `EventTags`
- Missing effect: `"EffectNotFound:{effectId}"` in `EventTags`
- Effect execution error: `"EffectError:{effectId}:{message}"` in `EventTags`

Host applications should inspect `EventTags` for non-critical errors.

---

## 13. Concurrency Model

The engine is **not thread-safe**. Host applications must ensure:
- Single-threaded access per `ICombatEngine` instance
- No concurrent `ResolveCard` or `NextTurn` calls
- Use separate engine instances for parallel battles

---

## 14. Future Extensions (Reserved)

The specification reserves space for:
- **Formula evaluation**: `EffectRef.Formula` currently unused
- **Status effect ticks**: `StatusEffects` dictionary populated but not processed
- **Upgrade mechanics**: `CardInstance.Level` and `UpgradeCount` available for host logic
- **Last enemy symbol**: `EnemyState.LastSymbol` tracked but not used

These features require host-side implementation or future engine updates.
