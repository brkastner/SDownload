using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using SDownload.Framework.Models;

namespace SDownload.Framework.Streams
{
    public class SCSetStream : BaseStream
    {
        /// <summary>
        /// Keys: Tracks in the playlist to be processed
        /// Values: The track's download task
        /// </summary>
        private IDictionary<SCTrackStream, Task<bool>> _downloads = new Dictionary<SCTrackStream, Task<bool>>();

        /// <summary>
        /// The percentage completed the download of this entire set is
        /// </summary>
        private int _percentDownloaded;

        /// <summary>
        /// The amount of failed downloads
        /// </summary>
        private int _failed;

        /// <summary>
        /// Internal progress reporting proxy for handling tracking progress of multiple downloads but only sending
        /// one % to the actual view
        /// </summary>
        private class SetItemReportProxy : InfoReportProxy
        {
            /// <summary>
            /// The actual set handler
            /// </summary>
            private readonly SCSetStream _master;

            /// <summary>
            /// The percent completed the download of this item is
            /// </summary>
            private int _percentage;

            /// <summary>
            /// The total percentage to count up to
            /// </summary>
            private readonly int _totalPercentage;

            /// <summary>
            /// Creates a set item proxy
            /// </summary>
            /// <param name="master">The actual set handler</param>
            /// <param name="downloadCount">The total amount of downloads</param>
            public SetItemReportProxy(SCSetStream master, int downloadCount)
            {
                _totalPercentage = downloadCount*100;
                _master = master;
            }

            /// <summary>
            /// Stores the progress for the track and updates the master progress
            /// </summary>
            /// <param name="percentage">The percentage complete the track ownlaod is</param>
            public override void UpdateProgress(int percentage)
            {
                _percentage = percentage;
                _master._percentDownloaded += percentage;
                _master.View.UpdateProgress(_master._percentDownloaded/_totalPercentage);
            }

            /// <summary>
            /// Closes the set item being tracked, 
            /// sets the progress to -1 if being closed before download is complete
            /// </summary>
            public override void Close()
            {
                // Set the download for this item to 100% 
                if (_percentage != 100)
                    _master._percentDownloaded += (100 - _percentage);
            }
        }

        /// <summary>
        /// Gathers and prepares all of the tracks in the set for download
        /// </summary>
        /// <param name="url">The url to the set</param>
        /// <param name="view">The view to report progress back to</param>
        public SCSetStream(string url, InfoReportProxy view) : base(url, view)
        {
            var playlistData = SCPlaylistData.LoadData(url);
            if (playlistData == null)
                throw new HandledException("Downloaded set information was corrupted!", true);

            if (playlistData.Tracks.Length < 1)
                throw new HandledException("Playlist does not contain any tracks!", true);

            for (var index = 0; index < playlistData.Tracks.Length; index++)
            {
                View.Report(String.Format("Processing track {0}/{1}", index+1, playlistData.Tracks.Length));
                var track = playlistData.Tracks[index];
                var setItemReporter = new SetItemReportProxy(this, playlistData.Tracks.Length);
                try
                {
                    var trackHandler = new SCTrackStream(track.PermalinkUrl, setItemReporter, track);
                    _downloads.Add(trackHandler, null);
                }
                catch (Exception)
                {
                    // Mark the item failed if it couldn't be processed
                    setItemReporter.Close();
                }
            }
        }

        /// <summary>
        /// Downloads the necessary files
        /// </summary>
        /// <param name="ignoreExtras">Not used, extras aren't tracked by set</param>
        /// <returns>A task representation for keeping track of the method's progress</returns>
        public override async Task<bool> Download(bool ignoreExtras = false)
        {
            // Start the download for each track
            _downloads = _downloads.ToDictionary(d => d.Key, d => d.Key.Download());

            IList<SCTrackStream> failedDownloads = new List<SCTrackStream>();

            // Wait for each task and count the amount of failures
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var pair in _downloads)
            {
                try
                {
                    if (!(await pair.Value))
                        failedDownloads.Add(pair.Key);
                }
                catch (Exception)
                {
                    failedDownloads.Add(pair.Key);
                }
            }
            // ReSharper restore LoopCanBeConvertedToQuery

            foreach (var failure in failedDownloads)
                _downloads.Remove(failure);

            _failed = failedDownloads.Count;

            // Return true if one or more was successful so it can be finished
            return _downloads.Count > 0;
        }

        /// <summary>
        /// Performs any post-download steps for all of the songs in the set
        /// </summary>
        /// <param name="close">Not used</param>
        /// <returns>Always true</returns>
        public override bool Finish(bool close = true)
        {
            var i = 1;
            // Attempt to finish each one, count any errors
            foreach (var download in _downloads.Keys)
            {
                View.Report(String.Format("Finalizing track {0}/{1}", i++, _downloads.Keys.Count));
                if (!download.Finish())
                    _failed++;
            }

            // Don't close the view, need to report errors after
            // Still want to clear debugging data though
            base.Finish(false);
            View.Report("Done!" + (_failed > 0 ? " Failed: " + _failed : ""), true);
            return true;
        }
    }
}