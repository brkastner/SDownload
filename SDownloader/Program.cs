using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SDownload
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(String[] args)
        {
            if (args.Length <= 0) return;
            try
            {
                String link = args[0].Substring(12);
                Sound.Notify = new NotifyHandler();
                Sound song = Sound.Download(link);
                Application.Run();
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                sb.AppendLine(e.ToString());
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace);
                MessageBox.Show(sb.ToString());
            }
        }
    }
}
