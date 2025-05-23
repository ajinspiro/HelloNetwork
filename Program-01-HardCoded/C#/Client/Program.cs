﻿using System.Net.Sockets;

Console.Write("CLIENT\n\n");
await Task.Delay(1000); // Wait till the server starts.

TcpClient tcpClient = new();
await tcpClient.ConnectAsync("127.0.0.1", 11000);
using NetworkStream channel = tcpClient.GetStream();
using BinaryWriter channelWriter = new(channel); // Using BinaryWriter.Write for simplicity. This is old synchronous API. A modern alternative would be to call the low level Stream(here NetworkStream).ReadAsync directly.
byte[] imageBytes = GetImageAsBytes();
channelWriter.Write(imageBytes);
Console.WriteLine("Sending file IMAGE.jpg complete.");
tcpClient.Dispose(); // Close the connection

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
