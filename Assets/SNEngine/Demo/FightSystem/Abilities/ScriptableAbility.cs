using CoreGame.Services;
using FightSystem.Abilities;
using SNEngine;
using UnityEngine;

namespace CoreGame.FightSystem.Abilities
{
    public abstract class ScriptableAbility : ScriptableObjectIdentity
    {
        [SerializeField, Min(0)] private int _cost = 1;
        [SerializeField, Min(0)] private float _duration;
        [SerializeField] private int _cooldown = 0;
        [SerializeField] private string _nameAbility;
        [SerializeField, TextArea] private string _descriptionAbility;

        public int Cost => _cost;
        public float Duration => _duration;
        public int Cooldown => _cooldown;
        public string NameAbility => _nameAbility;
        public string DescriptionAbility => _descriptionAbility;

        public void Turn ()
        {
            var fightService = NovelGame.Instance.GetService<FightService>();
            Execute(fightService.Player, fightService.Enemy);
        }

        protected abstract void Execute(IFightComponent player, IFightComponent enemy);

        public abstract AbilityType GetAbilityType();
    }
}
