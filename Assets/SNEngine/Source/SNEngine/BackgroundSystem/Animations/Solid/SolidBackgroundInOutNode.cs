using Cysharp.Threading.Tasks;
using DG.Tweening;
using SNEngine.Animations;
using SNEngine.BackgroundSystem.AsyncNodes;
using SNEngine.Services;
using XNode;

namespace SNEngine.BackgroundSystem.Animations.Solid
{
    public class SolidBackgroundInOutNode : AsyncBackgroundInOutNode
    {
        protected override void Play(float duration, AnimationBehaviourType type, Ease ease)
        {
            Solid(type, duration, ease).Forget();
        }

        private async UniTask Solid(AnimationBehaviourType animationBehaviour, float duration, Ease ease)
        {
            var backgroundService = NovelGame.Instance.GetService<BackgroundService>();

            await backgroundService.Solid(duration, animationBehaviour, ease);

            StopTask();
        }
    }
}