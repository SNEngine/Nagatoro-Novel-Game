using SNEngine;
using System;

namespace CoreGame.FightSystem.UI
{
    public interface IFightWindow : IResetable, IShowable, IHidden
    {
        event Action<PlayerAction> OnTurnExecuted;
        void SetData(IFightComponent fightComponentPlayer, IFightComponent fightComponentEnemy);
    }
}