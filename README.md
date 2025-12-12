# RSP_Core

A standalone C# core engine for turn-based card game battle logic. This library is designed to be integrated into Unity projects as a DLL.

## Features

- **Card System**: Cards with attack, defense, cost, and description properties
- **Deck Management**: Shuffle, draw, and manage card decks
- **Player System**: Health, energy, hand management, and deck operations
- **Turn-Based Battle**: Complete battle manager for turn-based card combat
- **Battle State Management**: Track battle states (NotStarted, PlayerTurn, EnemyTurn, Victory, Defeat, Draw)
- **Action History**: Track all actions taken during battle

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

## Unity Integration

1. Build the project in Release mode
2. Copy `RSP_Core.dll` from `src/RSP_Core/bin/Release/netstandard2.1/` to your Unity project's `Assets/Plugins/` folder
3. Import the namespace in your Unity scripts:

```csharp
using RSP_Core.Entities;
using RSP_Core.Battle;
```

## Usage Example

```csharp
using RSP_Core.Entities;
using RSP_Core.Battle;

// Create players
var player1 = new Player("p1", "Player 1", 100, 10);
var player2 = new Player("p2", "Player 2", 100, 10);

// Add cards to player decks
for (int i = 0; i < 10; i++)
{
    player1.Deck.AddCard(new Card($"card{i}", $"Card {i}", i + 1, i, 2));
    player2.Deck.AddCard(new Card($"card{i}", $"Card {i}", i + 1, i, 2));
}

// Create battle manager
var battleManager = new BattleManager(player1, player2);

// Start battle (draws 5 initial cards for each player)
battleManager.StartBattle(5);

// Play a card
var card = player1.Hand[0];
var action = battleManager.PlayCard(card);

// End turn
battleManager.EndTurn();

// Check battle state
if (battleManager.CurrentState == BattleState.Victory)
{
    var winner = battleManager.GetWinner();
    Console.WriteLine($"{winner.Name} wins!");
}
```

## API Overview

### Entities

- **Card**: Represents a game card with properties and abilities
- **Deck**: Manages a collection of cards with shuffle and draw operations
- **Player**: Represents a player with health, energy, deck, hand, and discard pile

### Battle System

- **BattleManager**: Main controller for turn-based combat
- **BattleState**: Enum for different battle states
- **BattleAction**: Represents an action taken during battle

## Target Framework

- .NET Standard 2.1 (Compatible with Unity 2021.2+)

## License

See LICENSE file for details.
