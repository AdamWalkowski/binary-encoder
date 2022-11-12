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


Console.WriteLine($"This is object before encoding: ${JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true })}");


var encoder = new MessageEncoder();

try
{
    var encodedMessage = encoder.Encode(message);
    if (encodedMessage != null)
    {
        Console.WriteLine($"This is encoded message: ${encodedMessage.ToString()}");
    }

    try
    {
        var decodedMessage = encoder.Decode(encodedMessage);


        Console.WriteLine($"This is decoded object: ${decodedMessage.ToString()}");


    }
    catch (Exception ex)
    {
        Console.WriteLine($"Cannot decode message: {ex.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Cannot encode message: {ex.Message}");
}

