# RSP_Core Build & Usage Guide

## Overview

This guide explains how to build RSP_Core and integrate it into your project (Unity, console app, or test project).

**Library**: RSP_Core  
**Target Framework**: .NET Standard 2.1  
**Output**: `RSP_Core.dll`  
**Compatibility**: Unity 6.x, .NET 5+, .NET Framework 4.7.2+

---

## Prerequisites

### Required
- .NET SDK 6.0 or later (for building)
- For Unity: Unity 2021.2 or later

### Verification
```bash
dotnet --version
# Should output 6.0.x or higher
```

---

## Building the Core Library

### Step 1: Clone Repository
```bash
git clone https://github.com/BaekNothing/RSP_Core.git
cd RSP_Core
```

### Step 2: Build Debug Version
```bash
dotnet build src/RSP_Core/RSP_Core.csproj
```

**Output**: `src/RSP_Core/bin/Debug/netstandard2.1/RSP_Core.dll`

### Step 3: Build Release Version
```bash
dotnet build src/RSP_Core/RSP_Core.csproj -c Release
```

**Output**: `src/RSP_Core/bin/Release/netstandard2.1/RSP_Core.dll`

### Build Verification
```bash
ls -lh src/RSP_Core/bin/Release/netstandard2.1/RSP_Core.dll
# Should show ~22KB file
```

---

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Tests with Verbosity
```bash
dotnet test --verbosity normal
```

### Expected Output
```
Passed!  - Failed:     0, Passed:    26, Skipped:     0, Total:    26
```

---

## Integration: Unity 6.x

### Step 1: Build DLL
Build the Release version as described above.

### Step 2: Copy DLL to Unity
Copy `RSP_Core.dll` to your Unity project:
```
<UnityProject>/Assets/Plugins/RSP_Core.dll
```

**Important**: Use the `Plugins` folder for .NET Standard DLLs.

### Step 3: Verify Import
In Unity:
1. Select `RSP_Core.dll` in Project window
2. Check Inspector: "Plugin platforms" should include "Editor" and "Standalone"
3. "API Compatibility Level" must be ".NET Standard 2.1" (Project Settings → Player)

### Step 4: Use in Unity Scripts
```csharp
using RSP_Core.Core;
using RSP_Core.Models;
using UnityEngine;

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

        // Add cards to player deck
        var attackCard = new CardDefinition
        {
            CardId = "strike",
            Name = "Strike",
            SymbolType = SymbolType.Square,
            CardRole = CardRole.Attack,
            Cost = 2,
            BaseValue = 10
        };
        attackCard.BaseEffects.Add(new EffectRef("attack_damage", 10));
        attackCard.WinEffects.Add(new EffectRef("attack_bonus_damage", 5));

        for (int i = 0; i < 10; i++)
        {
            initData.InitialPlayerState.Deck.Add(
                new CardInstance($"card_{i}", attackCard)
            );
        }

        // Setup enemy
        initData.InitialEnemyState.HP = 80;
        initData.InitialEnemyState.MaxHP = 80;
        initData.InitialEnemyState.AttackValue = 10;

        for (int i = 0; i < 5; i++)
        {
            initData.InitialEnemyState.Deck.Add(
                new EnemyCard($"enemy_{i}", (SymbolType)(i % 3), 8)
            );
        }

        engine.Initialize(initData);
        Debug.Log("Battle initialized!");
    }

    public void PlayCard(string cardInstanceId)
    {
        var request = new ResolveRequest(cardInstanceId, 0);
        var result = engine.ResolveCard(request);

        Debug.Log($"Outcome: {result.Outcome}");
        Debug.Log($"Damage dealt: {result.DamageDealt}");
        Debug.Log($"Player HP: {result.Snapshot.Player.HP}");

        // Map EventTags to VFX/SFX
        foreach (var tag in result.EventTags)
        {
            if (tag == "EnemyNegated")
            {
                // Play critical hit VFX
            }
            else if (tag == "EnemyAttacked")
            {
                // Play damage taken VFX
            }
        }
    }

    public void EndTurn()
    {
        var result = engine.NextTurn();
        Debug.Log($"Turn {result.Snapshot.TurnNumber} started");
    }
}
```

### Common Unity Issues

**Issue**: "Assembly not found" error  
**Solution**: Ensure DLL is in `Assets/Plugins` and API Compatibility is ".NET Standard 2.1"

**Issue**: "Method not found" error  
**Solution**: Clean and rebuild Unity project (Assets → Reimport All)

**Issue**: Inspector shows "Incompatible"  
**Solution**: Check that DLL targets `netstandard2.1` (not net6.0/net8.0)

---

## Integration: Console Application

