using System.Text;
using System.Windows;

namespace ThaiIDCardReader;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Register TIS-620 encoding provider for Thai language support
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}
