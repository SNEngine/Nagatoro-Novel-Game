using FightSystem.Abilities;
using UnityEngine;

namespace CoreGame.FightSystem.Abilities
{
    [CreateAssetMenu(fileName = "SuccubusDrainAbility", menuName = "CoreGame/Fight System/Abilities/Succubus Drain")]
    public class SuccubusDrainAbility : ScriptableOverTurnAbility
    {
        [SerializeField, Range(0.01f, 1.0f)]
        private float _drainPercent = 0.05f;

        protected override void TurnTick(IFightComponent user, IFightComponent target)
        {
            float targetCurrentHealth = target.HealthComponent.CurrentHealth;
            float drainAmount = targetCurrentHealth * _drainPercent;

            if (drainAmount > 0)
            {
                target.HealthComponent.TakeDamage(drainAmount);
                user.HealthComponent.Heal(drainAmount);
            }
        }

        public override AbilityType GetAbilityType()
        {
            return AbilityType.Skill;
        }
    }
}