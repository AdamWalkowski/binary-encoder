using System;
using System.Text;
using Header = System.Collections.Generic.KeyValuePair<string, string>;

namespace Sinch.Encoding.Tests
{
    public class MessageEncoderSpecs
    {
        private MessageEncoder _encoder;
        public MessageEncoderSpecs()
        {
            _encoder = new MessageEncoder();
        }

        [Fact]
        public void Should_encode_simple_payload_without_headers()
        {
            var encodedMessage = _encoder.Encode(new SignalMessage
            {
                Payload = System.Text.Encoding.UTF8.GetBytes("abc")
            });

            var decoder = System.Text.Encoding.UTF8.GetDecoder();
            // check bytes
            Assert.Equal(4, encodedMessage.Length);
            Assert.Equal(MessageEncoder.PayloadSeparator + "abc", System.Text.Encoding.Default.GetString(encodedMessage));
        }

        [Fact]
        public void Should_encode_simple_payload_with_headers()
        {
            var encodedMessage = _encoder.Encode(new SignalMessage
            {
                Headers = new Dictionary<string, string>(new Header[]
                {
                    new Header("Alpha", "Beta"),
                    new Header("Kappa", "Omega")
                }),
                Payload = System.Text.Encoding.UTF8.GetBytes("Some Text")
            });

            byte[] headPrefix = new ArraySegment<byte>(encodedMessage, 0, 4).ToArray();
            int messageLength = BitConverter.ToInt32(headPrefix, 0);
            byte[] messageStream = new ArraySegment<byte>(encodedMessage, headPrefix.Length, messageLength).ToArray();

            Assert.Equal(34, encodedMessage.Length);
            Assert.Equal(30, messageLength);
            Assert.Equal("AlphaBetaKappaOmegaSome Text", System.Text.Encoding.Default.GetString(messageStream));
        }

        //[Fact]
        //public void Should_throw_error_when_encoding_message_without_payload()
        //{
        //    Assert.Throws<NotImplementedException>(() =>
        //    {
        //        var encodedMessage = _encoder.Encode(new SignalMessage
        //        {
        //            Headers = new Dictionary<string, string>(new Header[]
        //            {
        //                new Header("N", "V")
        //            })
        //        });
        //    });
        //}

        [Fact]
        public void Should_decode_simple_stream()
        {
            // given
            string input = "Alpha~Beta}Kappa~Omega|Some Text";
            byte[] stram = input.Select(x => Convert.ToByte(x)).ToArray();

            // when
            var decodedMessage = _encoder.Decode(stram);

            // then
            Assert.Equal("Beta", decodedMessage.Headers.First().Value);
            Assert.Equal("Omega", decodedMessage.Headers.Last().Value);
            Assert.Equal("Some Text", new String(System.Text.Encoding.UTF8.GetChars(decodedMessage.Payload)));
        }
    }
}