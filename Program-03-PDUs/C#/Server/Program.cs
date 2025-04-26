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
        long availableFreeSpace = DriveInfo
            .GetDrives()
            .First(drive =>
                AppContext.BaseDirectory.StartsWith(drive.Name, StringComparison.OrdinalIgnoreCase)
                )
            .AvailableFreeSpace;
        var responseType = (availableFreeSpace <= metadataPDU.FileSize) ? PDU.Response.ResponseType.Error : PDU.Response.ResponseType.Okay;
        PDU.Response metadataResponse = new(responseType);
        Console.Write($"Sending RESPONSE({metadataResponse.Message})...");
        await channel.WriteAsync(pduSerializer.Serialize(metadataResponse));
        Console.WriteLine("Complete");
        if (availableFreeSpace <= metadataPDU.FileSize)
        {
            Console.WriteLine("There is not enough space to receive the data from client. Exiting.");
            return;
        }
        int totalBytesRead = 0;
        using MemoryStream payloadBuffer = new(metadataPDU.FileSize > int.MaxValue ? int.MaxValue : (int)metadataPDU.FileSize);
        short packetNumber = 1;
        do
        {
            PDU.Data dataPDU = await pduDeserializer.DeserializeData(channel);
            Console.WriteLine($"DATA(Packet number={packetNumber++}) received.");
            Console.Write($"Sending RESPONSE(OK)...");
            await channel.WriteAsync(responsePduBytes);
            Console.WriteLine("Completed");
            totalBytesRead += dataPDU.PayloadSize;
            await payloadBuffer.WriteAsync(dataPDU.Payload);
        } while (totalBytesRead < metadataPDU.FileSize);
        using FileStream payloadStream = new(metadataPDU.FileName, FileMode.Create);
        payloadBuffer.Seek(0, SeekOrigin.Begin);
        await payloadBuffer.CopyToAsync(payloadStream);
        await payloadStream.FlushAsync();
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
