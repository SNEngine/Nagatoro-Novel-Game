using SiphoinUnityHelpers.XNodeExtensions.Attributes;
using SNEngine.Services;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SNEngine.PauseWindowSystem
{
    [RequireComponent(typeof(Button))]
    public class OpenPauseWindowButton : MonoBehaviour, IOpenPauseWindowButton
    {
        [SerializeField, ReadOnly] private Button _button;
        private void Awake()
        {
            _button.onClick.AddListener(Pause);
        }

        private void Pause()
        {
            NovelGame.Instance.GetService<InputService>().SetActiveInput(false);
            NovelGame.Instance.GetService<PauseWindowService>().Show();
        }

        private void OnValidate()
        {
            if (!_button)
            {
                _button = GetComponent<Button>();
            }
        }

        public void ResetState()
        {
            gameObject.SetActive(false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}