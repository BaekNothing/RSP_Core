using RSP_Core.Entities;
using System.Collections.Generic;

namespace RSP_Core.Battle
{
    /// <summary>
    /// Main battle controller that manages turn-based combat between two players
    /// </summary>
    public class BattleManager
    {
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public BattleState CurrentState { get; private set; }
        public int TurnCount { get; private set; }
        public List<BattleAction> ActionHistory { get; private set; }
        
        private Player currentPlayer;
        private Player opponent;

        public BattleManager(Player player1, Player player2)
        {
            Player1 = player1;
            Player2 = player2;
            CurrentState = BattleState.NotStarted;
            TurnCount = 0;
            ActionHistory = new List<BattleAction>();
        }

        /// <summary>
        /// Starts the battle and initializes both players
        /// </summary>
        public void StartBattle(int initialHandSize = 5)
        {
            if (CurrentState != BattleState.NotStarted)
            {
                return;
            }

            // Shuffle both decks
            Player1.Deck.Shuffle();
            Player2.Deck.Shuffle();

            // Draw initial hands
            Player1.DrawCards(initialHandSize);
            Player2.DrawCards(initialHandSize);

            // Player 1 starts
            currentPlayer = Player1;
            opponent = Player2;
            CurrentState = BattleState.PlayerTurn;
            TurnCount = 1;
        }

        /// <summary>
        /// Executes a card play action during battle
        /// </summary>
        public BattleAction PlayCard(Card card)
        {
            if (CurrentState != BattleState.PlayerTurn && CurrentState != BattleState.EnemyTurn)
            {
                return null;
            }

            if (!currentPlayer.CanPlayCard(card))
            {
                return null;
            }

            // Play the card
            currentPlayer.PlayCard(card);

            // Calculate damage
            int damage = card.Attack;
            opponent.TakeDamage(damage);

            string description = $"{currentPlayer.Name} played {card.Name} and dealt {damage} damage to {opponent.Name}";
            var action = new BattleAction(currentPlayer, opponent, card, damage, description);
            ActionHistory.Add(action);

            // Check for battle end
            CheckBattleEnd();

            return action;
        }

        /// <summary>
        /// Ends the current player's turn and switches to the next player
        /// </summary>
        public void EndTurn()
        {
            if (CurrentState != BattleState.PlayerTurn && CurrentState != BattleState.EnemyTurn)
            {
                return;
            }

            // Draw cards for next turn
            const int cardsPerTurn = 2;
            
            // Switch players
            var temp = currentPlayer;
            currentPlayer = opponent;
            opponent = temp;

            // Restore energy and draw cards for new active player
            currentPlayer.RestoreEnergy();
            currentPlayer.DrawCards(cardsPerTurn);

            // Update turn count and state
            if (currentPlayer == Player1)
            {
                TurnCount++;
                CurrentState = BattleState.PlayerTurn;
            }
            else
            {
                CurrentState = BattleState.EnemyTurn;
            }

            // Check for battle end
            CheckBattleEnd();
        }

        /// <summary>
        /// Checks if the battle has ended and updates state accordingly
        /// </summary>
        private void CheckBattleEnd()
        {
            if (!Player1.IsAlive && !Player2.IsAlive)
            {
                CurrentState = BattleState.Draw;
            }
            else if (!Player1.IsAlive)
            {
                CurrentState = BattleState.Defeat;
            }
            else if (!Player2.IsAlive)
            {
                CurrentState = BattleState.Victory;
            }
        }

        /// <summary>
        /// Gets the current active player
        /// </summary>
        public Player GetCurrentPlayer()
        {
            return currentPlayer;
        }

        /// <summary>
        /// Gets the opponent of the current player
        /// </summary>
        public Player GetOpponent()
        {
            return opponent;
        }

        /// <summary>
        /// Returns whether the battle is still ongoing
        /// </summary>
        public bool IsBattleActive()
        {
            return CurrentState == BattleState.PlayerTurn || CurrentState == BattleState.EnemyTurn;
        }

        /// <summary>
        /// Gets the winner of the battle, or null if battle is not ended
        /// </summary>
        public Player GetWinner()
        {
            if (CurrentState == BattleState.Victory)
            {
                return Player1;
            }
            else if (CurrentState == BattleState.Defeat)
            {
                return Player2;
            }
            return null;
        }
    }
}
