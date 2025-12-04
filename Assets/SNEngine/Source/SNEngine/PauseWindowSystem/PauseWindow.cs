using SNEngine.Audio.UI.Services;
using SNEngine.SaveSystem;
using SNEngine.Services;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SNEngine.PauseWindowSystem
{
    public class PauseWindow : MonoBehaviour, IPauseWindow
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _loadSaveButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _backToMenuButton;

        private Button[] _buttons;

        private MainMenuService _mainMenuService;
        private InputService _inputService;
        private SettingsService _settingsService;
        private SaveListViewService _saveListViewService;
        private DialogueService _dialogueService;
        private float _currentTimeScale;

        private INovelGame NovelGameReference => NovelGame.Instance;

        private void OnEnable()
        {
            _currentTimeScale = Time.timeScale;
            Time.timeScale = 0;
            if (_buttons is null || _buttons.Length == 0)
            {
                _buttons = new Button[] 
                {
                    _resumeButton,
                    _settingsButton,
                    _loadSaveButton,
                    _saveButton,
                    _backToMenuButton
                };
            }
            _mainMenuService = NovelGameReference.GetService<MainMenuService>();
            _settingsService = NovelGameReference.GetService<SettingsService>();
            _inputService = NovelGameReference.GetService<InputService>();
            _saveListViewService = NovelGameReference.GetService<SaveListViewService>();
            _dialogueService = NovelGameReference.GetService<DialogueService>();

            foreach (var button in _buttons)
            {
                button.onClick.RemoveAllListeners();
            }

            _settingsButton.onClick.AddListener(OpenSettings);
            _loadSaveButton.onClick.AddListener(OpenSavesWindow);
            _backToMenuButton.onClick.AddListener(BackToMenu);
            _resumeButton.onClick.AddListener(() => gameObject.SetActive(false));

        }

        private void OnDisable()
        {
            Time.timeScale = _currentTimeScale;
        }

        private void BackToMenu()
        {
            _dialogueService.StopCurrentDialogue();
            NovelGameReference.ResetStateServices();
            _mainMenuService.Show();
            Hide();
        }

        private void OpenSavesWindow()
        {
            _saveListViewService.Show();
        }

        private void OpenSettings()
        {
            _settingsService.Show();
        }

        public void ResetState()
        {
            _settingsService.Hide();
            _saveListViewService.Hide();
            foreach (var button in _buttons)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}