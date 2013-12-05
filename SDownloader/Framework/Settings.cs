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
        /// Whether or not this is the first time this application has been run
        /// </summary>
        public static bool FirstRun
        {
            get { return Storage.Default.FirstRun; }
            set 
            { 
                Storage.Default.FirstRun = value;
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
                var folder = Properties.Settings.Default.DownloadFolder;
                folder = folder.Replace("[MUSICFOLDER]", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                if (!folder.EndsWith("\\"))
                    folder += "\\";
                return folder;
            }
            set
            {
                Properties.Settings.Default.DownloadFolder = value;
                Properties.Settings.Default.Save();
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
            get { return Properties.Settings.Default.ConfirmExit; }
            set 
            { 
                Properties.Settings.Default.ConfirmExit = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Whether or not the application should automatically check for updates
        /// </summary>
        public static bool CheckForUpdates
        {
            get { return Properties.Settings.Default.CheckForUpdates; }
            set 
            { 
                Properties.Settings.Default.CheckForUpdates = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Whether or not downloaded resource files should be organized in folders by the Author name
        /// TODO: Separate stream-specific settings from application settings
        /// </summary>
        public static bool AuthorFolder
        {
            get { return Properties.Settings.Default.AuthorFolder; }
            set
            {
                Properties.Settings.Default.AuthorFolder = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Whether or not the download link should be used over the link provided by the API
        /// </summary>
        public static bool UseDownloadLink
        {
            get { return Properties.Settings.Default.UseDownloadLink;  }
            set 
            { 
                Properties.Settings.Default.UseDownloadLink = value;
                Properties.Settings.Default.Save();
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
                return (TunesSetting)Properties.Settings.Default.iTunes;
            }
            set
            {
                Properties.Settings.Default.iTunes = (int)value;
                Properties.Settings.Default.Save();
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
