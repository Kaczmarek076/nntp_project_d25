using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using nntp_project_d25.Service.TCP;
using nntp_project_d25.ViewModels;

namespace nntp_project_d25.Views;

/// <summary>
/// Represents the login window where the user provides the server hostname 
/// and port to establish an initial TCP network connection.
/// </summary>
public partial class LoginScreen : Window
{
    
    /// <summary>
    /// Gets the view model responsible for handling login state and user inputs.
    /// </summary>
    public LoginViewModel LoginViewModel { get; }
    private readonly TcpConnectionService _connectionService;

    
    
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginScreen"/> class, 
    /// setting up data context bindings and the underlying connection service.
    /// </summary>
    public LoginScreen()
    {
        InitializeComponent();
        _connectionService = new TcpConnectionService();
        LoginViewModel = new LoginViewModel();
        DataContext = LoginViewModel;
    }
    
    
    
    /// <summary>
    /// Handles the login button click event, attempting to establish a TCP connection 
    /// to the specified server before transitioning to the main application window.
    /// </summary>
    /// <param name="sender">The source of the event (the login button).</param>
    /// <param name="e">Event data associated with the routed event.</param>
    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await
                _connectionService.ConnectAsync( // Attempt to connect asynchronously using parameters from the view model.
                        LoginViewModel.ServerHostName,
                        LoginViewModel.Port);

            ChangeView(_connectionService); // Transition to the main application window, passing the live connection service.

            Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }


    /// <summary>
    /// Instantiates and displays the main window, passing the established 
    /// network connection, and closes the current login window.
    /// </summary>
    /// <param name="connectionService">The active, connected TCP service instance.</param>
    private void ChangeView(TcpConnectionService connectionService)
    {
        var mainWindow = new MainWindow(connectionService);
        mainWindow.Show();
        Close();
    }
}