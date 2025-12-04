using UnityEngine.Events;
using UnityEngine;

namespace SNEngine.InputSystem
{
    public interface IInputSystem
    {
        void AddListener(UnityAction<KeyCode> action, StandaloneInputEventType eventType);

        void RemoveListener(UnityAction<KeyCode> action, StandaloneInputEventType eventType);

        void AddListener(UnityAction<Touch> action, MobileInputEventType eventType);

        void RemoveListener(UnityAction<Touch> action, MobileInputEventType eventType);

        void AddListener(UnityAction<KeyCode> action, GamepadButtonEventType eventType);

        void RemoveListener(UnityAction<KeyCode> action, GamepadButtonEventType eventType);

        void AddAxisListener(UnityAction<string, float> action, string axisName);

        void RemoveAxisListener(UnityAction<string, float> action);
        void SetActiveInput(bool status);
    }
}