# RSP_Core

A standalone C# **headless combat library** for symbol-based card game battle logic. This library is Unity-independent and designed to be integrated into Unity projects, console applications, or tests via DLL reference.

## Design Goals

- **Unity-Independent**: No UnityEngine references, pure C# .NET Standard 2.1
- **Headless Combat**: State-based engine with snapshot/request/result pattern
- **Effect System**: Extensible effect registry with support for external mod effects
- **Deterministic**: Dependency injection for random source enables replay and testing
- **Symbol-Based Combat**: Rock-paper-scissors style matchup system (Square > Triangle > Circle > Square)
- **Slot-Based Turns**: Multiple card plays per turn with clear resolution phases

## Core Mechanics

### Symbol System
Three symbols in a matchup cycle:
- **Square (□) > Triangle (△)**
- **Triangle (△) > Circle (○)**
- **Circle (○) > Square (□)**

### Combat Outcomes
- **Win (Critical)**: Player's base + win effects execute; enemy action negated
- **Draw (Normal)**: Player's base effects execute; enemy attacks
- **Lose (Failure)**: Player effects do NOT execute; enemy attacks

### Card Structure
Cards contain:
- **SymbolType**: Square/Triangle/Circle
- **CardRole**: Attack/Defense/Skill
- **Cost**, **BaseValue**, **WinBonusValue**
- **BaseEffects**: List of EffectRef (executed on Win/Draw)
- **WinEffects**: List of EffectRef (executed only on Win)

### Effect System
Effects are executed via **effectId** lookup:
- Built-in effects: `attack_damage`, `attack_bonus_damage`, `defense_percent`, `skill_draw`, `status_bleed`
- External effects can be registered: `engine.RegisterEffect(effectId, handler)`
- Missing effects are skipped with warning tags in results

## Building the DLL

To build the DLL for use in Unity:

```bash
dotnet build src/RSP_Core/RSP_Core.csproj -c Release
```

The DLL will be generated at: `src/RSP_Core/bin/Release/netstandard2.1/RSP_Core.dll`

## Running Tests

```bash
dotnet test
```

All tests include:
- Symbol matchup correctness
- Win/Draw/Lose combat resolution
- Effect execution and registration
- Energy management
- Enemy deck refill policy
- Defense calculation

## Unity Integration

### Step 1: Build and Copy DLL
1. Build the project in Release mode
2. Copy `RSP_Core.dll` from `src/RSP_Core/bin/Release/netstandard2.1/` to your Unity project's `Assets/Plugins/` folder

### Step 2: Import and Use
```csharp
using RSP_Core.Core;
using RSP_Core.Models;

public class BattleController : MonoBehaviour
{
    private ICombatEngine engine;
    
    void Start()
    {
        engine = new CombatEngine();
        InitializeBattle();
    }
    
    void InitializeBattle()
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
        var cardDef = new CardDefinition
        {
            CardId = "attack_1",
            Name = "Strike",
            SymbolType = SymbolType.Square,
            CardRole = CardRole.Attack,
            Cost = 2,
            BaseValue = 10
        };
        cardDef.BaseEffects.Add(new EffectRef("attack_damage", 10));
        cardDef.WinEffects.Add(new EffectRef("attack_bonus_damage", 5));
        
        for (int i = 0; i < 10; i++)
        {
            initData.InitialPlayerState.Deck.Add(
                new CardInstance($"inst_{i}", cardDef)
            );
        }
        
        // Setup enemy
        initData.InitialEnemyState.HP = 80;
        initData.InitialEnemyState.MaxHP = 80;
        initData.InitialEnemyState.AttackValue = 8;
        
        for (int i = 0; i < 5; i++)
        {
            initData.InitialEnemyState.Deck.Add(
                new EnemyCard($"enemy_{i}", (SymbolType)(i % 3), 10)
            );
        }
        
        engine.Initialize(initData);
    }
    
    public void PlayCard(string cardInstanceId, int slotIndex)
    {
        var request = new ResolveRequest(cardInstanceId, slotIndex);
        var result = engine.ResolveCard(request);
        
        // Use result for VFX/SFX
        Debug.Log($"Outcome: {result.Outcome}");
        Debug.Log($"Damage Dealt: {result.DamageDealt}");
        Debug.Log($"Damage Taken: {result.DamageTaken}");
        
        foreach (var tag in result.EventTags)
        {
            Debug.Log($"Event: {tag}");
            // Map tags to VFX/SFX: "EnemyNegated", "EnemyAttacked", etc.
        }
        
        // Update UI with new snapshot
        UpdateUI(result.Snapshot);
    }
    
    public void EndTurn()
    {
        var result = engine.NextTurn();
        UpdateUI(result.Snapshot);
    }
    
    void UpdateUI(BattleSnapshot snapshot)
    {
        // Update Unity UI with snapshot data
        // snapshot.Player.HP, snapshot.Enemy.HP, snapshot.Player.Hand, etc.
    }
}
```

