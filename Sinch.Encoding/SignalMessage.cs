namespace Sinch.Encoding
{
    public class SignalMessage
    {
        public Dictionary<string, string>? Headers { get; init; }
        public byte[]? Payload { get; init; }
    }
}
