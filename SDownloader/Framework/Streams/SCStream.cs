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

namespace SDownload.Framework.Streams
{
    /// <summary>
    /// Represents a Song file downloaded from SoundCloud
    /// </summary>
    public class SCStream : BaseStream
    {
        private const String Clientid = "4515286ec9d4ace678140c3f84357b35";

        private readonly TrackData _trackData;
        private readonly String _title;
        private readonly String _author;
        private readonly String _origUrl;
        public String Genre;

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
        /// Gather and prepare all the necessary information for downloading the actual remote resource
        /// </summary>
        /// <param name="url">The URL to the individual song</param>
        /// <param name="view">The connection associated with the browser extension</param>
        /// <returns>A Sound representation of the remote resource</returns>
        public SCStream(String url, InfoReportProxy view) : base(url, view)
        {
            TrackData track = null;
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
            }
            catch (Exception e)
            {
                HandledException.Throw("There was an issue connecting to the Soundcloud API.", e);
            }

            if (track == null)
                throw new HandledException("Downloaded track information was corrupted!", true);

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
            var absoluteUrl = directory + "\\" + GetCleanFileName(title) + ".mp3";
            var artworkUrl = track.ArtworkUrl ?? track.User.AvatarUrl;

            var extras = new List<DownloadItem>
                             {new DownloadItem(new Uri(artworkUrl), Path.GetTempPath() + rand + ".jpg")};

            // Save the class variables
            // TODO: clean this up and assign values as you go
            _trackData = track;
            _title = title;
            _author = author;
            _origUrl = url;
            Genre = track.Genre ?? "";
            MainResource = new DownloadItem(new Uri(GetDownloadUrl()), absoluteUrl);
            Extras = extras;
            View = view;
        }

        private String GetDownloadUrl(bool forceStream = false, bool forceManual = false)
        {
            var songDownload = (_trackData.DownloadUrl != null && Settings.UseDownloadLink && !forceStream) ? _trackData.DownloadUrl : _trackData.StreamUrl;
            if (songDownload == null || forceManual)
            {
                // There was no stream URL or download URL for the song, manually parse the resource stream link from the original URL
                BugSenseHandler.Instance.LeaveBreadCrumb("Manually downloading sound");
                HttpWebResponse response;
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(_origUrl);
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

        public override async Task<bool> Download(bool ignoreExtras = false)
        {
            try
            {
                return await base.Download(ignoreExtras);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Not Found"))
                {
                    View.Report("Download impossible!", true);
                }
                else
                    HandledException.Throw("There was an issue downloading the necessary file(s)!", e);
                return false;
            }
        }
        
        public override bool Validate()
        {
            if (!base.Validate())
                return false;

            // TODO: Possibly find a better way to validate more file types quicker
            // perhaps by reading the resolved url ending rather than assuming mp3 immediately
            SFile file = null;
            try
            {
                // Test if the file is a valid mp3
                file = SFile.Create(MainResource.AbsolutePath);
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
                    // File could not be validated, Use the stream download
                    View.Report("Retrying");
                    MainResource.Uri = new Uri(GetDownloadUrl(true));
                    Download(true);
                }
            }

            return file != null;
        }

        public override void Finish()
        {
            View.Report("Finalizing");
            try
            {
                UpdateId3Tags();
                AddToiTunes();
            } catch (Exception e)
            {
                // Should have been handled already
                View.Report("Invalid file!", true);
                HandledException.Throw("Invalid file was downloaded!", e);
                return;
            }

            base.Finish();
        }
    }
}
