using Cysharp.Threading.Tasks;

namespace SNEngine.Source.SNEngine.MessageSystem
{
    public class MessagePrinterText : PrinterText
    {
        protected override void Awake()
        {
        }

        public override void Print(string message)
        {
            Writing(message).Forget();
        }

        protected override void StartOutputDialog(string message)
        {
            Writing(message).Forget();
        }

        protected override void End()
        {
        }

        public override void ResetState()
        {
        }

        public override void Hide()
        {
        }

        public override void Show()
        {
        }
    }
}