# RSP_Core Public API Reference

## Overview

This document describes the public API surface of RSP_Core. All types are in the `RSP_Core` namespace hierarchy.

**DLL Name**: `RSP_Core.dll`  
**Target Framework**: .NET Standard 2.1  
**Namespaces**: `RSP_Core.Core`, `RSP_Core.Models`, `RSP_Core.Effects`, `RSP_Core.Systems`

---

## Core Interfaces

### ICombatEngine

**Namespace**: `RSP_Core.Core`

Primary interface for combat operations.

#### Methods

##### Initialize
```csharp
void Initialize(BattleInitData initData)
```
Initializes the combat engine with starting state.

**Parameters**:
- `initData`: Battle configuration and initial player/enemy state

**Throws**:
- `InvalidOperationException`: If already initialized
- `ArgumentNullException`: If `initData` is null

**Post-conditions**:
- Player and enemy decks shuffled
- Initial hand drawn
- Turn set to 1, slot to 0
- Engine ready for `ResolveCard` calls

---

##### GetSnapshot
```csharp
BattleSnapshot GetSnapshot()
```
Returns immutable copy of current battle state.

**Returns**: `BattleSnapshot` containing complete state

**Throws**:
- `InvalidOperationException`: If engine not initialized

**Note**: Multiple calls return identical data until state mutation occurs.

---

##### ResolveCard
```csharp
ResolveResult ResolveCard(ResolveRequest request)
```
Executes a card play in the current slot.

**Parameters**:
- `request`: Contains `CardInstanceId` and `SlotIndex`

**Returns**: `ResolveResult` with outcome, damage, and updated snapshot

**Throws**:
- `InvalidOperationException`: If engine not initialized
- `InvalidOperationException`: If card not in player's hand

**Behavior**:
- Validates energy (returns error tag if insufficient)
- Draws enemy card (with auto-refill if needed)
- Determines symbol matchup outcome
- Executes effects based on outcome
- Applies enemy damage (if applicable)
- Consumes player card (hand → discard)
- Advances slot index
- Returns result with EventTags

---

##### NextTurn
```csharp
NextTurnResult NextTurn()
```
Advances to the next turn.

**Returns**: `NextTurnResult` with updated snapshot and EventTags

**Throws**:
- `InvalidOperationException`: If engine not initialized

**Behavior**:
- Restores player energy to max
- Resets player defense to 0
- Draws 2 cards
- Increments turn number
- Resets slot index to 0
- Adds `"TurnStart"` event tag

---

## Request/Response DTOs

### ResolveRequest

**Namespace**: `RSP_Core.Models`

Request to resolve a card play.

#### Properties
```csharp
string CardInstanceId { get; set; }  // Card to play
int SlotIndex { get; set; }          // Slot number (informational)
```

#### Constructor
```csharp
ResolveRequest(string cardInstanceId, int slotIndex)
```

---

### ResolveResult

**Namespace**: `RSP_Core.Models`

Result of a card resolution.

#### Properties
```csharp
BattleSnapshot Snapshot { get; set; }      // Updated state
CombatOutcome Outcome { get; set; }        // Win/Draw/Lose
SymbolType PlayerSymbol { get; set; }      // Player's symbol
SymbolType EnemySymbol { get; set; }       // Enemy's symbol
int DamageDealt { get; set; }              // Damage to enemy
int DamageTaken { get; set; }              // Damage to player
List<string> EventTags { get; set; }       // Event identifiers
```

#### Common EventTags
- `"EnemyNegated"`: Enemy attack prevented (Win outcome)
- `"EnemyAttacked"`: Enemy dealt damage (Draw/Lose)
- `"PlayerEffectsNegated"`: Player effects skipped (Lose outcome)
- `"EnergyInsufficient"`: Not enough energy to play card
- `"EffectNotFound:{effectId}"`: Effect ID not registered
- `"EffectError:{effectId}:{message}"`: Effect threw exception

---

### NextTurnResult

**Namespace**: `RSP_Core.Models`

Result of turn advancement.

#### Properties
```csharp
BattleSnapshot Snapshot { get; set; }      // Updated state
List<string> EventTags { get; set; }       // Event identifiers
```

#### Common EventTags
- `"TurnStart"`: Turn successfully advanced

---

## State Models

### BattleSnapshot

**Namespace**: `RSP_Core.Models`

Complete battle state at a point in time.

#### Properties
```csharp
PlayerState Player { get; set; }       // Player state
EnemyState Enemy { get; set; }         // Enemy state
int TurnNumber { get; set; }           // Current turn (1-indexed)
int SlotIndex { get; set; }            // Current slot (0-indexed)
int MaxSlotsPerTurn { get; set; }      // Slots per turn limit
```

---

### PlayerState

**Namespace**: `RSP_Core.Models`

Player-specific state.

