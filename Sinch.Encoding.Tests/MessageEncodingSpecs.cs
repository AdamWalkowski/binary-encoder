using System;
using System.Reflection.PortableExecutable;
using Header = System.Collections.Generic.KeyValuePair<string, string>;

namespace Sinch.Encoding.Tests
{
    public class MessageEncodingSpecs
    {
        private Random random;
        private SignalMessageCodec _encoder;
        public MessageEncodingSpecs()
        {
            random = new Random(DateTime.Now.Millisecond);
            var builder = new ByteBufferBuilder();
            _encoder = new SignalMessageCodec(builder);
        }

        [Fact]
        public void Should_encode_simple_payload_with_headers()
        {
            // given
            var message = new SignalMessage
            {
                Headers = new Dictionary<string, string>(new Header[]
                {
                    new Header("Alpha", "Beta"),
                    new Header("Kappa", "Omega")
                }),
                Payload = System.Text.Encoding.ASCII.GetBytes("Some Text")
            };

            // when
            var encodedMessage = _encoder.Encode(message);

            // then
            byte[] payloadOffset = new ArraySegment<byte>(encodedMessage, 0, 4).ToArray();
            int offset = BitConverter.ToInt32(payloadOffset);
            byte[] headPrefix = new ArraySegment<byte>(encodedMessage, 4, 2).ToArray();
            short headersCount = BitConverter.ToInt16(headPrefix);
            int messageLength = encodedMessage.Length - offset;
            byte[] messageStream = new ArraySegment<byte>(encodedMessage, offset, encodedMessage.Length - offset).ToArray();

            Assert.Equal(42, encodedMessage.Length);
            Assert.Equal(9, messageLength);
            Assert.Equal(33, offset);
            Assert.Equal(2, headersCount);
            Assert.Equal("Some Text", System.Text.Encoding.Default.GetString(messageStream));
        }

        [Fact]
        public void Should_encode_simple_payload_without_headers()
        {
            // given
            var payload = System.Text.Encoding.ASCII.GetBytes("abc");
            var message = new SignalMessage
            {
                Payload = payload
            };

            // when
            var encodedMessage = _encoder.Encode(message);

            // then
            Assert.Equal(sizeof(int) + payload.Length, encodedMessage.Length);
        }

        [Fact]
        public void Should_encode_simple_message_without_payload()
        {
            // given
            var message = new SignalMessage
            {
                Headers = new Dictionary<string, string>(new Header[]
                {
                    new Header("N1", "V1"),
                    new Header("N2", "V2"),
                    new Header("N3", "V3")
                }),
                Payload = null
            };

            // when
            var encodedMessage = _encoder.Encode(message);

            // then
            int expectedLength = sizeof(int) + sizeof(short) + 3 * (2 * sizeof(short) + 4);
            Assert.Equal(expectedLength, encodedMessage.Length);
        }

        [Fact]
        public void Should_throw_error_when_encoding_empty_message()
        {
            // given
            var emptyMessage = new SignalMessage
            {
                Headers = null,
                Payload = null
            };

            // when
            var encodingAction = () => _encoder.Encode(emptyMessage);

            // then
            Assert.Throws<InvalidDataException>(encodingAction);
        }

        [Fact]
        public void Should_throw_error_when_too_long_header_name()
        {
            // given
            var payload = new byte[256];
            random.NextBytes(payload);

            var veryLongBuffer = new byte[1200];
            random.NextBytes(veryLongBuffer);
            var tooLongHeaderName = new String(System.Text.Encoding.ASCII.GetChars(veryLongBuffer));

            var invalidMessage = new SignalMessage
            {
                Headers = new Dictionary<string, string>(new Header[]
                {
                    new Header(tooLongHeaderName, "value"),
                    new Header("valid Name", "valid Value")
                }),
                Payload = null
            };

            // when
            var encodingAction = () => _encoder.Encode(invalidMessage);

            // then
            Assert.Throws<InvalidDataException>(encodingAction);
        }

        [Fact]
        public void Should_throw_error_when_too_many_headers()
        {
            // given
            var tooManyHeaders = Enumerable.Range(0, 100).Select(index => new Header($"Name {index}", $"Value {index}"));
            var emptyMessage = new SignalMessage
            {
                Headers = new Dictionary<string, string>(tooManyHeaders),
                Payload = null
            };

            // when
            var encodingAction = () => _encoder.Encode(emptyMessage);

            // then
            Assert.Throws<InvalidDataException>(encodingAction);
        }
    }
}