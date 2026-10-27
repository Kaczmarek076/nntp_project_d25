using System.IO;
using System.Net.Sockets;

namespace nntp_project_d25.Service;

public class TcpMessageHandler
{
   

    public void Reader()
    {
    }

    public void Writer()
    {
    }


    /*
    using var client = new TcpClient();

    await client.ConnectAsync(hostname, port);

        using NetworkStream stream = client.GetStream();

        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream)
    {
        AutoFlush = true,
        NewLine = "\r\n"
    };

    string? greeting = await reader.ReadLineAsync();
    Console.WriteLine($"Server: {greeting}");


    string? prompt = Console.ReadLine();
    await writer.WriteLineAsync(prompt);

    string? response;

        while ((response = await reader.ReadLineAsync()) != null)
    {
        Console.WriteLine(response);

        if (response == ".")
            break;
    }

    await writer.WriteLineAsync("QUIT");
    */
}