#nullable enable
namespace DevToys.Messages
{
    public sealed class ChangeInfoBarStatusMessage
    {
        public string Message { get; set; }
        public bool IsOpen { get; set; }

        public ChangeInfoBarStatusMessage(bool isOpen, string message)
        {
            Message = message;
            IsOpen = isOpen;
        }
    }
}
