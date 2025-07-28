#nullable enable
namespace DevToys.Messages
{
    public sealed class ChangeNumberFormattingMessage
    {
        public bool IsFormatted { get; set; }

        public ChangeNumberFormattingMessage(bool formatted)
        {
            IsFormatted = formatted;
        }
    }
}
