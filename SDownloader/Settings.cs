using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Storage = SDownload.Properties.Settings;


namespace SDownload
{
    public static class Settings
    {
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

        public static bool AuthorFolder
        {
            get { return Storage.Default.AuthorFolder; }
            set
            {
                Storage.Default.AuthorFolder = value;
                Storage.Default.Save();
            }
        }

        public static bool UseDownloadLink
        {
            get { return Storage.Default.UseDownloadLink;  }
            set 
            { 
                Storage.Default.UseDownloadLink = value;
                Storage.Default.Save();
            }
        }

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

        public enum TunesSetting
        {
            Nothing = 0,
            Copy = 1,
            Move = 2
        }
    }
}
