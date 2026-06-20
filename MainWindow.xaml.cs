using DevCommandLauncherApp.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using Windows.UI.ViewManagement;

namespace DevCommandLauncherApp;

public sealed partial class MainWindow : Window
{
    public ViewModels.MainViewModel ViewModel => App.ViewModel;

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = false;
        ApplySystemTitleBarColors();
        ApplyWindowIcon();
        AppWindow.Resize(new SizeInt32(1260, 780));
        RootNavigation.SelectedItem = RootNavigation.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    public void NavigateToLogs()
    {
        RootNavigation.SelectedItem = RootNavigation.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(item => (string?)item.Tag == "Logs");
        NavigateTo(typeof(LogsPage));
    }

    private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is not string tag)
        {
            return;
        }

        var pageType = tag switch
        {
            "Settings" => typeof(SettingsPage),
            "Logs" => typeof(LogsPage),
            _ => typeof(DashboardPage)
        };

        NavigateTo(pageType);
    }

    private void NavigateTo(Type pageType)
    {
        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }

    private void ApplySystemTitleBarColors()
    {
        var settings = new UISettings();
        var background = settings.GetColorValue(UIColorType.Background);
        var foreground = settings.GetColorValue(UIColorType.Foreground);
        var titleBar = AppWindow.TitleBar;

        titleBar.BackgroundColor = background;
        titleBar.ButtonBackgroundColor = background;
        titleBar.InactiveBackgroundColor = background;
        titleBar.ButtonInactiveBackgroundColor = background;
        titleBar.ForegroundColor = foreground;
        titleBar.ButtonForegroundColor = foreground;
        titleBar.InactiveForegroundColor = foreground;
        titleBar.ButtonInactiveForegroundColor = foreground;
    }

    private void ApplyWindowIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        if (File.Exists(iconPath))
        {
            AppWindow.SetIcon(iconPath);
        }
    }
}
