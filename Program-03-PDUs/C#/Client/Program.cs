using ProtocolAssets;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

Console.Write("CLIENT\n\n");
var (serverIP, serverPort, payloadFilePath) = ParseAndValidateCommandLineArguments();
await Task.Delay(1000); // Wait till the server starts.

TcpClient tcpClient = new();
await tcpClient.ConnectAsync(serverIP, serverPort);
using NetworkStream channel = tcpClient.GetStream();
using FileStream payloadStream = new(payloadFilePath, FileMode.Open, FileAccess.Read);

PDUSerializer pduSerializer = new();
PDUDeserializer pduDeserializer = new();
byte[] connectPDUBytes = pduSerializer.Serialize(new PDU.Connect());
Console.Write("Sending CONNECT...");
await channel.WriteAsync(connectPDUBytes);
Console.WriteLine("Complete");
PDU.Response responsePDU = await pduDeserializer.DeserializeResponse(channel);
Console.WriteLine($"RESPONSE({responsePDU.Message}) received. ");
if (responsePDU.Message == "ER")
{
    Console.WriteLine($"Exiting because of server error.");
    Environment.Exit(-1);
}
Console.Write("Sending METADATA...");
PDU.Metadata metadataPDU = new(payloadStream.Length, Path.GetFileName(payloadFilePath));
byte[] metadataPDUBytes = pduSerializer.Serialize(metadataPDU);
await channel.WriteAsync(metadataPDUBytes);
Console.WriteLine("Complete");
Console.WriteLine("Transfer complete.");
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
