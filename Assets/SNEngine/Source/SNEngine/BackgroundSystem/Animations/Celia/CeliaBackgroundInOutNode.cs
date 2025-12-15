using Cysharp.Threading.Tasks;
using DG.Tweening;
using SNEngine.Animations;
using SNEngine.BackgroundSystem.AsyncNodes;
using SNEngine.Services;

namespace SNEngine.BackgroundSystem.Animations.Celia
{
    public class CeliaBackgroundInOutNode : AsyncBackgroundInOutNode
    {
        protected override void Play(float duration, AnimationBehaviourType type, Ease ease)
        {
            Celia(type, duration, ease).Forget();
        }

        private async UniTask Celia(AnimationBehaviourType animationBehaviour, float duration, Ease ease)
        {
            var backgroundService = NovelGame.Instance.GetService<BackgroundService>();

            await backgroundService.Celia(duration, animationBehaviour, ease);

            StopTask();
        }
    }
}