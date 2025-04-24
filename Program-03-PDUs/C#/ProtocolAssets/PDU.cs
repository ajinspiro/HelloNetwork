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
    public class Error : IPDU
    {
        public Error(byte code, string message)
        {
            ErrorCode = code;
            Message = message;
        }
        public byte Type => 3;
        public byte ErrorCode { get; set; }
        public string Message { get; set; }
    }
    public class Proceed : IPDU
    {
        public byte Type => 2;
        public string Message => "PROCEED";
    }
    public class Metadata : IPDU
    {
        public Metadata(long fileSize, string fileName)
        {
            FileSize = fileSize;
            FileNameLength = (short)fileName.Length; // fileName will never be longer than 255.
            FileName = fileName;
        }
        public byte Type => 4;
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
        public byte Type => 5;
        public int PayloadSize { get; set; }
        public byte[] Payload { get; set; }
    }
    public class Finish : IPDU
    {
        public byte Type => 6;
        public string Message => "FINISH";
    }
}