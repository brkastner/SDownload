using System;
using System.Net.Mime;
using System.Windows.Forms;

namespace SDownload.Framework.Streams
{
    public class StreamFactory
    {
        /// <summary>
        /// Parse the given link and send it to the appropriate stream downloader
        /// </summary>
        /// <param name="url">The link provided to the application from the view</param>
        /// <param name="view">The view to report progress back to</param>
        /// <param name="exit">Exit after download?</param>
        public static async void DownloadTrack(String url, InfoReportProxy view, bool exit = false)
        {
            CrashHandler.AddExtra("stream_url", url);
            try
            {
                BaseStream sound;
                if (url.Contains(@"/sets/"))
                    sound = new SCSetStream(url, view);
                else
                    sound = new SCTrackStream(url, view);

                var download = sound.Download();

                if (download != null && await download)
                    sound.Finish();
            }
            catch (Exception e)
            {
                CrashHandler.Throw("There was an issue downloading the stream!", e);
            }
            finally
            {
                CrashHandler.ClearExtras();
                if (exit)
                {
                    Application.Exit();
                }
            }
        }
    }
}
