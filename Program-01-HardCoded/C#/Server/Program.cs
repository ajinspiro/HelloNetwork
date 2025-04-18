using System.Net;
using System.Net.Sockets;

Console.Write("SERVER\n\n");

IPAddress localhost = IPAddress.Parse("127.0.0.1");
TcpListener tcpListener = new(localhost, 11000);
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
        NetworkStream channel = connectedClient.GetStream();
        BinaryReader channelReader = new(channel);
        byte[] imageBytes = channelReader.ReadBytes(90674); // Limitation: server needs to know the size of the transferred file in advance.
        using FileStream imageStream = new("IMAGE.jpg", FileMode.Create, FileAccess.Write);
        await imageStream.WriteAsync(imageBytes);
        Console.WriteLine("File received.");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Processing failed. Exception: {ex.GetType().FullName}");
    } 
}