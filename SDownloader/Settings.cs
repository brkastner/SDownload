using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SDownload
{
    public static class Settings
    {
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

        public static bool AuthorFolder
        {
            get { return Properties.Settings.Default.AuthorFolder; }
            set
            {
                Properties.Settings.Default.AuthorFolder = value;
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
