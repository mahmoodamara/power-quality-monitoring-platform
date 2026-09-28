using System.Windows;
using PowerQuality.Desktop.Services;
using PowerQuality.Desktop.ViewModels;
namespace PowerQuality.Desktop;
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    public MainWindow()
    {
        InitializeComponent();
        var api = new ApiClient("http://localhost:8080");
        var realtime = new RealtimeClient("http://localhost:8080/hubs/monitoring");
        _viewModel = new MainViewModel(api, realtime);
        DataContext = _viewModel;
        Loaded += async (_, _) => await _viewModel.InitializeAsync();
        Closed += async (_, _) => await _viewModel.DisposeAsync();
    }
}
