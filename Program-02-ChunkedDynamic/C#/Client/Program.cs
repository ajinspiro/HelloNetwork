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
    FileSize = payloadStream.Length
};
string metadataJSONString = JsonSerializer.Serialize(metadataObject);
var metadataBytes = Encoding.UTF8.GetBytes(metadataJSONString); // UTF-8 has no endianness.
byte[] metadataLengthBytes;
if (BitConverter.IsLittleEndian)
{
    metadataLengthBytes = BitConverter.GetBytes(metadataBytes.Length).Reverse().ToArray(); // Network byte order is Big-endian. So, we need to convert from little-endian to big-endian.
}
else
{
    metadataLengthBytes = BitConverter.GetBytes(metadataBytes.Length);
}
await channel.WriteAsync(metadataLengthBytes, CancellationToken.None); // Using the more modern low level Stream.WriteAsync instead of BinaryWriter
Console.WriteLine($"METADATA LENGTH SENT: {metadataBytes.Length}");
await channel.WriteAsync(metadataBytes, CancellationToken.None);
Console.WriteLine($"METADATA SENT: {metadataJSONString}");
Console.WriteLine("Sending Metadata complete. Waiting for acknowledgement message from server.");
byte[] ackMessageByte = new byte[1];
int bytesRead = await channel.ReadAsync(ackMessageByte, CancellationToken.None);
if (bytesRead != 1)
{
    throw new Exception("Incorrect amount of bytes read.");
}
int ackMessage = BitConverter.IsLittleEndian ? 
    BitConverter.ToInt32([ackMessageByte[0], 0, 0, 0]) : 
    BitConverter.ToInt32([0, 0, 0, ackMessageByte[0]]);

if (ackMessage != 0 && ackMessage != 1)
{
    throw new Exception($"Server sent invalid acknowledgement: {ackMessage}");
}
Console.WriteLine($"ACKNOWLEDGEMENT RECEIVED: {ackMessage} - {(ackMessage == 1 ? "Proceed" : "Denied")}");

Console.WriteLine("Sending the file in chunks.");

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
