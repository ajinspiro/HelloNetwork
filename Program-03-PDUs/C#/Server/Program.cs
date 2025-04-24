using ProtocolAssets;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;


Console.Write("SERVER\n\n");
var (serverIP, serverPort) = ParseAndValidateCommandLineArguments();

PDUSerializer pduSerializer = new();
PDUDeserializer pduDeserializer = new();
using TcpListener tcpListener = new(serverIP, serverPort);
tcpListener.Start();
while (true)
{
    TcpClient connectedClient = await tcpListener.AcceptTcpClientAsync();
    _ = Task.Run(() => ProcessClient(connectedClient, pduSerializer, pduDeserializer));
}

static async Task ProcessClient(TcpClient connectedClient, PDUSerializer pduSerializer, PDUDeserializer pduDeserializer)
{
    try
    {
        using NetworkStream channel = connectedClient.GetStream();
        PDU.Connect connectPdu = await pduDeserializer.DeserializeConnect(channel);
        Console.Write("CONNECT received.\nSending RESPONSE(OK)...");
        byte[] responsePduBytes = pduSerializer.Serialize(new PDU.Response(PDU.Response.ResponseType.Okay));
        await channel.WriteAsync(responsePduBytes, 0, responsePduBytes.Length);
        Console.WriteLine("Complete");
        PDU.Metadata metadataPDU = await pduDeserializer.DeserializeMetadata(channel);
        Console.WriteLine($"METADATA received. {JsonSerializer.Serialize(metadataPDU)}");
        Console.WriteLine("Transfer complete.");
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
