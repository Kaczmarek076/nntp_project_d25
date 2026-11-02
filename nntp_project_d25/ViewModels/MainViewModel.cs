using CommunityToolkit.Mvvm.ComponentModel; 

namespace nntp_project_d25.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] public partial string IndputCommand { get; set; } = "";
    [ObservableProperty] public partial string? CommandList { get; set; } = "";
    
    [ObservableProperty] public partial string? Username { get; set; } = "";
    [ObservableProperty] public partial string? Password { get; set; } = "";
    
}