using Header = System.Collections.Generic.KeyValuePair<string, string>;

namespace Sinch.Encoding
{
    public class SignalMessageCodec : IMessageCodec<SignalMessage>
    {
        private const int TEXT_MAX_LENGTH = 1023;
        private const int HEADERS_MAX_COUNT = 63;
        private const int PAYLOAD_MAX_SIZE = 256 * 1024;

        private ByteBufferBuilder _builder;

        public SignalMessageCodec(ByteBufferBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Encode(SignalMessage message)
        {
            if (message.Payload == null && message.Headers == null)
            {
                throw new InvalidDataException($"Message is empty, has no data nor metadata headers");
            }

            if (message.Payload != null && message.Payload.Length > PAYLOAD_MAX_SIZE)
            {
                throw new InvalidDataException($"Maximal payload size exceeded: {message.Payload.Length} > {PAYLOAD_MAX_SIZE} bytes");
            }

            try
            {
                var headersBuffer = EncodeHeaders(message.Headers);
                var headersDataLength = headersBuffer != null ? headersBuffer.Length : 0;

                var payloadLength = message.Payload != null ? message.Payload.Length : 0;
                byte[] stream = _builder.AllocateBuffer(sizeof(int), headersDataLength, payloadLength);
                byte[] payloadOffsetBytes = _builder.GetBytes(headersDataLength + sizeof(int));
                _builder.CopyBuffersIntoTarget(stream, payloadOffsetBytes, headersBuffer, message.Payload);

                return stream;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot encode signal message: {e.Message}");
                throw e;
            }
        }

        private byte[]? EncodeHeaders(Dictionary<string, string>? headers)
        {
            if (headers == null || headers.Count == 0)
            {
                return null;
            }

            var headersCount = (short)headers.Count;
            if (headers.Count > HEADERS_MAX_COUNT)
            {
                throw new InvalidDataException($"Maximal headers number exceeded: {headersCount} > {HEADERS_MAX_COUNT} bytes");
            }

            var tooLongKey = headers.Keys.FirstOrDefault(key => key.Length > TEXT_MAX_LENGTH);
            if (tooLongKey != null)
            {
                throw new InvalidDataException($"Header name starting with \"{tooLongKey[..10]}...\" exceeded {TEXT_MAX_LENGTH} charactes long");
            }

            var tooLongValue = headers.Values.FirstOrDefault(value => value.Length > TEXT_MAX_LENGTH);
            if (tooLongValue != null)
            {
                throw new InvalidDataException($"Header value starting with \"{tooLongValue[..10]}...\" exceeded {TEXT_MAX_LENGTH} charactes long");
            }

            var CountHeaderLength = (Header header) => 2 * sizeof(short) + header.Key.Length + header.Value.Length;

            var EncodeNameValue = (Header header) =>
            {
                try
                {
                    byte[] nameLengthBytes = _builder.GetBytes((short)header.Key.Length);
                    byte[] valueLenghtBytes = _builder.GetBytes((short)header.Value.Length);
                    byte[] nameBuffer = _builder.GetBytes(header.Key);
                    byte[] valueBuffer = _builder.GetBytes(header.Value);

                    byte[] result = _builder.JoinBuffers(nameLengthBytes, valueLenghtBytes, nameBuffer, valueBuffer);

                    return result;
                }
                catch (Exception e)
                {
                    throw new InvalidDataException($"header \"{header.Key}:{header.Value}\" is invalid: {e.Message}");
                }
            };

            try
            {
                byte[] headersCounterBytes = _builder.GetBytes(headersCount);
                byte[][] encodedHeaders = headers.Select(EncodeNameValue).ToArray();

                byte[] result = _builder.AllocateBuffer(sizeof(short) + headers.Sum(CountHeaderLength));
                _builder.CopyBuffersIntoTarget(result, headersCounterBytes, encodedHeaders);
                return result;
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Cannot encode message metadata: {e.Message}");
            }
        }

        public SignalMessage Decode(byte[] stream)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new NullReferenceException($"Input stream is empty");
            }

            try
            {
                int payloadOffset = _builder.GetInteger(stream, 0);
                var headers = DecodeHeaders(stream, payloadOffset);
                var payload = DecodePayload(stream, payloadOffset);

                return new SignalMessage() { Headers = headers, Payload = payload };
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot decode stream: {e.Message}");
                return default;
            }

        }

        private Dictionary<string, string>? DecodeHeaders(in byte[] stream, in int payloadOffset)
        {
            Dictionary<string, string>? headers = null;

            var DecodeASCII = (byte[] stream, in int offset, in int length) =>
            {
                byte[] buffer = _builder.CopyFromStream(stream, offset, length);
                var result = new String(System.Text.Encoding.ASCII.GetChars(buffer));
                return result;
            };

            var DecodeNameValue = (in byte[] stream, in int headerCursor, in int valueOffset, in int valueLength) =>
            {
                var headerDataPointer = headerCursor + 2 * sizeof(short);
                var headerName = DecodeASCII(stream, headerDataPointer, valueOffset);
                var headerValue = DecodeASCII(stream, headerDataPointer + valueOffset, valueLength);
                return (headerName, headerValue);
            };

            try
            {
                bool hasHeaders = payloadOffset > sizeof(int);
                if (hasHeaders)
                {
                    headers = new Dictionary<string, string>();
                    int cursor = sizeof(int) + sizeof(short);

                    int headersCount = _builder.GetShort(stream, sizeof(int));
                    for (var hIndex = 0; hIndex < headersCount; ++hIndex)
                    {
                        short valueOffset = _builder.GetShort(stream, cursor);
                        short valueLength = _builder.GetShort(stream, cursor + sizeof(short));
                        var (name, value) = DecodeNameValue(stream, cursor, valueOffset, valueLength);
                        headers.Add(name, value);
                        cursor += sizeof(int) + valueOffset + valueLength;
                    }
                }

                return headers;
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"cannot decode message headers: {e.Message}");
            }
        }

        private byte[]? DecodePayload(in byte[] stream, in int payloadOffset)
        {
            try
            {
                bool hasPayload = payloadOffset < stream.Length;
                if (hasPayload)
                {
                    var payloadBytes = _builder.CopyFromStream(stream, payloadOffset);
                    return payloadBytes;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"cannot retrieve payload from stream: {ex.Message}");
            }

            return null;
        }
    }
}