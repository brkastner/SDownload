using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BugSense;
using BugSense.Core.Model;

namespace SDownload.Framework.Streams
{
    /// <summary>
    /// Base stream that defines the flow the program follows for downloading a stream.
    /// This class can be overridden to provide specific functionality for a certain type of stream.
    /// </summary>
    public abstract class BaseStream
    {
        /// <summary>
        /// The view to report progress back to
        /// </summary>
        protected InfoReportProxy View;

        /// <summary>
        /// The main resource that needs to be downloaded
        /// </summary>
        protected DownloadItem MainResource;

        protected Exception LastException { get; private set; }

        /// <summary>
        /// A list of extras that need to be downloaded with the main resource
        /// </summary>
        protected List<DownloadItem> Extras = new List<DownloadItem>();

        /// <summary>
        /// Represents an item that needs to be downloaded
        /// </summary>
        protected class DownloadItem
        {
            /// <summary>
            /// The URI of the remote stream
            /// </summary>
            public Uri Uri;

            /// <summary>
            /// The absolute local path where it should be saved
            /// </summary>
            public String AbsolutePath;

            /// <summary>
            /// Creates an object to be downloaded
            /// </summary>
            /// <param name="uri">URI of the remote stream</param>
            /// <param name="absolutePath">Absolute path where it should be saved</param>
            public DownloadItem(Uri uri, String absolutePath)
            {
                Uri = uri;
                AbsolutePath = absolutePath;
            }
        }

        /// <summary>
        /// Creates a base stream that downloads from a remote url
        /// </summary>
        /// <param name="url">The url to process</param>
        /// <param name="view">The view to report information back to</param>
        protected BaseStream(String url, InfoReportProxy view)
        {
            View = view;

            View.Report("Processing");
        }

        /// <summary>
        /// Downloads the Sound representation and any extra files that accompany it
        /// <param name="ignoreExtras">If true, extra files will not be downloaded. (Useful for retrying the main download)</param>
        /// <returns>A boolean value representing if the download was successful or not</returns>
        /// </summary>
        public virtual async Task<bool> Download(bool ignoreExtras = false)
        {
            View.Report("Downloading...");
            try
            {
                // Download the main resource
                if (MainResource == null)
                {
                    CrashHandler.Throw("No resource has been registered for download!");
                    return false;
                }

                var mainDownloader = new WebClient();
                mainDownloader.DownloadProgressChanged += (sender, e) => View.UpdateProgress(e.ProgressPercentage);
                var resourceDownload = mainDownloader.DownloadFileTaskAsync(MainResource.Uri, MainResource.AbsolutePath);

                IEnumerable<Task> extraTasks = null;

                // Download additional files
                if (!ignoreExtras)
                    extraTasks = (from extra in Extras select DownloadExtra(extra)).ToList();

                await resourceDownload;
                var ret = Validate();
                if (extraTasks != null)
                    foreach (var extra in extraTasks) await extra;

                return await ret;
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return false;
        }

        /// <summary>
        /// Asynchronous method for downloading an item that does not track progress
        /// </summary>
        /// <param name="item">The item to download</param>
        /// <returns>A task representation of the asynchronous call</returns>
        private static async Task DownloadExtra(DownloadItem item)
        {
            await new WebClient().DownloadFileTaskAsync(item.Uri, item.AbsolutePath);
        }

        /// <summary>
        /// Validates the download. The base stream does not do any validation.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> Validate()
        {
            View.Report("Validating");
            return true;
        }

        /// <summary>
        /// Called once the whole process is finished. Clears any log data and closes the connection
        /// to the view.
        /// 
        /// If this method is overridden, base.Finish() should be called AT THE END of the function.
        /// </summary>
        public virtual bool Finish(bool close = true)
        {
            View.Report("Done!", close);

            BugSenseHandler.Instance.ClearCrashExtraData();
            BugSenseHandler.Instance.ClearBreadCrumbs();
            return true;
        }

        /// <summary>
        /// Generate a random string of a given length
        /// </summary>
        /// <param name="size">Length of the string</param>
        /// <see cref="http://stackoverflow.com/questions/1122483/c-sharp-random-string-generator"/>
        /// <returns>A randomized string </returns>
        protected static String GenerateRandomString(int size)
        {
            var builder = new StringBuilder();
            var random = new Random();
            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Get the clean filename for a title
        /// </summary>
        /// <param name="value">The title of the song</param>
        /// <returns>A cleaned filename for the given title</returns>
        protected static string GetCleanFileName(string value)
        {
            var sb = new StringBuilder(value);
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var item in invalid)
            {
                sb.Replace(item.ToString(CultureInfo.InvariantCulture), "");
            }
            sb.Replace("\\", "_");
            return sb.ToString();
        }
    }
}
