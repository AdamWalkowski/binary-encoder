namespace Sinch.Encoding.Tests
{
    public class ByteBufferBuilderSpecs
    {
        private ByteBufferBuilder builder;
        public ByteBufferBuilderSpecs()
        {
            builder = new ByteBufferBuilder();
        }

        [Fact]
        public void Should_join_three_buffers_into_one()
        {
            // given      
            byte[] first = System.Text.Encoding.ASCII.GetBytes("heading text");
            byte[] second = System.Text.Encoding.ASCII.GetBytes("payload content");
            byte[] third = System.Text.Encoding.ASCII.GetBytes("some other textual content");

            // when
            byte[] result = builder.JoinBuffers(first, second, third);

            // then
            Assert.Equal(
                $"heading textpayload contentsome other textual content",
                System.Text.Encoding.Default.GetString(result)
            );
        }

        [Fact]
        public void Should_copy_into_stream()
        {
            // given
            var target = new byte[15];
            var buffers = new List<byte[]>(new[] {
                System.Text.Encoding.ASCII.GetBytes("Apollo"),
                System.Text.Encoding.ASCII.GetBytes("Mars"),
                System.Text.Encoding.ASCII.GetBytes("Venus")
            }).ToArray();

            // when
            builder.CopyBuffersIntoTarget(target, null, buffers);

            // then
            Assert.Equal("ApolloMarsVenus", new String(System.Text.Encoding.ASCII.GetChars(target)));
        }

        [Fact]
        public void Should_copy_from_stream()
        {
            // given
            var stream = System.Text.Encoding.ASCII.GetBytes("0123456789");

            // when
            var result = builder.CopyFromStream(stream, 2, 5);

            // then
            Assert.Equal("23456", new String(System.Text.Encoding.ASCII.GetChars(result)));
        }
    }
}
