namespace Sinch.Encoding.Tests
{
    public class ByteBufferBuilderSpecs
    {
        [Fact]
        public void Should_merge_two_buffers_into_one()
        {
            // given
            ByteBufferBuilder builder = new ByteBufferBuilder();
            byte[] someHeader = System.Text.Encoding.UTF8.GetBytes("heading text");
            byte[] somePayload = System.Text.Encoding.UTF8.GetBytes("payload content");
            var separator = "|";

            // when
            byte[] result = builder.MergeBuffers(someHeader, somePayload, System.Text.Encoding.UTF8.GetBytes(separator)[0]);

            // then
            Assert.Equal($"heading text{separator}payload content", System.Text.Encoding.Default.GetString(result));
        }

        [Fact]
        public void Should_join_three_buffers_into_one()
        {
            // given
            ByteBufferBuilder builder = new ByteBufferBuilder();
            var buffers = new List<byte[]>
            {
                System.Text.Encoding.UTF8.GetBytes("n-1:v-1"),
                System.Text.Encoding.UTF8.GetBytes("n-2:v-2"),
                System.Text.Encoding.UTF8.GetBytes("n-3:v-3"),
            };

            var separator = "?";

            // when
            byte[] result = builder.JoinBuffers(buffers, System.Text.Encoding.UTF8.GetBytes(separator)[0]);

            // then
            Assert.Equal($"n-1:v-1?n-2:v-2?n-3:v-3", System.Text.Encoding.Default.GetString(result));
        }

    }
}
