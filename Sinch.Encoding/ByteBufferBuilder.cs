namespace Sinch.Encoding
{
    public class ByteBufferBuilder : IByteBufferBuilder
    {
        public byte[] GetBytes(string input)
        {
            try
            {
                byte[] encodedString = new byte[input.Length];
                for (int index = 0; index < input.Length; index++)
                {
                    encodedString[index] = (byte)input[index];
                }

                return encodedString;
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Error while converting to bytes array: {e.Message}");
            }
        }

        public byte[] GetBytes(int input)
        {
            return BitConverter.GetBytes(input);
        }

        public byte[] GetBytes(short input)
        {
            return BitConverter.GetBytes(input);
        }

        public int GetInteger(byte[] source, in int startIndex)
        {
            return BitConverter.ToInt32(source, startIndex);
        }

        public short GetShort(byte[] source, in int startIndex)
        {
            return BitConverter.ToInt16(source, startIndex);
        }

        public byte[] AllocateBuffer(params int[] sizes)
        {
            try
            {
                int fullLength = sizes.Sum();
                byte[] buffer = new byte[fullLength];
                return buffer;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"cannot allocate buffer: {e.Message}");
            }
        }

        public byte[] JoinBuffers(params byte[][] buffers)
        {
            try
            {
                int len = buffers.Sum(x => x.Length);
                byte[] target = new byte[len];
                int resultCursor = 0;

                foreach (var buffer in buffers)
                {
                    Buffer.BlockCopy(buffer, 0, target, resultCursor, buffer.Length);
                    resultCursor += buffer.Length;
                }

                return target;
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Error while joining byte streams: {e.Message}");
            }

        }

        public byte[] CopyFromStream(in byte[] stream, in int startIndex, in int? length = null)
        {
            try
            {
                int substreamLength = length ?? stream.Length - startIndex;
                byte[] buffer = new byte[substreamLength];
                Buffer.BlockCopy(stream, startIndex, buffer, 0, substreamLength);
                return buffer;
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Error while copying byte streams: {e.Message}");
            }
        }

        public void CopyBuffersIntoTarget(in byte[] target, byte[]? head = null, params byte[]?[] buffers)
        {
            try
            {
                int resultCursor = 0;

                if (head != null)
                {
                    Buffer.BlockCopy(head, 0, target, 0, head.Length);
                    resultCursor = head.Length;
                }

                foreach (var buffer in buffers)
                {
                    if (buffer != null && buffer.Length > 0)
                    {
                        Buffer.BlockCopy(buffer, 0, target, resultCursor, buffer.Length);
                        resultCursor += buffer.Length;
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Error while copying byte streams: {e.Message}");
            }
        }
    }
}
