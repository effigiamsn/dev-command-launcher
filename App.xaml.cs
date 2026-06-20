using DevCommandLauncherApp.ViewModels;
using Microsoft.UI.Xaml;

namespace DevCommandLauncherApp;

public partial class App : Application
{
    private Window? _window;

    public static MainViewModel ViewModel { get; private set; } = null!;
    public static MainWindow? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        ViewModel = new MainViewModel();
        MainWindow = new MainWindow();
        _window = MainWindow;
        _window.Activate();
    }
}
