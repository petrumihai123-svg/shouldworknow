using System.Globalization;
using System.Text;

namespace PortableWinFormsRecorder;

internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // CLI mode if args present
        if (args.Length > 0)
            return Cli.Run(args);

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }
}
