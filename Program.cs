using Sinch.Encoding;
using System.Text.Json;
using System.Text.Json.Serialization;
using Header = System.Collections.Generic.KeyValuePair<string, string>;

var message = new SignalMessage
{
    Headers = new Dictionary<string, string>(new Header[] {
        new Header("name-1", "value-1"),
        new Header("name-2", "value-2"),
        new Header("name-3", "value-3"),
    }),
    Payload = new byte[] { 0b00100011, 0b00011010, 0b00100011 }
};

Console.WriteLine($"This is object before encoding: ${JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true })}\n");

var byteEncoder = new ByteBufferBuilder();
var encoder = new SignalMessageCodec(byteEncoder);

try
{
    var encodedMessage = encoder.Encode(message);

    Console.WriteLine($"This is encoded stream: ${Convert.ToHexString(encodedMessage)}\n");

    var decodedMessage = encoder.Decode(encodedMessage);

    Console.WriteLine($"This is object after decoding: ${JsonSerializer.Serialize(decodedMessage, new JsonSerializerOptions { WriteIndented = true })}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"Something happened: {ex.Message}");
}
