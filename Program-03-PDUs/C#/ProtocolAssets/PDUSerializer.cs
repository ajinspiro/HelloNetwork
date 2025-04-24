using System.Text;

namespace ProtocolAssets;

public class PDUSerializer
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
    public byte[] Serialize(PDU.Response response)
    {
        int pduSize = 1 + 2; // 1 byte type and 2 byte message
        byte[] pduBytes = new byte[pduSize];
        pduBytes[0] = response.Type;
        byte[] messageBytes = Encoding.UTF8.GetBytes(response.Message);
        Buffer.BlockCopy(messageBytes, 0, pduBytes, 1, messageBytes.Length);
        return pduBytes;
    }
    public byte[] Serialize(PDU.Metadata metadata)
    {
        byte[] fileSizeBytes = BitConverter.GetBytes(metadata.FileSize);
        byte[] fileNameLengthBytes = BitConverter.GetBytes(metadata.FileNameLength);
        byte[] fileNameBytes = Encoding.UTF8.GetBytes(metadata.FileName);
        if (BitConverter.IsLittleEndian)
        {
            // Network byte order is big endian. So, lets reverse the bytes if the bytes are in little endian.
            fileSizeBytes = fileSizeBytes.Reverse().ToArray();
            fileNameLengthBytes = fileNameLengthBytes.Reverse().ToArray();
        }
        int pduSize = 1 + fileSizeBytes.Length + fileNameLengthBytes.Length + fileNameBytes.Length;
        byte[] pduBytes = new byte[pduSize];
        int totalBytesWritten = 0; // Serves as the offset index for BlockCopy

        pduBytes[0] = metadata.Type;
        totalBytesWritten++;

        Buffer.BlockCopy(fileSizeBytes, 0, pduBytes, totalBytesWritten, fileSizeBytes.Length);
        totalBytesWritten += fileSizeBytes.Length;

        Buffer.BlockCopy(fileNameLengthBytes, 0, pduBytes, totalBytesWritten, fileNameLengthBytes.Length);
        totalBytesWritten += fileNameLengthBytes.Length;

        Buffer.BlockCopy(fileNameBytes, 0, pduBytes, totalBytesWritten, fileNameBytes.Length);
        // totalBytesWritten += fileNameBytes.Length; This line is commented because the value is never used.

        return pduBytes;
    }
}
