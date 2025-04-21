using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

Console.Write("CLIENT\n\n");
var (serverIP, serverPort, payloadFilePath) = ParseAndValidateCommandLineArguments();
await Task.Delay(1000); // Wait till the server starts.

TcpClient tcpClient = new();
await tcpClient.ConnectAsync(serverIP, serverPort);
using NetworkStream channel = tcpClient.GetStream();
using FileStream payloadStream = new(payloadFilePath, FileMode.Open, FileAccess.Read);
var metadataObject = new
{
    FileName = Path.GetFileName(payloadFilePath),
    FileSize = payloadFilePath.Length
};
var metadataJSONString = JsonSerializer.Serialize(metadataObject);
var metadataBytes = Encoding.BigEndianUnicode.GetBytes(metadataJSONString); // Network byte order is Big-endian.
byte[] metadataLengthBytes;
if (BitConverter.IsLittleEndian)
{
    metadataLengthBytes = BitConverter.GetBytes(metadataBytes.Length);
}
else
{
    metadataLengthBytes = BitConverter.GetBytes(metadataBytes.Length).Reverse().ToArray();
}
await channel.WriteAsync(metadataLengthBytes, CancellationToken.None); // Using the modern low level Stream.WriteAsync instead of BinaryWriter
await channel.WriteAsync(metadataBytes);

Console.WriteLine("Sending file IMAGE.jpg complete.");
tcpClient.Dispose(); // Close the connection

(IPAddress, int, string) ParseAndValidateCommandLineArguments()
{
    bool isThere3Args = args.Length == 3;

    if (!isThere3Args)
    {
        Console.Error.WriteLine("Incorrect usage.");
        Console.Error.WriteLine("Usage: >Client.exe 127.0.0.1 11000 C:/folder1/someFile.jpg");
        Environment.Exit(-1);
    }

    bool isParsingServerIPSuccess = IPAddress.TryParse(args[0], out IPAddress? ipAddress);
    bool isPortValid = int.TryParse(args[1], out int port);
    bool isPayloadPathExists = Path.Exists(args[2]);

    if (!isParsingServerIPSuccess && !isPortValid && !isPayloadPathExists)
    {
        Console.Error.WriteLine("Incorrect usage.");
        Console.Error.WriteLine("Usage: >Client.exe 127.0.0.1 11000 C:/folder1/someFile.jpg");
        Environment.Exit(-1);
    }

    return (ipAddress!, port, args[2]);
}

static byte[] GetImageAsBytes(string payloadFilePath)
{
    using FileStream imageStream = new(payloadFilePath, FileMode.Open, FileAccess.Read);
    BinaryReader imageReader = new(imageStream);
    Console.WriteLine(imageStream.Length);
    if (imageStream.Length > int.MaxValue)
    {
        throw new Exception("Can't send files larger than 2GB (int.MaxValue) in this version.");
    }
    return imageReader.ReadBytes((int)imageStream.Length);
}
