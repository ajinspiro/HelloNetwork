using ProtocolAssets;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;


Console.Write("SERVER\n\n");
var (serverIP, serverPort) = ParseAndValidateCommandLineArguments();

PDUConverter pduConverter = new();
using TcpListener tcpListener = new(serverIP, serverPort);
tcpListener.Start();
while (true)
{
    TcpClient connectedClient = await tcpListener.AcceptTcpClientAsync();
    _ = Task.Run(() => ProcessClient(connectedClient, pduConverter));
}

static async Task ProcessClient(TcpClient connectedClient, PDUConverter pduConverter)
{
    try
    {
        using NetworkStream channel = connectedClient.GetStream();
        PDU.Connect connectPdu = await pduConverter.DeserializeConnect(channel);
        Console.WriteLine(JsonSerializer.Serialize(connectPdu));
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Processing failed. Exception: {ex.GetType().FullName} Message: {ex.Message}");
    }
}
(IPAddress, int) ParseAndValidateCommandLineArguments()
{
    bool isThere2Args = args.Length == 2;

    if (!isThere2Args)
    {
        Console.Error.WriteLine("Incorrect usage.");
        Console.Error.WriteLine("Usage: >Server.exe 127.0.0.1 11000");
        Environment.Exit(-1);
    }

    bool isParsingServerIPSuccess = IPAddress.TryParse(args[0], out IPAddress? ipAddress);
    bool isPortValid = int.TryParse(args[1], out int port);

    if (!isParsingServerIPSuccess && !isPortValid)
    {
        Console.Error.WriteLine("Incorrect usage.");
        Console.Error.WriteLine("Usage: >Client.exe 127.0.0.1 11000 C:/folder1/someFile.jpg");
        Environment.Exit(-1);
    }

    return (ipAddress!, port);
}
