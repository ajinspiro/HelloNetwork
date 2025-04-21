using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

Console.Write("SERVER\n\n");
var (serverIP, serverPort) = ParseAndValidateCommandLineArguments();

using TcpListener tcpListener = new(serverIP, serverPort);
tcpListener.Start();
while (true)
{
    TcpClient connectedClient = await tcpListener.AcceptTcpClientAsync();
    _ = Task.Run(() => ProcessClient(connectedClient));
}

static async Task ProcessClient(TcpClient connectedClient)
{
    try
    {
        using NetworkStream channel = connectedClient.GetStream();
        byte[] metadataLengthBytes = new byte[sizeof(int)]; // sizeof(int)=4
        int bytesRead = await channel.ReadAsync(metadataLengthBytes, 0, sizeof(int)); // Using the more modern low level Stream.ReadAsync instead of BinaryReader
        if (bytesRead != sizeof(int))
        {
            throw new Exception("Incorrect amount of bytes read.");
        }
        if (BitConverter.IsLittleEndian)
        {
            metadataLengthBytes = metadataLengthBytes.Reverse().ToArray(); // Host byte order is Little-endian. So, we need to convert from little-endian to big-endian.
        }
        int metadataLength = BitConverter.ToInt32(metadataLengthBytes, 0);
        Console.WriteLine($"METADATA LENGTH RECEIVED: {metadataLength}");

        byte[] metadataBytes = new byte[metadataLength];
        bytesRead = await channel.ReadAsync(metadataBytes, 0, metadataLength);
        if (bytesRead != metadataLength)
        {
            throw new Exception("Incorrect amount of bytes read.");
        }
        string metadata = Encoding.UTF8.GetString(metadataBytes); // UTF-8 has no endianness.
        Console.WriteLine($"METADATA RECEIVED: {metadata}");

        JsonNode metadataRoot = JsonNode.Parse(metadata) ?? throw new Exception();
        long payloadSize = metadataRoot["FileSize"]?.GetValue<long>() ?? throw new Exception();
        string payloadName = metadataRoot["FileName"]?.GetValue<string>() ?? throw new Exception();

        // Checking if necessary space is available to save the file
        long availableFreeSpace = DriveInfo
            .GetDrives()
            .First(drive =>
                AppContext.BaseDirectory.StartsWith(drive.Name, StringComparison.OrdinalIgnoreCase)
                )
            .AvailableFreeSpace;

        byte message = (byte)(availableFreeSpace <= payloadSize ? 0 : 1); // "Denied" or "Proceed"
        await channel.WriteAsync(new byte[1] { message });
        Console.WriteLine($"SENDING ACKNOWLEDGEMENT: {message} {(message == 1 ? "Proceed" : "Denied")}");
        if (availableFreeSpace <= payloadSize)
        {
            return;
        }

        connectedClient.Dispose(); // Close the connection
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Processing failed. Exception: {ex.GetType().FullName}");
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
