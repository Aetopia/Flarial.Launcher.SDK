using System;
using System.IO;
using System.Windows.Forms;

static class Program
{
    static void Main()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Flarial.Launcher");
        Directory.CreateDirectory(path); Directory.SetCurrentDirectory(path);
        var _ = Minecraft.UWP.Game.VersionAsync().Result;
        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form());
    }
}