using Cysharp.Threading.Tasks;
using DG.Tweening;
using SNEngine.Animations;
using SNEngine.BackgroundSystem.AsyncNodes;
using SNEngine.Services;
using XNode;

namespace SNEngine.BackgroundSystem.Animations.Illumination
{
    public class IlluminationBackgroundInOutNode : AsyncBackgroundInOutNode
    {
        protected override void Play(float duration, AnimationBehaviourType type, Ease ease)
        {
            Illuminate(type, duration, ease).Forget();
        }

        private async UniTask Illuminate(AnimationBehaviourType animationBehaviour, float duration, Ease ease)
        {
            var backgroundService = NovelGame.Instance.GetService<BackgroundService>();

            await backgroundService.Illuminate(duration, animationBehaviour, ease);

            StopTask();
        }
    }
}