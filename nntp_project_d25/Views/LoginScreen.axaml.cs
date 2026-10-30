using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using nntp_project_d25.Service.TCP;
using nntp_project_d25.ViewModels;

namespace nntp_project_d25.Views;

public partial class LoginScreen : Window
{
    
    private readonly TcpConnectionService _connectionService;
    public LoginViewModel LoginViewModel { get; }
    public LoginScreen()
    {
        InitializeComponent();
        _connectionService = new TcpConnectionService();
        LoginViewModel = new LoginViewModel();
        DataContext = LoginViewModel;
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await _connectionService.ConnectAsync(
                LoginViewModel.ServerHostName,
                LoginViewModel.Port);
            
            ChangeView(_connectionService);

            Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void ChangeView(TcpConnectionService connectionService)
    {
        var mainWindow = new MainWindow(connectionService);
        mainWindow.Show();
        Close();    
    }
}