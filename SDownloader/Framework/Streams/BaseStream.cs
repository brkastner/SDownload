﻿using System;
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
    public abstract class BaseStream
    {
        protected InfoReportProxy View;

        protected DownloadItem MainResource;
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
            BugSenseHandler.Instance.AddCrashExtraData(new CrashExtraData { Key = "stream_url", Value = url });
        }

        /// <summary>
        /// Downloads the Sound representation and any extra files that accompany it
        /// <param name="ignoreExtras">If true, extra files will not be downloaded. (Useful for retrying the main download)</param>
        /// <returns>A boolean value representing if the download was successful or not</returns>
        /// </summary>
        public virtual async Task<bool> Download(bool ignoreExtras = false)
        {
            View.Report("Downloading");
            // Download the main resource
            if (MainResource == null)
            {
                HandledException.Throw("No resource has been registered for download!");
                return false;
            }

            var mainDownloader = new WebClient();
            mainDownloader.DownloadProgressChanged += (sender, e) => View.Report(String.Format("{0}%", e.ProgressPercentage));
            var resourceDownload = mainDownloader.DownloadFileTaskAsync(MainResource.Uri, MainResource.AbsolutePath);

            IEnumerable<Task> extraTasks = null;

            // Download additional files
            if (!ignoreExtras)
            {
                extraTasks = Extras.Select(extra => new WebClient().DownloadFileTaskAsync(extra.Uri, extra.AbsolutePath));
            }

            await resourceDownload;
            var ret = Validate();
            if (extraTasks != null)
                Task.WaitAll(extraTasks.ToArray());

            return ret;
        }

        public virtual bool Validate()
        {
            View.Report("Validating");
            return true;
        }

        public virtual void Finish()
        {
            View.Report("Done!", true);

            BugSenseHandler.Instance.ClearCrashExtraData();
            BugSenseHandler.Instance.ClearBreadCrumbs();
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
