using UnityEngine;

namespace SNEngine.DialogSystem
{
    public interface IOldRenderDialogue
    {
        Texture2D UpdateRender();

        void DisplayFrame(Texture2D frameTexture);
        void HideFrame();
    }
}