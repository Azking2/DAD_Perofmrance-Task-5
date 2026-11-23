using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HubLink.Services;
using HubLink.ViewModels;
using HubLink.Views;

namespace HubLink;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Wire everything together: settings -> service -> ViewModel -> View
            var settings = AppSettings.Load();
            var service = new ShipmentService(settings);
            var viewModel = new MainWindowViewModel(service, settings);

            var window = new MainWindow { DataContext = viewModel };
            viewModel.PickSavePathAsync = window.PickSavePathAsync;

            // Load shipments automatically once the window is shown
            window.Opened += (_, _) => viewModel.RefreshCommand.Execute(null);

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
