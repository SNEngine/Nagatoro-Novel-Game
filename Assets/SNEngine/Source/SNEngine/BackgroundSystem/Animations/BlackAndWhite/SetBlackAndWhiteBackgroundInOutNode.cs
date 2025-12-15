using Cysharp.Threading.Tasks;
using DG.Tweening;
using SNEngine.Animations;
using SNEngine.BackgroundSystem.AsyncNodes;
using SNEngine.Services;
using XNode;

namespace SNEngine.BackgroundSystem.Animations.BlackAndWhite
{
    public class SetBlackAndWhiteBackgroundInOutNode : AsyncBackgroundInOutNode
    {
        protected override void Play(float duration, AnimationBehaviourType type, Ease ease)
        {
            BlackAndWhite(type, duration, ease).Forget();
        }

        private async UniTask BlackAndWhite(AnimationBehaviourType animationBehaviour, float duration, Ease ease)
        {
            var backgroundService = NovelGame.Instance.GetService<BackgroundService>();

            await backgroundService.ToBlackAndWhite(duration, animationBehaviour, ease);

            StopTask();
        }
    }
}