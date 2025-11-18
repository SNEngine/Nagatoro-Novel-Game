using CoreGame.FightSystem;
using CoreGame.FightSystem.Models;
using System.Collections.Generic;

namespace CoreGame.FightSystem.AI
{
    public class AIFighter
    {
        private FightCharacter _self;
        private FightCharacter _target;
        private IFightComponent _selfComponent;
        private IFightComponent _targetComponent;

        public AIFighter(FightCharacter self, IFightComponent selfComponent,
                         FightCharacter target, IFightComponent targetComponent)
        {
            _self = self;
            _target = target;
            _selfComponent = selfComponent;
            _targetComponent = targetComponent;
        }

        public PlayerAction DecideAction()
        {
            return ChooseBasicAction();
        }

        private PlayerAction ChooseBasicAction()
        {
            float selfHealthRatio = _selfComponent.HealthComponent.CurrentHealth / _selfComponent.HealthComponent.MaxHealth;
            float selfMana = _selfComponent.ManaComponent.CurrentMana;
            float targetHealthRatio = _targetComponent.HealthComponent.CurrentHealth / _targetComponent.HealthComponent.MaxHealth;

            float selfDamage = _self.Damage;

            if (selfHealthRatio <= 0.3f)
            {
                return PlayerAction.Guard;
            }

            if (targetHealthRatio * _targetComponent.HealthComponent.MaxHealth <= selfDamage)
            {
                return PlayerAction.Attack;
            }

            int choice = UnityEngine.Random.Range(0, 3);

            if (choice == 0)
            {
                return PlayerAction.Attack;
            }
            else if (choice == 1)
            {
                return PlayerAction.Guard;
            }
            else
            {
                return PlayerAction.Wait;
            }
        }
    }
}