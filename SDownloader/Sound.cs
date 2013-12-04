using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using BugSense.Core.Model;
using BugSense;
using SDownload.Dialogs;
using SDownload.Framework;
using TagLib;
using File = System.IO.File;
using SFile = TagLib.File;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Resources = SDownload.Properties.Resources;

namespace SDownload
{
    /// <summary>
    /// Represents a Song file downloaded from SoundCloud
    /// </summary>
    public class Sound
    {
        private const String Clientid = "4515286ec9d4ace678140c3f84357b35";

        private TrackData _trackData;
        private String _title;
        private String _author;
        private String _origUrl;
        public String Genre;

        protected InfoReportProxy View;

        protected DownloadItem MainResource;
        protected List<DownloadItem> Extras = new List<DownloadItem>();

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
        /// Add the song to iTunes
        /// </summary>
        private void AddToiTunes()
        {
            var newdir = String.Format("{0}\\iTunes\\iTunes Media\\Automatically Add to iTunes\\{1}.{2}", 
                Settings.CustomITunesLocation ?? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), GetCleanFileName(_title), 
                MainResource.AbsolutePath.Substring(MainResource.AbsolutePath.Length-3));

            // TODO: Ask the user first
            if (File.Exists(newdir)) 
                File.Delete(newdir);

            try
            {
                switch (Settings.TunesTransfer)
                {
                    case Settings.TunesSetting.Move:
                        {
                            BugSenseHandler.Instance.LeaveBreadCrumb("Moving song to iTunes");
                            File.Move(MainResource.AbsolutePath, newdir);

                            // Delete the artist folder if empty
                            if (Settings.AuthorFolder && MainResource.AbsolutePath.StartsWith(Settings.DownloadFolder + _author) 
                                && !Directory.EnumerateFileSystemEntries(Settings.DownloadFolder + _author).Any())
                            {
                                Directory.Delete(Settings.DownloadFolder + _author);
                            }
                            break;
                        }
                    case Settings.TunesSetting.Copy:
                        BugSenseHandler.Instance.LeaveBreadCrumb("Copying song to iTunes");
                        File.Copy(MainResource.AbsolutePath, newdir);
                        break;
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Find iTunes location if it exists
                var dialog = new YesNoDialog(Resources.ErrorCantFindITunes, "Locate", "Disable")
                                 {
                                     ResponseCallback = result =>
                                                            {
                                                                if (result)
                                                                {
                                                                    // User would like to find installation on disk
                                                                    var folderBrowser = new FolderBrowserDialog
                                                                                            {
                                                                                                Description = Resources.FolderBrowserDescriptionFindITunes
                                                                                            };
                                                                    folderBrowser.ShowDialog();
                                                                    // TODO: Better validate if this is a correct iTunes directory
                                                                    if (folderBrowser.SelectedPath.EndsWith("iTunes"))
                                                                    {
                                                                        // Valid iTunes installation
                                                                        Settings.CustomITunesLocation =
                                                                            folderBrowser.SelectedPath;
                                                                    }
                                                                } else
                                                                {
                                                                    // User wants to disable iTunes functionality
                                                                    Settings.TunesTransfer =
                                                                        Settings.TunesSetting.Nothing;
                                                                }
                                                            }
                                 };
                dialog.Show();
            }
        }

        /// <summary>
        /// Update the ID3 tags
        /// </summary>
        private void UpdateId3Tags()
        {
            // Load the song file
            var song = SFile.Create(Extras[0].AbsolutePath);
            if (song == null)
                return;

            // Title
            if (!String.IsNullOrEmpty(_title))
                song.Tag.Title = _title;

            // Artist (Performer and Album Artist)
            if (!String.IsNullOrEmpty(_author))
            {
                var authorTag = new[] {_author};
                song.Tag.Performers = authorTag;
                song.Tag.AlbumArtists = authorTag;
            }

            // Genre
            if (!String.IsNullOrEmpty(Genre))
                song.Tag.Genres = new[] {Genre};

            // Album Art
            song.Tag.Pictures = new IPicture[] {new Picture(Extras[0].AbsolutePath)};

            song.Save();
        }

