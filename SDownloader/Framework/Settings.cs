using System;
using Storage = SDownload.Properties.Settings;


namespace SDownload.Framework
{
    public static class Settings
    {
        public static bool FirstRun
        {
            get { return Storage.Default.FirstRun; }
            set 
            { 
                Storage.Default.FirstRun = value;
                Storage.Default.Save();
            }
        }

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

        public static bool ConfirmExit
        {
            get { return Properties.Settings.Default.ConfirmExit; }
            set 
            { 
                Properties.Settings.Default.ConfirmExit = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool CheckForUpdates
        {
            get { return Properties.Settings.Default.CheckForUpdates; }
            set 
            { 
                Properties.Settings.Default.CheckForUpdates = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool AuthorFolder
        {
            get { return Properties.Settings.Default.AuthorFolder; }
            set
            {
                Properties.Settings.Default.AuthorFolder = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool UseDownloadLink
        {
            get { return Properties.Settings.Default.UseDownloadLink;  }
            set 
            { 
                Properties.Settings.Default.UseDownloadLink = value;
                Properties.Settings.Default.Save();
            }
        }

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

        public enum TunesSetting
        {
            Nothing = 0,
            Copy = 1,
            Move = 2
        }
    }
}
