using RSP_Core.Models;

namespace RSP_Core.Core
{
    /// <summary>
    /// Main combat engine interface
    /// </summary>
    public interface ICombatEngine
    {
        void Initialize(BattleInitData initData);
        BattleSnapshot GetSnapshot();
        ResolveResult ResolveCard(ResolveRequest request);
        NextTurnResult NextTurn();
    }
}
