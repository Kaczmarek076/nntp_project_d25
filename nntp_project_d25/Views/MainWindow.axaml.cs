using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Avalonia.Controls;
using Avalonia.Input;
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
        }

        ;
    }
    
    private async void Button_Login(object? sender, RoutedEventArgs e)
    {
        try
        {
            string message = "authinfo user " + ViewModel.Username + Environment.NewLine;
            await _connectionService.SendLineAsync(message);
            ViewModel.CommandList += await _connectionService.ReadLineAsync() + Environment.NewLine;
            
            message = "authinfo pass " + ViewModel.Password + Environment.NewLine;
            await _connectionService.SendLineAsync(message);
            ViewModel.CommandList += await _connectionService.ReadLineAsync() + Environment.NewLine;
            
            ViewModel.CommandList += await _connectionService.ReadLineAsync() + Environment.NewLine;
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

    private async void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key.Equals(Key.Enter))
        {
            if (_isBusy)
                return;
            _isBusy = true;
            try
            {
                string message = ViewModel.IndputCommand + Environment.NewLine;
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
                ViewModel.IndputCommand = string.Empty;
            }
        }
    }
}