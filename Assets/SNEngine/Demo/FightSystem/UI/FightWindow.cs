using System;
using System.Collections;
using UnityEngine;
using CoreGame.FightSystem;
using DG.Tweening;

namespace CoreGame.FightSystem.UI
{
    public class FightWindow : MonoBehaviour, IFightWindow
    {
        [SerializeField] private FillSlider _healthPlayer;
        [SerializeField] private FillSlider _healthEnemy;
        [SerializeField] private ClickableText _attackButton;
        [SerializeField] private ClickableText _waitButton;
        [SerializeField] private ClickableText _guardButton;
        [SerializeField] private ClickableText _skillButton;
        [SerializeField] private RectTransform _panelAction;

        [SerializeField, Min(0)] private float _durationChangeHealth = 0.3f;
        [SerializeField] private Ease _easeHealthAnimation = Ease.InBounce;

        public event Action<PlayerAction> OnTurnExecuted;

        private void Awake()
        {
            _attackButton.AddListener(() => OnClickButtonAction(PlayerAction.Attack));
            _guardButton.AddListener(() => OnClickButtonAction(PlayerAction.Guard));
            _waitButton.AddListener(() => OnClickButtonAction(PlayerAction.Wait));

            _skillButton.AddListener(OnClickSkillButton);
        }

        private void OnClickButtonAction(PlayerAction action)
        {
            OnTurnExecuted?.Invoke(action);
        }

        private void OnClickSkillButton()
        {

        }

        public void ResetState()
        {
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void HidePanelAction ()
        {
            _panelAction.gameObject.SetActive(false);
        }

        public void ShowPanelAction()
        {
            _panelAction.gameObject.SetActive(true);
        }

        private void OnDisable()
        {

        }

        public void SetData(IFightComponent fightComponentPlayer, IFightComponent fightComponentEnemy)
        {
            fightComponentEnemy.HealthComponent.OnHealthChanged += OnHealthChangedEnemy;
            fightComponentPlayer.HealthComponent.OnHealthChanged += OnHealthChangedPlayer;
            _healthEnemy.MaxValue = fightComponentEnemy.HealthComponent.MaxHealth;
            _healthEnemy.SetValueSmoothly(fightComponentEnemy.HealthComponent.CurrentHealth, _durationChangeHealth, _easeHealthAnimation);
            _healthPlayer.SetValueSmoothly(fightComponentPlayer.HealthComponent.CurrentHealth, _durationChangeHealth, _easeHealthAnimation);

        }

        private void OnHealthChangedPlayer(float current, float max)
        {
            _healthPlayer.SetValueSmoothly(current, _durationChangeHealth, _easeHealthAnimation);
        }

        private void OnHealthChangedEnemy(float current, float max)
        {
            _healthEnemy.SetValueSmoothly(current, _durationChangeHealth, _easeHealthAnimation);
        }
    }
}