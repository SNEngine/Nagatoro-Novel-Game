using Cysharp.Threading.Tasks;
using DG.Tweening;
using SNEngine.Animations;
using SNEngine.Services;
using SNEngine.BackgroundSystem.AsyncNodes;
using UnityEngine;

namespace SNEngine.BackgroundSystem.Animations.Dissolve
{
    public class DissolveBackgroundNode : DissolveBackgroundNodeBase
    {
        protected override void Play(float duration, AnimationBehaviourType type, Ease ease, Texture2D texture)
        {
            Dissolve(duration, type, ease, texture).Forget();
        }

        private async UniTask Dissolve(float duration, AnimationBehaviourType type, Ease ease, Texture2D texture)
        {
            var backgroundService = NovelGame.Instance.GetService<BackgroundService>();

            await backgroundService.Dissolve(duration, type, ease, texture);

            StopTask();
        }
    }
}