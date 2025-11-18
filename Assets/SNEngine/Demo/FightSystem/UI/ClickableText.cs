using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using SiphoinUnityHelpers.XNodeExtensions.Attributes;

namespace CoreGame.FightSystem.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ClickableText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public delegate void ClickAction();
        public event ClickAction OnClick;

        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _hoverColor = Color.yellow;
        [SerializeField] private float _transitionDuration = 0.1f;
        [SerializeField] private float _hoverScale = 1.05f;

        [SerializeField, ReadOnly(ReadOnlyMode.Always)]   private TextMeshProUGUI _textComponent;
        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _textComponent.color = _normalColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _textComponent.DOColor(_hoverColor, _transitionDuration);
            transform.DOScale(_originalScale * _hoverScale, _transitionDuration).SetEase(Ease.OutSine);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _textComponent.DOColor(_normalColor, _transitionDuration);
            transform.DOScale(_originalScale, _transitionDuration).SetEase(Ease.OutSine);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }

        public void AddListener(ClickAction action)
        {
            OnClick += action;
        }

        public void RemoveListener(ClickAction action)
        {
            OnClick -= action;
        }

        private void OnValidate()
        {
            if (!_textComponent)
            {
                _textComponent = GetComponent<TextMeshProUGUI>();
            }
        }
    }
}