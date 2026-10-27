using CommunityToolkit.Mvvm.ComponentModel;

namespace nntp_project_d25.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";
}
