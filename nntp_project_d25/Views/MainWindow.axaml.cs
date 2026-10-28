using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Avalonia.Controls;
using Avalonia.Interactivity;
using nntp_project_d25.Service.TCP;
using nntp_project_d25.ViewModels;

namespace nntp_project_d25.Views;

public partial class MainWindow : Window
{
    private readonly TcpConnectionService _connectionService;
    private MainViewModel ViewModel { get; }

    public MainWindow(TcpConnectionService connectionService)
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        DataContext = ViewModel;

        _connectionService = connectionService;
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs reEventArgs)
    {
        try
        {
            Console.WriteLine("test");
            string? svar = await _connectionService.ReadLineAsync();
            
            Console.WriteLine(svar + Environment.NewLine);

            await _connectionService.SendLineAsync("list" +  Environment.NewLine);
            List<string> answer = await _connectionService.ReadMultilineResponseAsync();
            
            
            foreach (var commands in answer)
            {
                ViewModel.CommandList += commands + Environment.NewLine;
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