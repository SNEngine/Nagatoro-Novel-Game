using UnityEngine;
using System.Collections;
using CoreGame.FightSystem.HealthSystem.Models;
namespace CoreGame.FightSystem.HealthSystem
{
    public class HealthComponent : MonoBehaviour
    {

        private Health _healthModel;

        public Health HealthModel => _healthModel;


        public void SetData(float initialMaxHealth)
        {
            _healthModel = new Health(initialMaxHealth);

            _healthModel.OnHealthChanged += HandleHealthChanged;
            _healthModel.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            _healthModel.OnHealthChanged -= HandleHealthChanged;
            _healthModel.OnDied -= HandleDied;
        }


        public void TakeDamage(float damage)
        {
            _healthModel.TakeDamage(damage);
        }

        public void Heal(float amount)
        {
            _healthModel.Heal(amount);
        }


        private void HandleHealthChanged(float current, float max)
        {

            Debug.Log($"{gameObject.name} Health: {current} / {max}");
        }

        private void HandleDied()
        {
            _healthModel.OnHealthChanged -= HandleHealthChanged;
            _healthModel.OnDied -= HandleDied;
            Debug.Log($"{gameObject.name} has died!");
        }

    }
}