### Step 1: Create Console Project
```bash
dotnet new console -n MyCardGame
cd MyCardGame
```

### Step 2: Add DLL Reference
```bash
dotnet add reference ../RSP_Core/src/RSP_Core/RSP_Core.csproj
```

Or copy DLL and reference directly:
```xml
<ItemGroup>
  <Reference Include="RSP_Core">
    <HintPath>../RSP_Core.dll</HintPath>
  </Reference>
</ItemGroup>
```

### Step 3: Example Program
```csharp
using RSP_Core.Core;
using RSP_Core.Models;
using System;

class Program
{
    static void Main()
    {
        var engine = new CombatEngine(new DefaultRandomSource(42));

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
        var strikeCard = new CardDefinition
        {
            CardId = "strike",
            Name = "Strike",
            SymbolType = SymbolType.Square,
            CardRole = CardRole.Attack,
            Cost = 2,
            BaseValue = 12
        };
        strikeCard.BaseEffects.Add(new EffectRef("attack_damage", 12));
        strikeCard.WinEffects.Add(new EffectRef("attack_bonus_damage", 6));

        for (int i = 0; i < 10; i++)
        {
            initData.InitialPlayerState.Deck.Add(
                new CardInstance($"inst_{i}", strikeCard)
            );
        }

        // Setup enemy
        initData.InitialEnemyState.HP = 80;
        initData.InitialEnemyState.MaxHP = 80;
        initData.InitialEnemyState.AttackValue = 10;

        for (int i = 0; i < 5; i++)
        {
            initData.InitialEnemyState.Deck.Add(
                new EnemyCard($"enemy_{i}", (SymbolType)(i % 3), 10)
            );
        }

        engine.Initialize(initData);

        // Game loop
        while (true)
        {
            var snapshot = engine.GetSnapshot();
            Console.WriteLine($"\n=== Turn {snapshot.TurnNumber} ===");
            Console.WriteLine($"Player: {snapshot.Player.HP} HP | {snapshot.Player.Energy} Energy");
            Console.WriteLine($"Enemy: {snapshot.Enemy.HP} HP");
            Console.WriteLine($"Hand: {snapshot.Player.Hand.Count} cards");

            if (snapshot.Player.HP <= 0)
            {
                Console.WriteLine("DEFEAT!");
                break;
            }

            if (snapshot.Enemy.HP <= 0)
            {
                Console.WriteLine("VICTORY!");
                break;
            }

            // Play first card in hand
            if (snapshot.Player.Hand.Count > 0)
            {
                var card = snapshot.Player.Hand[0];
                if (snapshot.Player.Energy >= card.Definition.Cost)
                {
                    Console.WriteLine($"\nPlaying: {card.Definition.Name}");
                    var result = engine.ResolveCard(
                        new ResolveRequest(card.InstanceId, snapshot.SlotIndex)
                    );

                    Console.WriteLine($"Result: {result.Outcome}");
                    Console.WriteLine($"Damage: {result.DamageDealt} dealt, {result.DamageTaken} taken");
                }
            }

            // End turn after playing cards
            if (snapshot.SlotIndex >= snapshot.MaxSlotsPerTurn - 1)
            {
                engine.NextTurn();
            }
        }
    }
}
```

### Step 4: Run
```bash
dotnet run
```

---

## Integration: Test Project

### Step 1: Create Test Project
```bash
dotnet new xunit -n RSP_Core.Tests
cd RSP_Core.Tests
```

### Step 2: Add Reference
```bash
dotnet add reference ../src/RSP_Core/RSP_Core.csproj
```

### Step 3: Example Test
```csharp
using Xunit;
using RSP_Core.Core;
using RSP_Core.Models;
using RSP_Core.Systems;

public class SymbolMatchupTests
{
    [Theory]
    [InlineData(SymbolType.Square, SymbolType.Triangle, CombatOutcome.Win)]
    [InlineData(SymbolType.Triangle, SymbolType.Circle, CombatOutcome.Win)]
    [InlineData(SymbolType.Circle, SymbolType.Square, CombatOutcome.Win)]
    public void SymbolMatchup_WinConditions(SymbolType player, SymbolType enemy, CombatOutcome expected)
    {
        var result = SymbolMatchup.DetermineOutcome(player, enemy);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CombatEngine_Initialize_SetsUpCorrectly()
    {
        var engine = new CombatEngine(new DefaultRandomSource(42));
        var initData = new BattleInitData
        {
            MaxSlotsPerTurn = 3,
            InitialHandSize = 5
        };

        initData.InitialPlayerState.HP = 100;
        initData.InitialPlayerState.MaxHP = 100;
        initData.InitialPlayerState.Energy = 10;
        initData.InitialPlayerState.MaxEnergy = 10;

        engine.Initialize(initData);
        var snapshot = engine.GetSnapshot();

        Assert.Equal(1, snapshot.TurnNumber);
        Assert.Equal(0, snapshot.SlotIndex);
        Assert.Equal(10, snapshot.Player.Energy);
    }
}
```

