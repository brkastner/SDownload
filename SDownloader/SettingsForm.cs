using System.Windows.Forms;

namespace SDownload
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            // DownloadFolder
            downloadFolderBox.Text = Settings.DownloadFolder;
            selectDownloadFolderBtn.Click += (sender, args) =>
                                                 {
                                                     if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                                                         downloadFolderBox.Text = folderBrowserDialog1.SelectedPath;
                                                 };
            
            // Author Folder
            authorFolderSort.Checked = Settings.AuthorFolder;
            authorFolderSort.CheckedChanged += (sender, args) =>
                                                   {
                                                       Settings.AuthorFolder = authorFolderSort.Checked;
                                                   };

            // iTunes Functionality
            iTunesEnabled.Checked = Settings.TunesTransfer != Settings.TunesSetting.Nothing;
            iTunesEnabled.CheckedChanged += (sender, args) =>
                                                {
                                                    iTunesCopy.Enabled = iTunesEnabled.Checked;
                                                };
            iTunesCopy.Enabled = iTunesEnabled.Checked;
            iTunesCopy.Checked = Settings.TunesTransfer == Settings.TunesSetting.Copy;

            // Save Settings
            saveBtn.Click += (sender, args) =>
                                 {
                                     // TODO: Validate?
                                     Settings.DownloadFolder = downloadFolderBox.Text;

                                     if (!iTunesEnabled.Checked)
                                         Settings.TunesTransfer = Settings.TunesSetting.Nothing;
                                     else
                                     {
                                         Settings.TunesTransfer = iTunesCopy.Checked
                                                                      ? Settings.TunesSetting.Copy
                                                                      : Settings.TunesSetting.Move;
                                     }
                                 };
        }
    }
}