        /// <summary>
        /// Gather all the necessary information for downloading the actual remote resource
        /// </summary>
        /// <param name="url">The URL to the individual song</param>
        /// <param name="view">The connection associated with the browser extension</param>
        /// <returns>A Sound representation of the remote resource</returns>
        public static Sound Parse(String url, InfoReportProxy view)
        {
            view.Report("Processing");
            BugSenseHandler.Instance.AddCrashExtraData(new CrashExtraData { Key = "song_url", Value = url });
            TrackData track;
            try
            {
                const String resolveUrl = "http://api.soundcloud.com/resolve?url={0}&client_id={1}";
                var request = (HttpWebRequest)WebRequest.Create(String.Format(resolveUrl, url, Clientid));
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/json";
                var response = request.GetResponse().GetResponseStream();
                if (response == null)
                    throw new HandledException("Soundcloud API failed to respond! This could due to an issue with your connection.");
                track = new DataContractJsonSerializer(typeof(TrackData)).ReadObject(response) as TrackData;
                if (track == null)
                    throw new HandledException("Downloaded track information was corrupted!", true);
            }
            catch (Exception e)
            {
                HandledException.Throw("There was an issue connecting to the Soundcloud API.", e);
                return null;
            }

            var tokens = WebUtility.HtmlDecode(track.Title).Split('-');
            var author = track.User.Username;
            var title = track.Title;

            // Split the song name if it contains the Author
            if (tokens.Length > 1)
            {
                BugSenseHandler.Instance.LeaveBreadCrumb("Song name split");
                author = tokens[0].Trim();
                title = tokens[1].Trim();
            }

            var rand = GenerateRandomString(8) + ".mp3";

            var directory = Settings.DownloadFolder + GetCleanFileName(Settings.AuthorFolder ? author : "");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Use the download url if it exists, probably better quality
            var songUrl = GetDownloadUrl(url, track);
            var absoluteUrl = directory + "\\" + GetCleanFileName(title) + ".mp3";
            var artworkUrl = track.ArtworkUrl ?? track.User.AvatarUrl;

            var extras = new List<DownloadItem>
                             {new DownloadItem(new Uri(artworkUrl), Path.GetTempPath() + "\\" + rand + ".jpg")};

            // Create and return the sound representation
            return new Sound
                       {
                           _trackData = track,
                           _title = title,
                           _author = author,
                           _origUrl = url,
                           Genre = track.Genre ?? "",
                           MainResource = new DownloadItem(new Uri(songUrl), absoluteUrl),
                           Extras = extras,
                           View = view
                       };
        }

        private static String GetDownloadUrl(String url, TrackData track, bool forceStream = false, bool forceManual = false)
        {
            var songDownload = (track.DownloadUrl != null && Settings.UseDownloadLink && !forceStream) ? track.DownloadUrl : track.StreamUrl;
            if (songDownload == null || forceManual)
            {
                // There was no stream URL or download URL for the song, manually parse the resource stream link from the original URL
                BugSenseHandler.Instance.LeaveBreadCrumb("Manually downloading sound");
                HttpWebResponse response;
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception e)
                {
                    HandledException.Throw("Song does not allow streaming and there was an issue manually downloading the song file!", e, false);
                    return null;
                }

                var doc = new HtmlDocument();
                doc.Load(response.GetResponseStream());
                var searchString = WebUtility.HtmlDecode(doc.DocumentNode.InnerHtml);
                var links = Regex.Matches(searchString, "((http:[/][/])(media.soundcloud.com/stream/)([a-z]|[A-Z]|[0-9]|[/.]|[~]|[?]|[_]|[=])*)");
                songDownload = links[0].Value;
            }

            return songDownload + "?client_id=" + Clientid;
        }

        /// <summary>
        /// Downloads the Sound representation
        /// </summary>
        public async Task<bool> Download(bool ignoreExtras = false)
        {
            View.Report("Downloading");
            // Download the main resource
            var mainDownloader = new WebClient();
            mainDownloader.DownloadProgressChanged += (sender, e) => View.Report(String.Format("{0}%", e.ProgressPercentage));
            var resourceDownload = mainDownloader.DownloadFileTaskAsync(MainResource.Uri, MainResource.AbsolutePath);

            IEnumerable<Task> extraTasks = null;

            // Download additional files
            if (!ignoreExtras)
            {
                extraTasks = Extras.Select(extra => new WebClient().DownloadFileTaskAsync(extra.Uri, extra.AbsolutePath));
            }

            try
            {
                await resourceDownload;
                var ret = Validate();
                if (extraTasks != null)
                    Task.WaitAll(extraTasks.ToArray());

                return ret;
            }
            catch (Exception e)
            {
                /* TODO:
                 * Some songs on Soundcloud cannot be streamed via the API or downloaded directly.
                 * The current direct method could possibly be altered to look for a different media stream.
                 */
                if (e.Message.Contains("Not Found"))
                {
                    View.Report("Streaming disabled by Artist :C", true);
                } 
                else
                {
                    View.Close();
                    HandledException.Throw(e.Message, e);
                }
            }
            return false;
        }

        public virtual bool Validate()
        {
            // TODO: Possibly find a better way to validate more file types quicker
            // perhaps by reading the resolved url ending
            SFile file = null;
            try
            {
                // Test if the file is a valid mp3
                SFile.Create(MainResource.AbsolutePath);
            } catch (CorruptFileException)
            {
                try
                {
                    // Check if the file is wma
                    var old = MainResource.AbsolutePath;
                    MainResource.AbsolutePath = MainResource.AbsolutePath.Substring(0,
                                                                                    MainResource.AbsolutePath.Length - 3) + "wma";
                    File.Move(old, MainResource.AbsolutePath);
                    file = SFile.Create(MainResource.AbsolutePath);
                } catch (CorruptFileException)
                {
                    File.Delete(MainResource.AbsolutePath);
                    // File not supported by validation, Use the stream download
                    View.Report("Retrying");
                    new WebClient().DownloadFile(new Uri(GetDownloadUrl(_origUrl, _trackData, true)), MainResource.AbsolutePath);
                }
            }

            return file != null;
        }

        public void Finish()
        {
            View.Report("Finalizing");
            try
            {
                UpdateId3Tags();
            } catch (Exception e1)
            {
                // Should have been handled already
                View.Report("Invalid file!", true);
                HandledException.Throw("Invalid file was downloaded!", e1);
                return;

            }
            AddToiTunes();

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
        private static String GenerateRandomString(int size)
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
        public static string GetCleanFileName(string value)
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
