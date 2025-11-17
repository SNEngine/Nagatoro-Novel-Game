using CoreGame.FightSystem.ManaSystem.Models;
using UnityEngine;

namespace CoreGame.FightSystem.ManaSystem
{
    public class ManaComponent : MonoBehaviour
    {

        private Mana _manaModel;

        public Mana ManaModel => _manaModel;

        public void SetData (float initialMaxMana)
        {
            _manaModel = new Mana(initialMaxMana);

            _manaModel.OnManaChanged += HandleManaChanged;
        }

        private void OnDisable()
        {
            _manaModel.OnManaChanged -= HandleManaChanged;
        }

        public bool TrySpend(float cost)
        {
            return _manaModel.TrySpend(cost);
        }

        public void Restore(float amount)
        {
            _manaModel.Restore(amount);
        }

        private void HandleManaChanged(float current, float max)
        {
            Debug.Log($"{gameObject.name} Mana: {current} / {max}");
        }
    }
}