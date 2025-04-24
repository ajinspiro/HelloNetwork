using ProtocolAssets;
using System.Net;
using System.Net.Sockets;

Console.Write("CLIENT\n\n");
var (serverIP, serverPort, payloadFilePath) = ParseAndValidateCommandLineArguments();
await Task.Delay(1000); // Wait till the server starts.

TcpClient tcpClient = new();
await tcpClient.ConnectAsync(serverIP, serverPort);
using NetworkStream channel = tcpClient.GetStream();
using FileStream payloadStream = new(payloadFilePath, FileMode.Open, FileAccess.Read);

PDUConverter pduConverter = new();

byte[] connectPDU = pduConverter.Serialize(new PDU.Connect());
await channel.WriteAsync(connectPDU);

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
