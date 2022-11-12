using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using Header = System.Collections.Generic.KeyValuePair<string, string>;

namespace Sinch.Encoding
{
    public class MessageEncoder
    {
        private void GetBytes(string input, in byte[] buffer)
        {
            for (int index = 0; index < input.Length; index++)
            {
                var inputSign = input[index];

                buffer[index] = Convert.ToByte(inputSign);
            }
        }

        private string GetString(byte[] input)
        {
            char[] chars = System.Text.Encoding.UTF8.GetChars(input);

            //System.Buffer.BlockCopy(input, 0, chars, 0, input.Length);
            return new String(chars);
        }

        private static byte HEADER_SEPARATOR = 0b01111101;
        private static byte NAME_VALUE_SEPARATOR = 0b01111110;
        private static byte PAYLOAD_SEPARATOR = 0b01111100;
        private static IEnumerable<byte> SpecialSigns = new List<byte> { HEADER_SEPARATOR, NAME_VALUE_SEPARATOR, PAYLOAD_SEPARATOR };

        public static char PayloadSeparator { get => Convert.ToChar(PAYLOAD_SEPARATOR); }

        private ByteBufferBuilder _builder = new ByteBufferBuilder();

        private bool IsSpecialSign(byte sign)
        {
            return SpecialSigns.Contains(sign);
        }

        private byte[] GetBytes(KeyValuePair<string, string> header)
        {
            byte[] nameBuffer = new byte[header.Key.Length];
            byte[] valueBuffer = new byte[header.Value.Length];

            GetBytes(header.Key, nameBuffer);
            GetBytes(header.Value, valueBuffer);
            //BitConverter.IsLittleEndian

            byte[] result = _builder.MergeBuffers(nameBuffer, valueBuffer, NAME_VALUE_SEPARATOR);

            return result;
        }

        public byte[] Encode(SignalMessage message)
        {
            byte[] headStream = new byte[0];

            if (message.Headers != null)
            {
                int headerBytes = 0;
                var encodedHeaders = new List<byte[]>();
                foreach (var header in message.Headers)
                {
                    var econdodHeader = GetBytes(header);
                    headerBytes += econdodHeader.Length;
                    encodedHeaders.Add(econdodHeader);
                }

                headStream = _builder.JoinBuffers(encodedHeaders);
            }

            byte[] finalStream = _builder.MergeBuffers(BitConverter.GetBytes(headStream.Length + message.Payload.Length), headStream, message.Payload);

            return finalStream;
        }

        public SignalMessage Decode(byte[] stream)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            byte[] payload = new byte[0];

            int cursor = 0;
            int nextCursor = 0;

            while (cursor < stream.Length)
            {
                while (nextCursor < stream.Length)
                {
                    var sign = stream[nextCursor];
                    if (sign == PAYLOAD_SEPARATOR)
                    {
                        var header1 = DecodeHeader(stream, cursor, nextCursor);
                        payload = GetPayload(stream, nextCursor);
                        headers.Add(header1.name, header1.value);
                    }
                    else if (sign == HEADER_SEPARATOR)
                    {
                        var header = DecodeHeader(stream, cursor, nextCursor);
                        headers.Add(header.name, header.value);
                        cursor = ++nextCursor;
                    }

                    nextCursor++;
                }

                cursor++;
            }

            if (payload != null)
            {
                return new SignalMessage() { Headers = headers, Payload = payload };
            }
            else
            {
                throw new InvalidDataException("invalid");
            }
        }

        private (string name, string value) DecodeHeader(byte[] stream, in int startIndex, in int endIndex)
        {
            int separatorPointer = startIndex;
            while (separatorPointer < endIndex)
            {
                if (stream[separatorPointer] == NAME_VALUE_SEPARATOR)
                {
                    break;
                }
                separatorPointer++;
            }

            byte[] nameBuffer = new byte[separatorPointer - startIndex];
            byte[] valueBuffer = new byte[endIndex - 1 - separatorPointer];

            Buffer.BlockCopy(stream, startIndex, nameBuffer, 0, nameBuffer.Length);
            Buffer.BlockCopy(stream, separatorPointer + 1, valueBuffer, 0, valueBuffer.Length);

            return (GetString(nameBuffer), GetString(valueBuffer));
        }

        private (string name, string value) DecodeHeader(byte[] stream, in int startIndex, in int endIndex)
        {
            byte[] nameBuffer = new byte[separatorPointer - startIndex];
            byte[] valueBuffer = new byte[endIndex - 1 - separatorPointer];

            Buffer.BlockCopy(stream, startIndex, nameBuffer, 0, nameBuffer.Length);
            Buffer.BlockCopy(stream, separatorPointer + 1, valueBuffer, 0, valueBuffer.Length);

            return (GetString(nameBuffer), GetString(valueBuffer));
        }

        private byte[] GetPayload(in byte[] stream, in int startIndex)
        {
            int payloadLength = stream.Length - startIndex - 1;
            byte[] payload = new byte[payloadLength];
            Buffer.BlockCopy(stream, startIndex + 1, payload, 0, payloadLength);
            return payload;
        }

    }
}