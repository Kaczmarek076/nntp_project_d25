using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
        DataContext = this;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Login button pressed");
        
        _connectionService.ConnectAsync(LoginViewModel.ServerHostName, LoginViewModel.Port);
        ChangeView();
    }

    private void ChangeView()
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
        Close();    
    }
}