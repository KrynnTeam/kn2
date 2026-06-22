using System.Windows;

namespace ShadowCheat;

public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += (s, e) =>
        {
            System.IO.File.WriteAllText("crash.log", e.Exception.ToString());
            e.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            System.IO.File.WriteAllText("crash_domain.log", e.ExceptionObject.ToString());
        };
    }
}
