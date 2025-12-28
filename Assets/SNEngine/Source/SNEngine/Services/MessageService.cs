using System.Linq;
using SNEngine.DialogOnScreenSystem;
using SNEngine.DialogSystem;
using SNEngine.Graphs;
using SNEngine.Services;
using SNEngine.Source.SNEngine.MessageSystem;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SNEngine.Source.SNEngine.Services
{
    [CreateAssetMenu(menuName = "SNEngine/Services/MessageService")]
    public class MessageService : ServiceBase, IResetable
    {
        private DialogueService _dialogueService;
        private IMessageWindow _window;
        private const string WINDOW_VANILLA_PATH = "UI/MessageWindow";

        private IDialogue _currentDialogue;

        public override void Initialize()
        {
            _dialogueService = NovelGame.Instance.GetService<DialogueService>();
            _dialogueService.OnStartDialogue += StartDialogHandler;
            _dialogueService.OnEndDialogue += OnEndDialogHandler;

            var prefabGO = Resources.Load<GameObject>(WINDOW_VANILLA_PATH);
            if (prefabGO == null)
            {
                Debug.LogError($"MessageOnScreenWindow prefab not found at Resources/{WINDOW_VANILLA_PATH}");
                return;
            }

            var instance = Object.Instantiate(prefabGO);
            instance.name = prefabGO.name;

            var windowComponent = instance.GetComponent<MessageWindow>();
            if (windowComponent == null)
            {
                Debug.LogError("MessageOnScreenWindow component missing on prefab!");
                return;
            }

            _window = windowComponent;

            DontDestroyOnLoad(instance);

            NovelGame.Instance.GetService<UIService>()
                .AddElementToUIContainer(instance);

            instance.SetActive(false);

            ResetState();
        }

        private void StartDialogHandler(IDialogue dialogue)
        {
            _currentDialogue = dialogue;

            var graph = _currentDialogue as DialogueGraph;
            if (graph == null) return;

            var printerNodes = graph.AllNodes.Values.OfType<PrinterTextNode>();

            foreach (var node in printerNodes)
            {
                node.OnMessage += OnMessageSubscribe;
            }
        }

        private void OnEndDialogHandler(IDialogue dialogue)
        {
            var graph = _currentDialogue as DialogueGraph;
            if (graph == null) return;

            var printerNodes = graph.AllNodes.Values.OfType<PrinterTextNode>();

            foreach (var node in printerNodes)
            {
                node.OnMessage -= OnMessageSubscribe;
            }
        }

        private void OnMessageSubscribe(IPrinterNode node)
        {
            var dialogNode = node as IDialogOnScreenNode;
            if (dialogNode == null) return;
            
            ShowMessage(dialogNode);
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

            var windowGO = ((MessageWindow)_window).gameObject;
            windowGO.SetActive(true);

            _window.SetData(dialog);
            _window.StartOutputDialog();
        }
    }
}