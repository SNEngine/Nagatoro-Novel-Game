using SNEngine.DialogOnScreenSystem;
using SNEngine.Services;
using SNEngine.Source.SNEngine.MessageSystem;
using TMPro;
using UnityEngine;

namespace SNEngine.Source.SNEngine.Services
{
    [CreateAssetMenu(menuName = "SNEngine/Services/Message Dialog OnScreen Service")]
    public class MessageDialogOnScreenService : ServiceBase, IResetable
    {
        private IMessageOnScreenWindow _window;

        // Путь к префабу в Resources
        private const string WINDOW_VANILLA_PATH = "UI/MessageOnScreenWindow";

        public override void Initialize()
        {
            // Загружаем именно GameObject-префаб
            var prefabGO = Resources.Load<GameObject>(WINDOW_VANILLA_PATH);
            if (prefabGO == null)
            {
                Debug.LogError($"MessageOnScreenWindow prefab not found at Resources/{WINDOW_VANILLA_PATH}");
                return;
            }

            // Инстанцируем объект
            var instance = Object.Instantiate(prefabGO);
            instance.name = prefabGO.name;

            // Получаем компонент окна
            var windowComponent = instance.GetComponent<MessageOnScreenWindow>();
            if (windowComponent == null)
            {
                Debug.LogError("MessageOnScreenWindow component missing on prefab!");
                return;
            }

            _window = windowComponent;

            // Окно должно жить между сценами
            Object.DontDestroyOnLoad(instance);

            // Добавляем в UI
            NovelGame.Instance.GetService<UIService>()
                .AddElementToUIContainer(instance);

            // Окно скрыто по умолчанию
            instance.SetActive(false);

            // Сброс состояния (очистка контейнера)
            ResetState();
        }

        public override void ResetState()
        {
            _window?.ResetState();
        }

        public void SetFontDialog(TMP_FontAsset font)
        {
            _window?.SetFontDialog(font);
        }

        public void ShowMessage(IDialogOnScreenNode dialog)
        {
            if (_window == null)
            {
                Debug.LogError("MessageOnScreenWindow is not initialized!");
                return;
            }

            // Включаем окно перед выводом
            var windowGO = ((MessageOnScreenWindow)_window).gameObject;
            windowGO.SetActive(true);

            _window.SetData(dialog);
            _window.StartOutputDialog();
        }
    }
}
