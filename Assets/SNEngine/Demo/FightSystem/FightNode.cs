using SiphoinUnityHelpers.XNodeExtensions.AsyncNodes;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SNEngine.SaveSystem;
namespace CoreGame.FightSystem
{
    public class FightNode : AsyncNode, ISaveProgressNode
    {
        [SerializeField] private FightCharacter _playerCharacter;
        [SerializeField] private FightCharacter _enemyCharacter;
        private bool _finished;
        public override async void Execute()
        {
            if (!_playerCharacter)
            {
                Debug.LogError($"fight player character not seted to node {GUID}");
                return;
            }

            if (!_enemyCharacter)
            {
                Debug.LogError($"fight enemy character not seted to node {GUID}");
                return;
            }
            base.Execute();
        }

        public override bool CanSkip()
        {
            return _finished;
        }

        public object GetDataForSave()
        {
            return null;
        }

        public void ResetSaveBehaviour()
        {
            _finished = false;
        }

        public void SetDataFromSave(object data)
        {
            
        }
    }
}