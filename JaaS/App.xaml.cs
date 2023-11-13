using System.Windows;

namespace JaaS;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        MainWindow window = new MainWindow();
        var viewModel = new MainViewModel();
        viewModel.RequestClose += window.Close;
        window.Model = viewModel;
        window.DataContext = viewModel;
        window.Show();
    }
}