## Usage Example (Console/Tests)

```csharp
using RSP_Core.Core;
using RSP_Core.Models;

// Create engine with deterministic random (for tests)
var engine = new CombatEngine(new DefaultRandomSource(42));

// Initialize battle
var initData = new BattleInitData();
// ... setup initData (see Unity example above)
engine.Initialize(initData);

// Get current state
var snapshot = engine.GetSnapshot();
Console.WriteLine($"Player HP: {snapshot.Player.HP}");
Console.WriteLine($"Hand: {snapshot.Player.Hand.Count} cards");

// Resolve a card
var cardId = snapshot.Player.Hand[0].InstanceId;
var result = engine.ResolveCard(new ResolveRequest(cardId, 0));

Console.WriteLine($"Combat: {result.PlayerSymbol} vs {result.EnemySymbol}");
Console.WriteLine($"Outcome: {result.Outcome}");
Console.WriteLine($"Damage: {result.DamageDealt} dealt, {result.DamageTaken} taken");

// Advance turn
var turnResult = engine.NextTurn();
Console.WriteLine($"Turn {turnResult.Snapshot.TurnNumber} started");
```

## Registering Custom Effects

```csharp
var engine = new CombatEngine();

// Register a custom heal effect
engine.RegisterEffect("custom_heal", ctx =>
{
    int healAmount = ctx.Value;
    ctx.Snapshot.Player.HP += healAmount;
    if (ctx.Snapshot.Player.HP > ctx.Snapshot.Player.MaxHP)
    {
        ctx.Snapshot.Player.HP = ctx.Snapshot.Player.MaxHP;
    }
});

// Use in card definition
var cardDef = new CardDefinition();
cardDef.BaseEffects.Add(new EffectRef("custom_heal", 15));
```

## API Reference

### Core Types

#### ICombatEngine
- `Initialize(BattleInitData)`: Setup battle with initial state
- `GetSnapshot()`: Get current battle state snapshot
- `ResolveCard(ResolveRequest)`: Execute card play in a slot
- `NextTurn()`: Advance to next turn (energy restore, draw cards)

#### Models
- `BattleSnapshot`: Complete state (player, enemy, turn, slot)
- `PlayerState`: HP, Energy, Deck, Hand, Discard, StatusEffects
- `EnemyState`: HP, Deck, Discard, StatusEffects
- `CardDefinition`: Static card data with effects
- `CardInstance`: Instance with upgrade data
- `ResolveResult`: Outcome, damage, event tags, updated snapshot
- `NextTurnResult`: Updated snapshot, event tags

### Built-in Effects

- **attack_damage**: Deal `value` damage to enemy
- **attack_bonus_damage**: Deal `value` damage to enemy (for win effects)
- **defense_percent**: Add `value`% defense for this turn (reduces enemy damage)
- **skill_draw**: Draw `value` cards
- **status_bleed**: Apply `value` stacks of bleed status to enemy

## Target Framework

- .NET Standard 2.1 (Compatible with Unity 2021.2+)

## Project Structure

```
RSP_Core/
├── src/RSP_Core/           # Core library
│   ├── Core/               # Engine and interfaces
│   ├── Models/             # Data models
│   ├── Effects/            # Effect system
│   └── Systems/            # Game systems
├── tests/RSP_Core.Tests/   # Test suite
└── RSP_Core.sln            # Solution file
```

## License

See LICENSE file for details.
