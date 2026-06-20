using DevCommandLauncherApp.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevCommandLauncherApp.Views;

public sealed partial class SettingsPage : Page
{
    private MainViewModel ViewModel => App.ViewModel;

    public SettingsPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void OnAddProjectClick(object sender, RoutedEventArgs e) => ViewModel.AddProject();
    private void OnDeleteProjectClick(object sender, RoutedEventArgs e) => ViewModel.DeleteSelectedProject();
    private void OnAddCommandClick(object sender, RoutedEventArgs e) => ViewModel.AddCommand();
    private void OnDeleteCommandClick(object sender, RoutedEventArgs e) => ViewModel.DeleteSelectedCommand();
    private void OnSaveClick(object sender, RoutedEventArgs e) => ViewModel.SaveConfiguration();
    private void OnReloadClick(object sender, RoutedEventArgs e) => ViewModel.ReloadConfiguration();
}
