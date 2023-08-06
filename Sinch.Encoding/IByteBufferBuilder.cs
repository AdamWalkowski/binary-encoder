namespace Sinch.Encoding
{
    public interface IByteBufferBuilder
    {
        byte[] GetBytes(int input);
        byte[] GetBytes(short input);
        byte[] GetBytes(string input);

        int GetInteger(byte[] source, in int startIndex);
        short GetShort(byte[] source, in int startIndex);

        byte[] AllocateBuffer(params int[] sizes);

        byte[] JoinBuffers(params byte[][] buffers);

        byte[] CopyFromStream(in byte[] stream, in int startIndex, in int? endIndex);
        void CopyBuffersIntoTarget(in byte[] target, byte[] head, params byte[]?[] buffers);
    }
}
