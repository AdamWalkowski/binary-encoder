namespace Sinch.Encoding
{
    public struct SignalMessage
    {
        //private const int MAX_HEADERS = 63;
        //private const int MAX_PAYLOAD_BYTES = 1023;
        //private const int MAX_HEADER_NAME_BYTES = 63;
        //private const int MAX_HEADER_VALUE_BYTES = 63;

        public Dictionary<string, string> Headers { get; set; }
        public byte[] Payload { get; set; }
        //public int ByteLength()
        //{
        //    var headersLenght = Headers.Sum(h => h.Key.Length + h.Value.Length);
        //    return headersLenght + Payload.Length;
        //}


    }
}
