using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Alchemy.Classes;
using BugSense.Core.Model;
using BugSense_WF;
using SDownload.Dialogs;
using SDownload.Framework;
using TagLib;
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
        private int _downloadedCount;
        private const int TotalToDownload = 2;

        private UserContext _browser;

        private String _filename;

        /// <summary>
        /// Add the song to iTunes
        /// </summary>
        public void AddToTunes()
        {
            var old = String.Format("{0}{1}\\{2}.mp3", Settings.DownloadFolder, Settings.AuthorFolder ? Author : "", GetFileName(Title));
            var newdir = String.Format("{0}\\iTunes\\iTunes Media\\Automatically Add to iTunes\\{1}.mp3", Settings.CustomITunesLocation ?? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), GetFileName(Title));
            if (System.IO.File.Exists(newdir)) 
                System.IO.File.Delete(newdir);
            try
            {
                switch (Settings.TunesTransfer)
                {
                    case Settings.TunesSetting.Move:
                        {
                            BugSenseHandler.Instance.LeaveBreadCrumb("Moving song to iTunes");
                            System.IO.File.Move(old, newdir);

                            // Delete the artist folder if empty
                            if (Settings.AuthorFolder && old.StartsWith(Settings.DownloadFolder + Author) 
                                && !Directory.EnumerateFileSystemEntries(Settings.DownloadFolder + Author).Any())
                            {
                                Directory.Delete(Settings.DownloadFolder + Author);
                            }
                            break;
                        }
                    case Settings.TunesSetting.Copy:
                        BugSenseHandler.Instance.LeaveBreadCrumb("Copying song to iTunes");
                        System.IO.File.Copy(old, newdir);
                        break;
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Find iTunes location if it exists
                var dialog = new YesNoDialog(Resources.ErrorCantFindITunes, "Locate", "Disable")
                                 {
                                     ResponseCallback = (result) =>
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
                MessageBox.Show(Resources.ErrorCantFindITunes);
            }
        }

        /// <summary>
        /// Update the ID3 tags
        /// </summary>
        public void Update()
        {
            var song = SFile.Create(Settings.DownloadFolder + (Settings.AuthorFolder ? Author : "") + "\\" +  GetFileName(Title) + ".mp3");

            if (song == null)
                return;

            if (!String.IsNullOrEmpty(Title))
                song.Tag.Title = Title;

            if (!String.IsNullOrEmpty(Author)) 
            {
                song.Tag.Performers = new[] {Author};
            }

            if (!String.IsNullOrEmpty(Genre))
                song.Tag.Genres = new[] {Genre};

            song.Tag.Pictures = new IPicture[] {new Picture(Path.GetTempPath() + "\\" + _filename + ".jpg")};

            song.Save();
        }

        /// <summary>
        /// Gather all the necessary information for downloading the actual remote resource
        /// </summary>
        /// <param name="url">The URL to the individual song</param>
        /// <param name="browser">The connection associated with the browser extension</param>
        /// <returns>A Sound representation of the remote resource</returns>
        public static Sound PrepareLink(String url, UserContext browser)
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

            var rand = RandomString(8) + ".mp3";

            // Create and return the sound representation
            return new Sound
                       {
                           _trackData = track,
                           _filename = rand,
                           Title = title,
                           Author = author,
                           Genre = track.Genre ?? "",
                           _url = url,
                           _browser = browser
                       };
        }

        /// <summary>
        /// Downloads the Sound representation
        /// </summary>
        public void Download()
        {
            var directory = Settings.DownloadFolder + GetFileName(Settings.AuthorFolder ? Author : "");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Use the download url if it exists, probably better quality
            var songDownload = (_trackData.DownloadUrl != null && Settings.UseDownloadLink) ? _trackData.DownloadUrl : _trackData.StreamUrl;
            if (songDownload == null)
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
            songDownloader.DownloadFileCompleted += FileDownloadCompleted;
            songDownloader.DownloadProgressChanged += (sender, e) =>
                                                          { if (_browser != null) _browser.Send(String.Format("{0}%", e.ProgressPercentage)); };
            songDownloader.DownloadFileAsync(new Uri(songDownload + "?client_id=" + Clientid), directory + "\\" + GetFileName(Title) + ".mp3");

            // Download the album art silently in the background
            var artDownloader = new WebClient();
            artDownloader.DownloadFileCompleted += FileDownloadCompleted;
            artDownloader.DownloadFileAsync(new Uri(_trackData.ArtworkUrl ?? _trackData.User.AvatarUrl), Path.GetTempPath() + "\\" + _filename + ".jpg");
        }

        /// <summary>
        /// Keeps track of what still needs to be downloaded and performs any 
        /// necessary post-download tasks once all files have been downloaded
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Download Information</param>
        private void FileDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
                _downloadedCount++;
            else
                throw new HandledException(e.Error.ToString(), true);

            if (_downloadedCount != TotalToDownload) return;

            if (_browser != null) _browser.Send("Finalizing");
            Update();
            AddToTunes();
            if (_browser != null) _browser.Send("Done!");
            BugSenseHandler.Instance.ClearCrashExtraData();
            BugSenseHandler.Instance.ClearBreadCrumbs();
        }

        /// <summary>
        /// Generate a random string of a given length
        /// </summary>
        /// <param name="size">Length of the string</param>
        /// <see cref="http://stackoverflow.com/questions/1122483/c-sharp-random-string-generator"/>
        /// <returns>A randomized string </returns>
        private static String RandomString(int size)
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
        public static string GetFileName(string value)
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
