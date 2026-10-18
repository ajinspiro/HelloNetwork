namespace ProtocolAssets;

public interface IPDU
{
    public byte Type { get; }
}

public class PDU
{
    public class Connect : IPDU
    {
        public byte Type => 1;
        public string Message => "CONNECT";
    }
    public class Response : IPDU
    {
        public enum ResponseType { Error, Okay }

        public byte Type => 2;
        public string Message { get; }
        public Response(ResponseType responseType)
        {
            Message = responseType == ResponseType.Error ? "ER" : "OK";
        }
    }
    public class Metadata : IPDU
    {
        public Metadata(long fileSize, string fileName)
        {
            FileSize = fileSize;
            FileNameLength = (short)fileName.Length; // fileName will never be longer than 255.
            FileName = fileName;
        }
        public byte Type => 3;
        public long FileSize { get; set; }
        public short FileNameLength { get; set; }
        public string FileName { get; set; }
    }
    public class Data : IPDU
    {
        public Data(byte[] payload)
        {
            Payload = payload;
            PayloadSize = payload.Length;
        }
        public byte Type => 4;
        public int PayloadSize { get; set; }
        public byte[] Payload { get; set; }
    }
    public class Finish : IPDU
    {
        public byte Type => 5;
        public string Message => "FINISH";
    }
}