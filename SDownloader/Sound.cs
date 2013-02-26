using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TagLib;
using SFile = TagLib.File;
using HtmlAgilityPack;

namespace SDownload
{
    /// <summary>
    /// Represents a Song file downloaded from SoundCloud
    /// </summary>
    public class Sound
    {
        public String Title;
        public String Author;
        public String Genre;

        private readonly String _filename;

        public static NotifyHandler Notify = null;

        public Sound(String filename, String title = "", String author = "", String genre = "")
        {
            Title = title;
            Author = author;
            Genre = genre;

            _filename = filename;
        }

        /// <summary>
        /// Add the song to iTunes
        /// </summary>
        public void AddToMusic()
        {
            var old = String.Format("{0}\\{1}", Directory.GetCurrentDirectory(), _filename);
            var newdir = String.Format("{0}\\iTunes\\iTunes Media\\Automatically Add to iTunes\\{1}.mp3", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), Title);
            try
            {
                System.IO.File.Move(old, newdir);
            }
            catch (DirectoryNotFoundException e)
            {
                // iTunes directory does not exist, move to music folder
                newdir = String.Format("{0}\\{1}\\}", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                                       Author);
                Directory.CreateDirectory(newdir);
                newdir += Title;
                System.IO.File.Move(old, newdir);
            }
            catch
            {
                
            }
        }

        /// <summary>
        /// Update the ID3 tags
        /// </summary>
        public void Update()
        {
            var song = SFile.Create(_filename);

            if (song == null)
                return;

            if (!String.IsNullOrEmpty(Title))
                song.Tag.Title = Title;

            if (!String.IsNullOrEmpty(Author)) 
            {
                song.Tag.AlbumArtists = new[] {Author};
                song.Tag.Performers = new[] {Author};
            }

            if (!String.IsNullOrEmpty(Genre))
                song.Tag.Genres = new[] {Genre};

            song.Tag.Pictures = new IPicture[] {new Picture(_filename + ".jpg")};

            song.Save();
        }

        /// <summary>
        /// Download a SoundCloud song
        /// </summary>
        /// <param name="url">The url of the song</param>
        /// <returns>A Sound representation of the song</returns>
        public static Sound Download(String url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            var doc = new HtmlDocument();
            doc.Load(response.GetResponseStream());

            var searchString = WebUtility.HtmlDecode(doc.DocumentNode.InnerHtml);
            var links = Regex.Matches(searchString, "((http:[/][/])(media.soundcloud.com/stream/)([a-z]|[A-Z]|[0-9]|[/.]|[~]|[?]|[_]|[=])*)");

            var author = doc.DocumentNode.SelectSingleNode(@"//a[@class='user-name']").InnerText;
            var title = doc.DocumentNode.SelectSingleNode(@"//meta[@property='og:title']")
                           .GetAttributeValue("content", "");
            var album = doc.DocumentNode.SelectSingleNode(@"//meta[@property='og:image']")
                           .GetAttributeValue("content", "");
            var genre = doc.DocumentNode.SelectSingleNode(@"//span[@class='genre']").InnerText;

            var tokens = WebUtility.HtmlDecode(title).Split('-');
            if (tokens.Length > 1)
            {
                author = tokens[0].Trim();
                title = tokens[1].Trim();
            }

            var wc = new WebClient();

            var rand = RandomString(8) + ".mp3";
            Notify.Show(String.Format("Downloading {0} by {1}", title, author));
            wc.DownloadFile(new Uri(links[0].Value), Directory.GetCurrentDirectory() + "\\" + rand);
            wc.DownloadFile(new Uri(album), Directory.GetCurrentDirectory() + "\\" + rand + ".jpg");

            return new Sound(rand, title, author, genre);
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
    }
}
