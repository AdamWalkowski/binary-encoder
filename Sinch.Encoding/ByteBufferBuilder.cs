using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinch.Encoding
{
    public class ByteBufferBuilder
    {
        public byte[] MergeBuffers(byte[] first, byte[] second, byte separator)
        {
            byte[] result = new byte[first.Length + second.Length + 1];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            result[first.Length] = separator;
            Buffer.BlockCopy(second, 0, result, first.Length + 1, second.Length);
            return result;
        }

        public byte[] MergeBuffers(byte[] head, byte[] first, byte[] second)
        {
            byte[] result = new byte[head.Length + first.Length + second.Length];
            Buffer.BlockCopy(head, 0, result, 0, head.Length);
            Buffer.BlockCopy(first, 0, result, head.Length, first.Length);
            Buffer.BlockCopy(second, 0, result, head.Length + first.Length, second.Length);
            return result;
        }

        public byte[] JoinBuffers(IEnumerable<byte[]> buffers, byte separator)
        {
            long separatorsLenght = buffers.Count() - 1;
            int len = buffers.Sum(x => x.Length);
            byte[] result = new byte[len + separatorsLenght];

            int resultCursor = 0;
            foreach (var buffer in buffers)
            {
                Buffer.BlockCopy(buffer, 0, result, resultCursor, buffer.Length);
                resultCursor += buffer.Length;
                if (resultCursor < len + separatorsLenght)
                {
                    result[resultCursor] = separator;
                    resultCursor++;
                }
            }

            return result;
        }

        public byte[] JoinBuffers(IEnumerable<byte[]> buffers)
        {
            int len = buffers.Sum(x => x.Length);
            byte[] result = new byte[len];

            int resultCursor = 0;
            foreach (var buffer in buffers)
            {
                Buffer.BlockCopy(buffer, 0, result, resultCursor, buffer.Length);
                resultCursor += buffer.Length;
            }

            return result;
        }


        //public byte[] ConvertToBytes(string value)
        //{

        //}

        //public string ConvertToString(byte[] value)
        //{

        //}

    }
}