#### Properties
```csharp
int HP { get; set; }                        // Current health
int MaxHP { get; set; }                     // Max health
int Energy { get; set; }                    // Current energy
int MaxEnergy { get; set; }                 // Max energy
int DefensePercent { get; set; }            // Damage reduction %
List<CardInstance> Deck { get; set; }       // Draw pile
List<CardInstance> Hand { get; set; }       // Cards in hand
List<CardInstance> Discard { get; set; }    // Played/discarded cards
Dictionary<string, int> StatusEffects { get; set; }  // Status name → stacks
```

**Note**: `StatusEffects` are tracked but not processed by engine. Host handles status logic.

---

### EnemyState

**Namespace**: `RSP_Core.Models`

Enemy-specific state.

#### Properties
```csharp
int HP { get; set; }                        // Current health
int MaxHP { get; set; }                     // Max health
int AttackValue { get; set; }               // Default attack damage
SymbolType? LastSymbol { get; set; }        // Last played symbol (unused)
List<EnemyCard> Deck { get; set; }          // Unplayed enemy cards
List<EnemyCard> Discard { get; set; }       // Played enemy cards
Dictionary<string, int> StatusEffects { get; set; }  // Status name → stacks
```

---

### BattleInitData

**Namespace**: `RSP_Core.Models`

Configuration for initializing a battle.

#### Properties
```csharp
PlayerState InitialPlayerState { get; set; }  // Starting player state
EnemyState InitialEnemyState { get; set; }    // Starting enemy state
int MaxSlotsPerTurn { get; set; }             // Slots per turn (default: 3)
int InitialHandSize { get; set; }             // Initial draw (default: 5)
```

**Setup Requirements**:
- Populate `InitialPlayerState.Deck` with `CardInstance` objects
- Populate `InitialEnemyState.Deck` with `EnemyCard` objects
- Set HP, MaxHP, Energy, MaxEnergy on both states

---

### CardDefinition

**Namespace**: `RSP_Core.Models`

Static card blueprint.

#### Properties
```csharp
string CardId { get; set; }                // Unique identifier
string Name { get; set; }                  // Display name
SymbolType SymbolType { get; set; }        // Square/Triangle/Circle
CardRole CardRole { get; set; }            // Attack/Defense/Skill
int Cost { get; set; }                     // Energy cost
int BaseValue { get; set; }                // Default effect value
int WinBonusValue { get; set; }            // Bonus value (metadata)
List<EffectRef> BaseEffects { get; set; } // Effects on Win/Draw
List<EffectRef> WinEffects { get; set; }  // Effects only on Win
```

**Note**: `WinBonusValue` is not used by engine. Host can use for display.

---

### CardInstance

**Namespace**: `RSP_Core.Models`

Runtime card instance.

#### Properties
```csharp
string InstanceId { get; set; }            // Unique instance ID
CardDefinition Definition { get; set; }    // Card blueprint
int Level { get; set; }                    // Upgrade level (default: 1)
int UpgradeCount { get; set; }             // Upgrade count (default: 0)
```

#### Constructor
```csharp
CardInstance(string instanceId, CardDefinition definition)
```

**Note**: `Level` and `UpgradeCount` are tracked but not processed by engine.

---

### EnemyCard

**Namespace**: `RSP_Core.Models`

Simple enemy card.

#### Properties
```csharp
string CardId { get; set; }                // Card identifier
SymbolType SymbolType { get; set; }        // Square/Triangle/Circle
int AttackValue { get; set; }              // Damage dealt
```

#### Constructor
```csharp
EnemyCard(string cardId, SymbolType symbolType, int attackValue)
```

---

### EffectRef

**Namespace**: `RSP_Core.Models`

Reference to an effect invocation.

#### Properties
```csharp
string EffectId { get; set; }              // Effect identifier
int? ValueOverride { get; set; }           // Optional value (null = BaseValue)
string Formula { get; set; }               // Optional formula (unused)
```

#### Constructor
```csharp
EffectRef(string effectId, int? valueOverride = null, string? formula = null)
```

---

## Enums

### SymbolType

**Namespace**: `RSP_Core.Models`

```csharp
public enum SymbolType
{
    Square,
    Triangle,
    Circle
}
```

Matchup cycle: `Square > Triangle > Circle > Square`

---

### CombatOutcome

**Namespace**: `RSP_Core.Models`

```csharp
public enum CombatOutcome
{
    Win,   // Player wins matchup
    Draw,  // Symbols match
    Lose   // Player loses matchup
}
```

---

### CardRole

**Namespace**: `RSP_Core.Models`

```csharp
public enum CardRole
{
    Attack,
    Defense,
    Skill
}
```

**Note**: Metadata only. Engine does not enforce role-based rules.

---

## Effect System

### EffectContext

**Namespace**: `RSP_Core.Effects`

Context passed to effect handlers.

#### Properties
```csharp
BattleSnapshot Snapshot { get; set; }      // Current state (mutable)
CardInstance SourceCard { get; set; }      // Card triggering effect
int Value { get; set; }                    // Effective value
string Formula { get; set; }               // Formula string (unused)
CombatOutcome Outcome { get; set; }        // Combat outcome
```

