using DevCommandLauncherApp.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace DevCommandLauncherApp.Views;

public sealed partial class DashboardPage : Page
{
    private MainViewModel ViewModel => App.ViewModel;

    public DashboardPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.DetectExistingServersAsync();
    }

    private void OnToggleSectionCollapsedClick(object sender, RoutedEventArgs e)
    {
        ((sender as FrameworkElement)?.Tag as ProjectViewModel)?.ToggleCollapsed();
    }
    private async void OnStartClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.StartAsync((sender as FrameworkElement)?.Tag as CommandViewModel);
    }

    private void OnStopClick(object sender, RoutedEventArgs e)
    {
        ViewModel.Stop((sender as FrameworkElement)?.Tag as CommandViewModel);
    }

    private async void OnRestartClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.RestartAsync((sender as FrameworkElement)?.Tag as CommandViewModel);
    }

    private void OnLogsClick(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectLogs((sender as FrameworkElement)?.Tag as CommandViewModel);
        (App.MainWindow as MainWindow)?.NavigateToLogs();
    }

    private async void OnOpenUrlClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is CommandViewModel command &&
            command.ServerUri is { } uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }
    }

    private void OnCopyUrlClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not CommandViewModel command ||
            string.IsNullOrWhiteSpace(command.ServerAddress))
        {
            return;
        }

        var package = new DataPackage();
        package.SetText(command.ServerAddress);
        Clipboard.SetContent(package);
    }

    private async void OnStartAllClick(object sender, RoutedEventArgs e)
    {
        foreach (var command in ViewModel.Projects.SelectMany(x => x.Commands).ToList())
        {
            await ViewModel.StartAsync(command);
        }
    }

    private void OnStopAllClick(object sender, RoutedEventArgs e)
    {
        foreach (var command in ViewModel.Projects.SelectMany(x => x.Commands).ToList())
        {
            ViewModel.Stop(command);
        }
    }

    private async void OnReloadClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ReloadConfiguration();
        await ViewModel.DetectExistingServersAsync();
    }
}
