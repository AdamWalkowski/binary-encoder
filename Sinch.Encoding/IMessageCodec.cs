namespace Sinch.Encoding
{
    public interface IMessageCodec<TMessage>
        where TMessage : class
    {
        byte[] Encode(TMessage message);
        TMessage Decode(byte[] stream);
    }
}
