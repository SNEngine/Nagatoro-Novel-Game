using Cysharp.Threading.Tasks;
using DG.Tweening;
using SNEngine.BackgroundSystem.AsyncNodes;
using SNEngine.Services;
using UnityEngine;

namespace SNEngine.BackgroundSystem.Animations.Solid
{
    public class SolidBackgroundNode : AsyncBackgroundNode
    {
        [Input(connectionType = ConnectionType.Override), Range(0, 1), SerializeField] private float value;

        protected override void Play(float duration, Ease ease)
        {
            float finalValue = value;

            var input = GetInputPort(nameof(value));

            if (input != null && input.Connection != null)
            {
                finalValue = GetDataFromPort<float>(nameof(value));
            }
            Solid(finalValue, duration, ease).Forget();
        }

        private async UniTask Solid(float value, float duration, Ease ease)
        {
            var backgroundService = NovelGame.Instance.GetService<BackgroundService>();

            await backgroundService.Solid(duration, value, ease);

            StopTask();
        }
    }
}