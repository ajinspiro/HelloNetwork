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
    public async Task<PDU.Response> DeserializeResponse(Stream stream)
    {
        byte[] buffer = new byte[3];
        int bytesRead = await stream.ReadAsync(buffer, 0, 1);
        if (bytesRead != 1)
        {
            throw new Exception("Incorrect amount of bytes read.");
        }
        if (buffer[0] != 2)
        {
            throw new Exception($"Failed to deserialize RESPONSE. Type value was {buffer[0]} instead of 2");
        }
        bytesRead = await stream.ReadAsync(buffer, 1, 2);
        string connectMessage = Encoding.UTF8.GetString(buffer, 1, 2);

        PDU.Response responsePdu = connectMessage switch
        {
            "ER" => new(PDU.Response.ResponseType.Error),
            "OK" => new(PDU.Response.ResponseType.Okay),
            _ => throw new Exception($"Failed to deserialize RESPONSE. Message value was {connectMessage} instead of ER or OK.")
        };
        return responsePdu;
    }
    public async Task<PDU.Metadata> DeserializeMetadata(Stream stream)
    {
        byte[] buffer = new byte[1];
        int bytesRead = await stream.ReadAsync(buffer);
        if (bytesRead != 1)
        {
            throw new Exception("Incorrect amount of bytes read.");
        }
        if (buffer[0] != 3)
        {
            throw new Exception($"Failed to deserialize METADATA. Type value was {buffer[0]} instead of 3");
        }

        byte[] fileSizeBytes = new byte[sizeof(long)];
        bytesRead = await stream.ReadAsync(fileSizeBytes);
        fileSizeBytes = BitConverter.IsLittleEndian ? fileSizeBytes.Reverse().ToArray() : fileSizeBytes;
        if (bytesRead != sizeof(long))
        {
            throw new Exception("Incorrect amount of bytes read.");
        }
        long fileSize = BitConverter.ToInt64(fileSizeBytes);

        byte[] fileNameLengthBytes = new byte[sizeof(short)];
        bytesRead = await stream.ReadAsync(fileNameLengthBytes);
        fileNameLengthBytes = BitConverter.IsLittleEndian ? fileNameLengthBytes.Reverse().ToArray() : fileNameLengthBytes;
        if (bytesRead != sizeof(short))
        {
            throw new Exception("Incorrect amount of bytes read.");
        }
        short fileNameLength = BitConverter.ToInt16(fileNameLengthBytes);

        byte[] fileNameBytes = new byte[fileNameLength];
        bytesRead = await stream.ReadAsync(fileNameBytes);
        if (bytesRead != fileNameLength)
        {
            throw new Exception("Incorrect amount of bytes read.");
        }
        string fileName = Encoding.UTF8.GetString(fileNameBytes);
        PDU.Metadata metadataPdu = new(fileSize, fileName);
        return metadataPdu;
    }
}
