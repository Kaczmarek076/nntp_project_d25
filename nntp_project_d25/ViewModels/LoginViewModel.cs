using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace nntp_project_d25.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty] public partial string ServerHostName { get; set; } = "";
    [ObservableProperty] public partial int Port { get; set; } = 0;
}