using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

namespace SNEngine.InputWindowSystem
{
    public interface IInputWindow : IResetable, IShowable, IHidden
    {
        void SetData(string keyTitle, Sprite icon);
         UniTask<InputWindowResult> WaitInputPlayer();
    }
}