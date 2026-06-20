using DevCommandLauncherApp.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevCommandLauncherApp.Views;

public sealed partial class LogsPage : Page
{
    private MainViewModel ViewModel => App.ViewModel;

    public LogsPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void OnClearLogsClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearSelectedLogs();
    }
}
