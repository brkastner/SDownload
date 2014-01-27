using System;
using Storage = SDownload.Properties.Settings;

namespace SDownload.Framework
{
    /// <summary>
    /// Contains all application settings
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// If beta updates should be considered when updating
        /// </summary>
        public static bool EnableBetaUpdates
        {
            get { return Storage.Default.EnableBetaUpdates; }
            set 
            { 
                Storage.Default.EnableBetaUpdates = value;
                Storage.Default.Save();
            }
        }

        /// <summary>
        /// The location main resource files should be downloaded to
        /// </summary>
        public static String DownloadFolder
        {
            get 
            { 
                var folder = Storage.Default.DownloadFolder;
                folder = folder.Replace("[MUSICFOLDER]", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                if (!folder.EndsWith("\\"))
                    folder += "\\";
                return folder;
            }
            set
            {
                Storage.Default.DownloadFolder = value;
                Storage.Default.Save();
            }
        }

        /// <summary>
        /// Returns the location of the iTunes installation on the computer if it is not in the default spot
        /// </summary>
        public static String CustomITunesLocation
        {
            get
            {
                var location = Storage.Default.CustomITunesLocation;
                return location == "" ? null : location;
            }
            set 
            { 
                Storage.Default.CustomITunesLocation = value;
                Storage.Default.Save();
            }
        }

        /// <summary>
        /// Whether or not the application should confirm with the user before exiting
        /// </summary>
        public static bool ConfirmExit
        {
            get { return Storage.Default.ConfirmExit; }
            set 
            { 
                Storage.Default.ConfirmExit = value;
                Storage.Default.Save();
            }
        }

        /// <summary>
        /// Whether or not the application should automatically check for updates
        /// </summary>
        public static bool CheckForUpdates
        {
            get { return Storage.Default.CheckForUpdates; }
            set 
            { 
                Storage.Default.CheckForUpdates = value;
                Storage.Default.Save();
            }
        }

        /// <summary>
        /// Whether or not downloaded resource files should be organized in folders by the Author name
        /// TODO: Separate stream-specific settings from application settings
        /// </summary>
        public static bool AuthorFolder
        {
            get { return Storage.Default.AuthorFolder; }
            set
            {
                Storage.Default.AuthorFolder = value;
                Storage.Default.Save();
            }
        }

        /// <summary>
        /// Whether or not the download link should be used over the link provided by the API
        /// </summary>
        public static bool UseDownloadLink
        {
            get { return Storage.Default.UseDownloadLink;  }
            set 
            { 
                Storage.Default.UseDownloadLink = value;
                Storage.Default.Save();
            }
        }

        /// <summary>
        /// iTunes functionality setting
        /// Nothing to disable
        /// Copy/Move to enable (move deletes from download folder)
        /// </summary>
        public static TunesSetting TunesTransfer
        {
            get
            {
                return (TunesSetting)Storage.Default.iTunes;
            }
            set
            {
                Storage.Default.iTunes = (int)value;
                Storage.Default.Save();
            }
        }

        /// <summary>
        /// Possible iTunes functionality settings
        /// </summary>
        public enum TunesSetting
        {
            /// <summary>
            /// Disable iTunes functionality
            /// </summary>
            Nothing = 0,

            /// <summary>
            /// Add the resource to iTunes, leaving a copy in the download folder
            /// </summary>
            Copy = 1,

            /// <summary>
            /// Add the resource to iTunes and remove it from the download folder
            /// </summary>
            Move = 2
        }
    }
}
