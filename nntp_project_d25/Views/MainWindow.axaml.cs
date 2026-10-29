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
    private bool _isBusy;

    public MainWindow(TcpConnectionService connectionService)
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        DataContext = ViewModel;

        _connectionService = connectionService;
        GetServerGreeting();

        //ServerLoop();
    }

    private async void GetServerGreeting()
    {
        string? greeting = await _connectionService.ReadLineAsync();
        ViewModel.CommandList = greeting + Environment.NewLine;
        await _connectionService.SendLineAsync("CAPABILITIES");
        List<string> answer = await _connectionService.ReadMultilineResponseAsync();

        foreach (var commands in answer)
        {
            ViewModel.CommandList += commands + Environment.NewLine;
            Console.WriteLine(commands);
        };
        
    }

    
    private async void Button_OnClick(object? sender, RoutedEventArgs reEventArgs)
    {
        if (_isBusy)
            return;
        _isBusy = true;
        try
        {
            string message = ViewModel.Password + Environment.NewLine;
            await _connectionService.SendLineAsync(message);
            List<string> answer = await _connectionService.ReadMultilineResponseAsync();

            foreach (var commands in answer)
            {
                ViewModel.CommandList += commands + Environment.NewLine;
                Console.WriteLine(commands);
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
        finally
        {
            _isBusy = false;
        }
    }


    public async void ServerLoop()
    {
        while (true)
        {
            try
            {
                string message = ViewModel.Password + Environment.NewLine;
                await _connectionService.SendLineAsync(message + Environment.NewLine);
                List<string> answer = await _connectionService.ReadMultilineResponseAsync();

                foreach (var commands in answer)
                    ViewModel.CommandList += commands + Environment.NewLine;
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
}