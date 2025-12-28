using SNEngine.DialogOnScreenSystem;
using TMPro;

namespace SNEngine.Source.SNEngine.MessageSystem
{
    public interface IMessageWindow 
    {
        void SetData(IDialogOnScreenNode dialog);
        void StartOutputDialog();
        
        void ResetState();
        void SetFontDialog(TMP_FontAsset font);
    }
}