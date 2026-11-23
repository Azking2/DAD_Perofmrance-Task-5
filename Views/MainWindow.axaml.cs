using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace HubLink.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Only opens the "Save file" dialog. All export logic is in the ViewModel/service.
    public async Task<string?> PickSavePathAsync(string suggestedFileName)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export shipments to CSV",
            SuggestedFileName = suggestedFileName,
            DefaultExtension = "csv",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("CSV file") { Patterns = new[] { "*.csv" } }
            }
        });

        return file?.TryGetLocalPath();
    }
}
