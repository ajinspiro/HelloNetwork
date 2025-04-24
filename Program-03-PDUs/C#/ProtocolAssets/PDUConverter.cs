using System.Text;

namespace ProtocolAssets;

public class PDUConverter
{
    public byte[] Serialize(PDU.Connect connect)
    {
        int pduSize = 1 + connect.Message.Length; // +1 for type byte
        byte[] pduBytes = new byte[pduSize];
        pduBytes[0] = connect.Type;
        byte[] messageBytes = Encoding.UTF8.GetBytes(connect.Message);
        Buffer.BlockCopy(messageBytes, 0, pduBytes, 1, messageBytes.Length);
        return pduBytes;
    }

    public async Task<PDU.Connect> DeserializeConnect(Stream stream)
    {
        byte[] buffer = new byte[8];
        int bytesRead = await stream.ReadAsync(buffer, 0, 1);
        PDU.Connect connectPdu = new();
        if (bytesRead != 1)
        {
            throw new Exception("Incorrect amount of bytes read.");
        }
        if (buffer[0] != 1)
        {
            throw new Exception($"Failed to deserialize CONNECT. Type value was {buffer[0]} instead of {connectPdu.Type}");
        }
        bytesRead = await stream.ReadAsync(buffer, 1, 7);
        string connectMessage = Encoding.UTF8.GetString(buffer, 1, 7);
        if (connectMessage != "CONNECT")
        {
            throw new Exception($"Failed to deserialize CONNECT. Message value was {connectMessage} instead of {connectPdu.Message}");
        }
        return connectPdu;
    }
}
