using System.Text;

namespace ProtocolAssets;

public class PDUDeserializer
{
    public async Task<PDU.Connect> DeserializeConnect(Stream stream)
    {
        byte[] buffer = new byte[8];
        int bytesRead = await stream.ReadAsync(buffer, 0, 1);
        PDU.Connect connectPdu = new();
        if (bytesRead != 1)
        {
            throw new Exception("Incorrect amount of bytes read.");
        }
        if (buffer[0] != connectPdu.Type)
        {
            throw new Exception($"Failed to deserialize CONNECT. Type value was {buffer[0]} instead of {connectPdu.Type}");
        }
        bytesRead = await stream.ReadAsync(buffer, 1, 7);
        string connectMessage = Encoding.UTF8.GetString(buffer, 1, 7);
        if (connectMessage != connectPdu.Message)
        {
            throw new Exception($"Failed to deserialize CONNECT. Message value was {connectMessage} instead of {connectPdu.Message}");
        }
        return connectPdu;
    }
    public async Task<PDU.Proceed> DeserializeProceed(Stream stream)
    {
        byte[] buffer = new byte[8];
        int bytesRead = await stream.ReadAsync(buffer, 0, 1);
        PDU.Proceed proceedPdu = new();
        if (bytesRead != 1)
        {
            throw new Exception("Incorrect amount of bytes read.");
        }
        if (buffer[0] != proceedPdu.Type)
        {
            throw new Exception($"Failed to deserialize PROCEED. Type value was {buffer[0]} instead of {proceedPdu.Type}");
        }
        bytesRead = await stream.ReadAsync(buffer, 1, 7);
        string connectMessage = Encoding.UTF8.GetString(buffer, 1, 7);
        if (connectMessage != proceedPdu.Message)
        {
            throw new Exception($"Failed to deserialize PROCEED. Message value was {connectMessage} instead of {proceedPdu.Message}");
        }
        return proceedPdu;
    }
}