### Step 4: Run Tests
```bash
dotnet test
```

---

## Minimal Code Example

This is the absolute minimum to run a combat:

```csharp
using RSP_Core.Core;
using RSP_Core.Models;

// Create engine
var engine = new CombatEngine();

// Setup init data
var initData = new BattleInitData();
initData.InitialPlayerState.HP = 100;
initData.InitialPlayerState.MaxHP = 100;
initData.InitialPlayerState.Energy = 10;
initData.InitialPlayerState.MaxEnergy = 10;

// Add a card
var card = new CardDefinition
{
    CardId = "test",
    Name = "Test",
    SymbolType = SymbolType.Square,
    Cost = 2,
    BaseValue = 10
};
card.BaseEffects.Add(new EffectRef("attack_damage", 10));

initData.InitialPlayerState.Deck.Add(new CardInstance("inst_0", card));

// Add enemy
initData.InitialEnemyState.HP = 50;
initData.InitialEnemyState.MaxHP = 50;
initData.InitialEnemyState.Deck.Add(new EnemyCard("e1", SymbolType.Triangle, 5));

// Initialize
engine.Initialize(initData);

// Play card
var snapshot = engine.GetSnapshot();
var result = engine.ResolveCard(new ResolveRequest("inst_0", 0));

// Check result
Console.WriteLine($"Enemy HP: {result.Snapshot.Enemy.HP}");  // 40 (50 - 10)
Console.WriteLine($"Outcome: {result.Outcome}");             // Win
```

---

## Registering Custom Effects

Add custom effects before `Initialize`:

```csharp
var engine = new CombatEngine();

// Register lifesteal effect
engine.RegisterEffect("lifesteal", ctx =>
{
    int damage = ctx.Value;
    ctx.Snapshot.Enemy.HP -= damage;
    ctx.Snapshot.Player.HP += damage / 2;
});

// Use in card
var card = new CardDefinition { /* ... */ };
card.BaseEffects.Add(new EffectRef("lifesteal", 10));
```

---

## Target Framework Explanation

### .NET Standard 2.1
RSP_Core targets **.NET Standard 2.1**, which is compatible with:
- Unity 2021.2+ (via Mono or IL2CPP)
- .NET 5, 6, 7, 8, 9+
- .NET Framework 4.7.2+

### Why .NET Standard 2.1?
- **Unity Compatibility**: Unity 6.x uses .NET Standard 2.1 as the base API surface
- **No Unity Dependencies**: Pure C# code, no UnityEngine references
- **Maximum Compatibility**: Works across Unity, desktop, mobile, web platforms

### Build Requirements
- You need .NET SDK 6+ to **build** the library
- But the **output DLL** runs on .NET Standard 2.1 runtimes (including Unity)

---

## Troubleshooting

### "Could not load file or assembly"
- Ensure DLL path is correct
- Check that DLL targets `netstandard2.1`
- Verify API Compatibility Level in Unity (must be .NET Standard 2.1 or higher)

### "Type or namespace could not be found"
- Add `using RSP_Core.Core;` and `using RSP_Core.Models;`
- Check that DLL reference is added to project

### Tests fail to build
- Update test project to net8.0 or later
- Ensure test project references core project correctly

### Unity shows "Incompatible with current platform"
- Check Platform settings in Inspector for DLL
- Enable "Any Platform" or specific platforms needed
- Reimport DLL after changes

---

## Performance Considerations

### Memory Allocation
- `GetSnapshot()` creates a deep copy (allocates memory)
- Minimize calls per frame in hot paths
- Consider caching snapshots if UI only updates occasionally

### Deterministic Replay
For replay systems:
```csharp
// Record seed and player inputs
int seed = 12345;
var engine = new CombatEngine(new DefaultRandomSource(seed));

// ... record each ResolveCard(cardId, slot) call

// Replay by replaying inputs with same seed
```

---

## Sample Projects

The repository includes a console runner sample:
```bash
dotnet run --project samples/CoreCombat.ConsoleRunner/CoreCombat.ConsoleRunner.csproj
```

This demonstrates a full combat simulation with diverse card types.

---

## Further Resources

- **SPEC_CoreCombat.md**: Formal engine specification
- **SPEC_API.md**: Complete API reference
- **README.md**: Project overview and features

For questions or issues, visit the GitHub repository: https://github.com/BaekNothing/RSP_Core
