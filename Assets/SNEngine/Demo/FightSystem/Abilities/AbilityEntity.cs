using System;

namespace CoreGame.FightSystem.Abilities
{
    [Serializable]
    public class AbilityEntity
    {

        public ScriptableAbility ReferenceAbility { get; private set; }
        public int CurrentCooldown { get; set; }

        public void Turn () => ReferenceAbility?.Turn();

        public AbilityEntity(ScriptableAbility referenceAbility)
        {
            ReferenceAbility = referenceAbility;
            CurrentCooldown = 0;
        }
    }
}
