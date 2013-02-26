using System;
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
                Sound.Notify.Show("Fetching Information...");
                Sound song = Sound.Download(link);
                song.Update();
                song.AddToMusic();
                Sound.Notify.Show(String.Format("{0} download completed!", song.Title), true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
        }
    }
}