**Usage**: Effect handlers mutate `Snapshot` directly (e.g., reduce enemy HP).

---

### CardEffectDelegate

**Namespace**: `RSP_Core.Effects`

```csharp
public delegate void CardEffectDelegate(EffectContext context)
```

Function signature for effect handlers.

**Contract**:
- Receives `EffectContext` with mutable `Snapshot`
- Mutates `Snapshot` to apply effect
- No return value
- May throw exceptions (caught and tagged by engine)

---

### EffectRegistry

**Namespace**: `RSP_Core.Effects`

Registry for effect handlers.

#### Methods

##### RegisterEffect
```csharp
public void RegisterEffect(string effectId, CardEffectDelegate handler)
```

Registers or replaces an effect handler.

**Parameters**:
- `effectId`: Non-null, non-empty string identifier
- `handler`: Non-null delegate

**Throws**:
- `ArgumentException`: If `effectId` is null or empty
- `ArgumentNullException`: If `handler` is null

**Behavior**: Later registrations overwrite earlier ones.

---

##### TryExecuteEffect
```csharp
public bool TryExecuteEffect(string effectId, EffectContext context, out string errorTag)
```

Executes an effect handler (internal API).

**Returns**: `true` if executed successfully, `false` otherwise

**Out Parameter**: `errorTag` contains error message if execution failed

---

## Built-in Effects

The engine registers these effects by default:

### attack_damage
Deals damage to enemy HP.
```csharp
ctx.Snapshot.Enemy.HP -= ctx.Value;
```

### attack_bonus_damage
Deals damage to enemy HP (semantically identical to `attack_damage`).
```csharp
ctx.Snapshot.Enemy.HP -= ctx.Value;
```

### defense_percent
Adds damage reduction percentage.
```csharp
ctx.Snapshot.Player.DefensePercent += ctx.Value;
```
**Note**: Defense capped at 100%. Stacks within turn.

### skill_draw
Draws cards from deck.
```csharp
// Draws ctx.Value cards
// Auto-reshuffles discard if deck empty
```

### status_bleed
Applies bleed stacks to enemy.
```csharp
ctx.Snapshot.Enemy.StatusEffects["bleed"] += ctx.Value;
```
**Note**: Engine tracks but does not process status effects.

---

## Random Source Interface

### IRandomSource

**Namespace**: `RSP_Core.Core`

Interface for random number generation.

#### Methods
```csharp
int Next(int min, int max)
```

Returns random integer in range `[min, max)` (exclusive max).

---

### DefaultRandomSource

**Namespace**: `RSP_Core.Core`

Default implementation wrapping `System.Random`.

#### Constructors
```csharp
DefaultRandomSource()                // Non-deterministic seed
DefaultRandomSource(int seed)        // Deterministic seed
```

---

## Error Handling

### Exceptions

The engine throws exceptions for:

| Exception Type | Condition |
|----------------|-----------|
| `InvalidOperationException` | Engine not initialized |
| `InvalidOperationException` | Card not in hand |
| `ArgumentException` | Invalid `effectId` (null/empty) |
| `ArgumentNullException` | Null `handler` or `initData` |

### Error Tags

Non-critical errors returned in `EventTags`:

| Tag | Meaning |
|-----|---------|
| `EnergyInsufficient` | Not enough energy to play card |
| `EffectNotFound:{effectId}` | Effect ID not registered |
| `EffectError:{effectId}:{message}` | Effect threw exception |

**Recovery**: Host should check `EventTags` and handle accordingly.

---

## Integration Example

```csharp
using RSP_Core.Core;
using RSP_Core.Models;

// Create engine
var engine = new CombatEngine();

// Initialize battle
var initData = new BattleInitData
{
    MaxSlotsPerTurn = 3,
    InitialHandSize = 5
};
initData.InitialPlayerState.HP = 100;
initData.InitialPlayerState.MaxHP = 100;
initData.InitialPlayerState.Energy = 10;
initData.InitialPlayerState.MaxEnergy = 10;
// ... populate decks

engine.Initialize(initData);

// Play cards
var snapshot = engine.GetSnapshot();
var cardId = snapshot.Player.Hand[0].InstanceId;
var result = engine.ResolveCard(new ResolveRequest(cardId, 0));

Console.WriteLine($"Outcome: {result.Outcome}");
Console.WriteLine($"Damage dealt: {result.DamageDealt}");
Console.WriteLine($"Player HP: {result.Snapshot.Player.HP}");

// Advance turn
var turnResult = engine.NextTurn();
Console.WriteLine($"Turn: {turnResult.Snapshot.TurnNumber}");
```

---

## Thread Safety

The engine is **not thread-safe**. Ensure:
- Single-threaded access per instance
- No concurrent method calls
- Use separate instances for parallel battles

---

## Versioning

The API follows semantic versioning. Breaking changes increment major version.

**Current Version**: 1.0.0
