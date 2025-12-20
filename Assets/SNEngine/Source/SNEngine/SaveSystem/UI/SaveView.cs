using Cysharp.Threading.Tasks;
using SNEngine.ConfirmationWindowSystem;
using SNEngine.SaveSystem.Models;
using SNEngine.Services;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SNEngine.SaveSystem.UI
{
    public class SaveView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private TextMeshProUGUI _textNameSave;
        [SerializeField] private TextMeshProUGUI _textDateSave;
        [SerializeField] private Button _button;
        [SerializeField] private Button _deleteButton;
        private string _saveName;

        public event Action<string> OnSelect;

        private void OnEnable()
        {
            _button.onClick.AddListener(Select);

            if (_deleteButton != null)
            {
                _deleteButton.onClick.AddListener(DeleteSave);
                _deleteButton.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(Select);

            if (_deleteButton != null)
            {
                _deleteButton.onClick.RemoveListener(DeleteSave);
            }
        }

        private void Select()
        {
            OnSelect?.Invoke(_saveName);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_deleteButton != null)
            {
                _deleteButton.gameObject.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_deleteButton != null)
            {
                _deleteButton.gameObject.SetActive(false);
            }
        }

        private async void DeleteSave()
        {
            var confirmationService = NovelGame.Instance.GetService<ConfirmationWindowService>();

            confirmationService.SetData(
                "delete_save",
                "confirm_delete_save_message",
                null,
                ConfirmationWindowButtonType.YesNo,
                "Delete Save",
                string.Format("Are you sure you want to delete '{0}'?", _saveName)
            );
            confirmationService.Show();

            var result = await confirmationService.WaitForConfirmation();
            confirmationService.Hide();

            if (result.IsConfirmed)
            {
                var saveLoadService = NovelGame.Instance.GetService<SaveLoadService>();
                bool success = await saveLoadService.DeleteSave(_saveName);

                if (success)
                {
                    // Notify the parent to refresh the list
                    var parentListView = GetComponentInParent<SaveListView>();
                    if (parentListView != null)
                    {
                        parentListView.RefreshList();
                    }
                }
            }
        }

        public void SetData (PreloadSave data)
        {
            _rawImage.texture = data.PreviewTexture;
            _saveName = data.SaveName;
            _textNameSave.text = data.SaveName;
            _textDateSave.text = data.SaveData.DateSave.ToString("dd.mm:yyyy");
        }
    }
}