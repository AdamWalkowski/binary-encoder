using Header = System.Collections.Generic.KeyValuePair<string, string>;

namespace Sinch.Encoding.Tests
{
    public class IngerationDecodingSpecs
    {
        private Random random;
        private SignalMessageCodec _encoder;
        public IngerationDecodingSpecs()
        {
            random = new Random(DateTime.Now.Millisecond);
            var builder = new ByteBufferBuilder();
            _encoder = new SignalMessageCodec(builder);
        }

        [Fact]
        public void Should_decode_simple_message()
        {
            // given
            var message = new SignalMessage
            {
                Headers = new Dictionary<string, string>(new Header[]
                {
                    new Header("Alpha", "Beta"),
                    new Header("Kappa", "Omega")
                }),
                Payload = System.Text.Encoding.UTF8.GetBytes("Some Text")
            };
            var encodedMessage = _encoder.Encode(message);

            // when
            var decodedMessage = _encoder.Decode(encodedMessage);

            // then
            Assert.Equal(message.Headers.First().Key, decodedMessage.Headers.First().Key);
            Assert.Equal(message.Headers.First().Value, decodedMessage.Headers.First().Value);
            Assert.Equal(message.Headers.Last().Key, decodedMessage.Headers.Last().Key);
            Assert.Equal(message.Headers.Last().Value, decodedMessage.Headers.Last().Value);
            Assert.Equal(
                new String(System.Text.Encoding.UTF8.GetChars(message.Payload)),
                new String(System.Text.Encoding.UTF8.GetChars(decodedMessage.Payload))
            );
        }

        [Fact]
        public void Should_decode_large_message()
        {
            // given
            var payloadBuffer = new byte[10240];
            random.NextBytes(payloadBuffer);
            var headers = Enumerable.Range(0, 48).Select(index => new Header($"Name {index}", $"Value {index}"));
            var message = new SignalMessage
            {
                Headers = new Dictionary<string, string>(headers),
                Payload = payloadBuffer
            };
            var encodedMessage = _encoder.Encode(message);

            // when
            var decodedMessage = _encoder.Decode(encodedMessage);

            // then
            Assert.Equal(message.Headers.Count(), decodedMessage.Headers.Count());
            Assert.NotStrictEqual(message.Headers, decodedMessage.Headers);
            Assert.Equal(10240, decodedMessage.Payload.Length);
        }
    }
}
