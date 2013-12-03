using System;
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
        public String Title;
        public String Author;
        public String Genre;
        private TrackData _trackData;
        private String _url;

        private InfoReportProxy view;

        private String _tempFile;

        private String _absolutePath;

        private Task _artworkDownloadTask;

        /// <summary>
        /// Add the song to iTunes
        /// </summary>
        public void AddToiTunes()
        {
            var newdir = String.Format("{0}\\iTunes\\iTunes Media\\Automatically Add to iTunes\\{1}.{2}", 
                Settings.CustomITunesLocation ?? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), GetCleanFileName(Title), 
                _absolutePath.Substring(_absolutePath.Length-3));
            if (File.Exists(newdir)) 
                File.Delete(newdir);
            try
            {
                switch (Settings.TunesTransfer)
                {
                    case Settings.TunesSetting.Move:
                        {
                            BugSenseHandler.Instance.LeaveBreadCrumb("Moving song to iTunes");
                            File.Move(_absolutePath, newdir);

                            // Delete the artist folder if empty
                            if (Settings.AuthorFolder && _absolutePath.StartsWith(Settings.DownloadFolder + Author) 
                                && !Directory.EnumerateFileSystemEntries(Settings.DownloadFolder + Author).Any())
                            {
                                Directory.Delete(Settings.DownloadFolder + Author);
                            }
                            break;
                        }
                    case Settings.TunesSetting.Copy:
                        BugSenseHandler.Instance.LeaveBreadCrumb("Copying song to iTunes");
                        File.Copy(_absolutePath, newdir);
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
        public void UpdateId3Tags()
        {
            var song = SFile.Create(_absolutePath);

            if (song == null)
                return;

            // Title
            if (!String.IsNullOrEmpty(Title))
                song.Tag.Title = Title;

            // Artist (Performer and Album Artist)
            if (!String.IsNullOrEmpty(Author))
            {
                var authorTag = new[] {Author};
                song.Tag.Performers = authorTag;
                song.Tag.AlbumArtists = authorTag;
            }

            // Genre
            if (!String.IsNullOrEmpty(Genre))
                song.Tag.Genres = new[] {Genre};

            // Album Art
            song.Tag.Pictures = new IPicture[] {new Picture(_tempFile + ".jpg")};

            song.Save();
        }

        /// <summary>
        /// Gather all the necessary information for downloading the actual remote resource
        /// </summary>
        /// <param name="url">The URL to the individual song</param>
        /// <param name="proxy">The connection associated with the browser extension</param>
        /// <returns>A Sound representation of the remote resource</returns>
        public static Sound Parse(String url, InfoReportProxy proxy)
        {
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
                    throw new HandledException("Soundcloud API failed to respond!");
                track = new DataContractJsonSerializer(typeof(TrackData)).ReadObject(response) as TrackData;
                if (track == null)
                    throw new HandledException("Could not deserialize the track information!", true);
            }
            catch (Exception e)
            {
                HandledException.Throw("Unable to make a connection to the specified URL.", e);
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

            // Create and return the sound representation
            return new Sound
                       {
                           _trackData = track,
                           _tempFile = Path.GetTempPath() + "\\" + rand,
                           Title = title,
                           Author = author,
                           Genre = track.Genre ?? "",
                           _url = url,
                           view = proxy,
                       };
        }

        /// <summary>
        /// Downloads the Sound representation
        /// </summary>
        public void Download(bool forceManual = false)
        {
            var directory = Settings.DownloadFolder + GetCleanFileName(Settings.AuthorFolder ? Author : "");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Use the download url if it exists, probably better quality
            var songDownload = (_trackData.DownloadUrl != null && Settings.UseDownloadLink) ? _trackData.DownloadUrl : _trackData.StreamUrl;
            if (songDownload == null || forceManual)
            {
                // There was no stream URL or download URL for the song, manually parse the resource stream link from the original URL
                BugSenseHandler.Instance.LeaveBreadCrumb("Manually downloading sound");
                HttpWebResponse response;
                try
                {
                    var request = (HttpWebRequest) WebRequest.Create(_url);
                    response = (HttpWebResponse) request.GetResponse();
                }
                catch (Exception e)
                {
                    HandledException.Throw("Song does not allow streaming and there was an issue manually downloading the song file!", e, false);
                    view.Close();
                    return;
                }
                var doc = new HtmlDocument();
                doc.Load(response.GetResponseStream());
                var searchString = WebUtility.HtmlDecode(doc.DocumentNode.InnerHtml);
                var links = Regex.Matches(searchString, "((http:[/][/])(media.soundcloud.com/stream/)([a-z]|[A-Z]|[0-9]|[/.]|[~]|[?]|[_]|[=])*)");
                songDownload = links[0].Value;
            }

            // Download the song and report progress to the browser
            var songDownloader = new WebClient();
            songDownloader.DownloadFileCompleted += SongFileDownloadCompleted;
            songDownloader.DownloadProgressChanged += (sender, e) => view.Report(String.Format("{0}%", e.ProgressPercentage));
            _absolutePath = directory + "\\" + GetCleanFileName(Title) + ".mp3";
            String songUrl = songDownload + "?client_id=" + Clientid;

            // Download the album art silently in the background, if it hasn't been done already
            if (_artworkDownloadTask == null)
            {
                var artDownloader = new WebClient();
                String artworkUrl = _trackData.ArtworkUrl ?? _trackData.User.AvatarUrl;
                _artworkDownloadTask = artDownloader.DownloadFileTaskAsync(new Uri(artworkUrl), _tempFile + ".jpg");
            }

            songDownloader.DownloadFileAsync(new Uri(songUrl), _absolutePath);
        }

        /// <summary>
        /// Keeps track of what still needs to be downloaded and performs any 
        /// necessary post-download tasks once all files have been downloaded
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Download Information</param>
        private async void SongFileDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // Soundcloud api has an issue with StreamUrl, need to download the song w/o API
                if (e.Error.Message.Contains("Not Found"))
                {
                    view.Report("Streaming disabled by Artist :C", true);
                    return;
                }
                throw new HandledException(e.Error.ToString(), true);
            }

            // Wait for the artwork to finish downloading
            await _artworkDownloadTask;

            view.Report("Validating");
            try
            {
                UpdateId3Tags();
            } 
            catch (CorruptFileException)
            {
                // Provided download link was not mp3, manually download file
                view.Report("Invalid Format, Retrying");
                var songDownloader = new WebClient();
                songDownloader.DownloadFileCompleted += SongFileDownloadCompleted;
                songDownloader.DownloadProgressChanged += (s, e2) => view.Report(String.Format("{0}%", e2.ProgressPercentage));
                songDownloader.DownloadFileAsync(new Uri(_trackData.StreamUrl + "?client_id=" + Clientid), _absolutePath);
                return;
            }

            AddToiTunes();

            view.Report("Done!", true);

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
