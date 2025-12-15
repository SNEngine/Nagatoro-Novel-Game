using Cysharp.Threading.Tasks;
using SNEngine.Debugging;
using SNEngine.DialogSystem;
using SNEngine.Graphs;
using SNEngine.SaveSystem.Models;
using SNEngine.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace SNEngine.Services
{
    [CreateAssetMenu(menuName = "SNEngine/Services/Dialogue Service")]
    public class DialogueService : ServiceBase, IService
    {
        private IDialogue _currentDialogue;
        private IDialogue _startDialogue;
        private IOldRenderDialogue _oldRenderDialogueService;

        public event Action<IDialogue> OnEndDialogue;

        public IDialogue CurrentDialogue => _currentDialogue;

        public override void Initialize()
        {
            _oldRenderDialogueService = NovelGame.Instance.GetService<RenderOldDialogueService>();

            _startDialogue = Resources.Load<DialogueGraph>($"Dialogues/{nameof(_startDialogue)}");

        }

        public void JumpToStartDialogue()
        {
            JumpToDialogue(_startDialogue);
        }

        public void JumpToDialogue(IDialogue dialogue)
        {
            if (dialogue is null)
            {
                NovelGameDebug.LogError("dialogue argument is null. Check your graph");
            }

            _currentDialogue?.Stop();

            _currentDialogue = dialogue;

            _currentDialogue.OnEndExecute += OnEndExecute;

            NovelGameDebug.Log($"Jump To Dialogue: {_currentDialogue.Name}");

            _currentDialogue.Execute();
            NovelGame.Instance.GetService<OpenPauseWindowButtonService>().Show();
        }

        public void ToDialogue(SaveData saveData)
        {
            var dislogues = Resources.LoadAll<DialogueGraph>("Dialogues");
            var targetDialogue = dislogues.FirstOrDefault(x => x.GUID == saveData.DialogueGUID);

            if (targetDialogue != null)
            {
                _currentDialogue = targetDialogue;
                _currentDialogue.OnEndExecute += OnEndExecute;
                targetDialogue.LoadSave(saveData.CurrentNode, saveData);
                NovelGame.Instance.GetService<OpenPauseWindowButtonService>().Show();
            }

        }

        public void StopCurrentDialogue()
        {
            if (_currentDialogue != null)
            {
                _currentDialogue.OnEndExecute -= OnEndExecute;
                _currentDialogue.Stop();
                _currentDialogue = null;

            }
        }

        private void OnEndExecute()
        {
            _currentDialogue.OnEndExecute -= OnEndExecute;

            OnEndDialogue?.Invoke(_currentDialogue);

            ClearScreen();
        }


        private void ClearScreen()
        {
            Texture2D capturedFrame = _oldRenderDialogueService.UpdateRender();

            _oldRenderDialogueService.DisplayFrame(capturedFrame);

            NovelGame.Instance.ResetStateServices();

        }
    }
}