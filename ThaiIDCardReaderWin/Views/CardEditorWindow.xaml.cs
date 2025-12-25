using System.Windows;
using ThaiIDCardReader.ViewModels;
using ThaiIDCardReader.Services;

namespace ThaiIDCardReader.Views;

public partial class CardEditorWindow
{
    public CardEditorWindow(SmartCardService smartCardService, string? selectedReader = null)
    {
        InitializeComponent();

        var viewModel = new CardEditorViewModel(smartCardService);
        if (!string.IsNullOrEmpty(selectedReader))
        {
            viewModel.SelectedReader = selectedReader;
        }

        DataContext = viewModel;
    }
}
