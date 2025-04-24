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
}
