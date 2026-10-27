using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace nntp_project_d25.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs reEventArgs)
    {
        string hostname = "news.eternal-september.org";
        int port = 119;

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
    }
}