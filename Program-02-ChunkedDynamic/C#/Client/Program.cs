using System.Net;
using System.Net.Sockets;

Console.Write("CLIENT\n\n");
var (serverIP, serverPort, payloadFilePath) = ParseAndValidateCommandLineArguments();
await Task.Delay(1000); // Wait till the server starts.

TcpClient tcpClient = new();
await tcpClient.ConnectAsync(serverIP, serverPort);
using NetworkStream channel = tcpClient.GetStream();
using BinaryWriter channelWriter = new(channel); // Using BinaryWriter.Write for simplicity. This is old synchronous API. A modern alternative would be to call the low level Stream(here NetworkStream).ReadAsync directly.
byte[] imageBytes = GetImageAsBytes();
channelWriter.Write(imageBytes);
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

//static (int metadataLength, string metadata, byte[] fileBytes) PreparePayload()
//{
//    Path.GetRelativePath(args)
//}

static byte[] GetImageAsBytes()
{
    string imagePath = "./IMAGE.jpg";
    using FileStream imageStream = new(imagePath, FileMode.Open, FileAccess.Read);
    BinaryReader imageReader = new(imageStream);
    Console.WriteLine(imageStream.Length);
    if (imageStream.Length > int.MaxValue)
    {
        throw new Exception("Can't send files larger than 2GB (int.MaxValue) in this version.");
    }
    return imageReader.ReadBytes((int)imageStream.Length);
}
