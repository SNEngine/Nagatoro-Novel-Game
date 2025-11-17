using UnityEngine;
using UnityEngine.UI;
namespace CoreGame.FightSystem.UI
{

    [RequireComponent(typeof(Image))]
    public class FillSlider : MonoBehaviour
    {
        [SerializeField]
        private float _value;

        [SerializeField, Min(0)]
        private float _maxValue = 1;

        private Image _fillImage;

        public float Value
        {
            get => _value;
            set
            {
                _value = Mathf.Clamp(value, 0, _maxValue);
                UpdateFill();
            }
        }

        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = Mathf.Max(0, value);
                UpdateFill();
            }
        }

        private void Awake()
        {
            _fillImage = GetComponent<Image>();
            UpdateFill();
        }

        private void UpdateFill()
        {
            if (!_fillImage)
            {
                _fillImage = GetComponent<Image>();
            }
            _value = Mathf.Clamp(_value, 0, _maxValue);
            _fillImage.fillAmount = _value / _maxValue;


        }

        private void OnValidate()
        {
            UpdateFill();
        }
    }

}