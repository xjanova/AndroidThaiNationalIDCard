using System.Windows;
using ThaiIDCardReader.ViewModels;

namespace ThaiIDCardReader;

public partial class MainWindow
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize the card reader on window load
        await ViewModel.InitializeCommand.ExecuteAsync(null);
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Clean up resources
        ViewModel.Cleanup();
    }
}
