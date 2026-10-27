using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace nntp_project_d25.Views;

public partial class MainWindow : Window
{
    private readonly TcpConnectionService _connectionService;

    public MainWindow()
    {
        InitializeComponent();
        _connectionService = new TcpConnectionService();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs reEventArgs)
    {
        const string host = "news.eternal-september.org"; // en åben citat-server 
        const int port = 119;

        try
        {
            using TcpClient client = new TcpClient();
            client.Connect(host, port);
            using NetworkStream ns = client.GetStream();

            using StreamReader reader = new StreamReader(
                ns,
                Encoding.ASCII,
                false,
                1024,
                leaveOpen: true
            );
            using StreamWriter writer = new StreamWriter(
                ns,
                Encoding.ASCII,
                leaveOpen: true,
                bufferSize: 1024);

            string? svar = await reader.ReadLineAsync();
            Console.WriteLine("Serverens svar: " + svar + Environment.NewLine);
            
            await writer.WriteLineAsync("list" + Environment.NewLine);
            await writer.FlushAsync();;
            svar = await reader.ReadLineAsync();
            Console.WriteLine(svar + Environment.NewLine);
            if (svar?.StartsWith("215 ") == true)
            {
                while ((svar = await reader.ReadLineAsync()) != null)
                {
                    // NNTP marks the end of a multiline response with a single dot.
                    if (svar == ".")
                        break;

                    // Handle NNTP dot-stuffing.
                    if (svar.StartsWith(".."))
                        svar = svar[1..];

                    Console.WriteLine(svar);
                }
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Netværksfejl: " + ex.Message);
        }
        catch (IOException ex)
        {
            Console.WriteLine("Læsefejl eller timeout: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fejl: " + ex.Message);
        }
    }
}