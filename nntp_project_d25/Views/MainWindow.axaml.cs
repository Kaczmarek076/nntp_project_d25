using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using nntp_project_d25.Service.TCP;
using nntp_project_d25.ViewModels;

namespace nntp_project_d25.Views;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel { get; }
    private readonly TcpConnectionService _connectionService;
    private Semaphore _isBusySemaphore = new(1, 1);


    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// Sets up UI components, establishes the view model binding, and triggers the initial server greeting routine.
    /// </summary>
    /// <param name="connectionService">The TCP connection service injected for managing network communication.</param>
    public MainWindow(TcpConnectionService connectionService)
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        DataContext = ViewModel;

        _connectionService = connectionService;
        GetServerGreeting();
    }


    private static readonly string ServerGreeting = "CAPABILITIES";

    /// <summary>
    /// Asynchronously retrieves the server's initial greeting, requests its supported 
    /// capabilities, and populates the view model with the available commands.
    /// </summary>
    ///<remarks>
    /// Uses a non-blocking semaphore check to prevent overlapping command requests.
    /// </remarks>
    private async void GetServerGreeting()
    {
        if (!_isBusySemaphore
                .WaitOne(0)) // Prevent concurrent execution; exit immediately if another request is active.
            return;

        try
        {
            string? greeting = await _connectionService.ReadLineAsync(); // Read the initial greeting line
            ViewModel.CommandList = greeting + Environment.NewLine;
            await _connectionService.SendLineAsync(ServerGreeting); // Request the server's supported command set.
            List<string> answer = await _connectionService.ReadMultilineResponseAsync();

            foreach (var commands in answer) // Process and append each command capability to the UI view model
            {
                ViewModel.CommandList += commands + Environment.NewLine;
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Network error: " + ex.Message);
        }
        catch (IOException ex)
        {
            Console.WriteLine("Read eroor or timeout: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            _isBusySemaphore.Release(); // preventing permanent lockouts if an exception occurs.
        }
    }


    private static readonly string UserLoginString = "authinfo user ";
    private static readonly string PassLoginString = "authinfo pass ";

    /// <summary>
    /// Handles the login button click event by transmitting user credentials 
    /// over the network connection and updating the command log.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Login(object? sender, RoutedEventArgs e)
    {
        if (!_isBusySemaphore
                .WaitOne(0)) // Prevent concurrent execution; exit immediately if another request is active.
            return;
        try
        {
            string message =
                UserLoginString + ViewModel.Username +
                Environment.NewLine; // Transmit the username command and credential.
            await _connectionService.SendLineAsync(message);
            message = await _connectionService.ReadLineAsync() +
                      Environment
                          .NewLine; // Read and discard the server's immediate response line to stay synchronized with the protocol.

            message = PassLoginString + ViewModel.Password +
                      Environment.NewLine; // Transmit the password command and credential.
            await _connectionService.SendLineAsync(message);
            ViewModel.CommandList += await _connectionService.ReadLineAsync() +
                                     Environment
                                         .NewLine; // Read the final authentication response and append it to the command log.
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Network error: " + ex.Message);
        }
        catch (IOException ex)
        {
            Console.WriteLine("Read eroor or timeout: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            _isBusySemaphore.Release(); // preventing permanent lockouts if an exception occurs.
        }
    }


    /// <summary>
    /// Handles key down events on the input element, transmitting the user's 
    /// command to the server when the Enter key is pressed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key.Equals(Key.Enter)) // Guard clause: Exit immediately if the pressed key is not Enter
        {
            if (!_isBusySemaphore
                    .WaitOne(0)) // Prevent concurrent execution; exit immediately if another request is active.
                return;
            try
            {
                string message = ViewModel.IndputCommand + Environment.NewLine;
                await _connectionService.SendLineAsync(message);

                List<string>
                    answer = await _connectionService
                        .ReadMultilineResponseAsync(); // Read the block of lines comprising the server's multiline response.

                foreach (var commands in answer) // Append each line of the server's response to the UI view model log.
                {
                    ViewModel.CommandList += commands + Environment.NewLine;
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Network error: " + ex.Message);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Read eroor or timeout: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                _isBusySemaphore.Release(); // preventing permanent lockouts if an exception occurs.
                ViewModel.IndputCommand = string.Empty;
            }
        }
    }
}