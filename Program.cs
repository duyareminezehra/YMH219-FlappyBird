using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using YMH219_FlappyBird.Forms;

namespace YMH219_FlappyBird
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            if (args.Contains("--generate-screenshots") || Environment.GetCommandLineArgs().Any(a => a.Contains("--generate-screenshots")))
            {
                try
                {
                    using var form = new MainForm();
                    string screenshotsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "screenshots");
                    string targetPath = Path.GetFullPath(screenshotsDir);
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_screenshot.log"), "Target: " + targetPath + Environment.NewLine);
                    form.SaveDocumentationScreenshots(targetPath);
                    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_screenshot.log"), "Success!" + Environment.NewLine);
                    form.Close();
                }
                catch (Exception ex)
                {
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_screenshot.log"), "Error: " + ex.ToString());
                }
                return;
            }

            Application.Run(new MainForm());
        }
    }
}
