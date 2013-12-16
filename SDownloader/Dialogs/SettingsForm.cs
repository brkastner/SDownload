using System.Windows.Forms;
using SDownload.Framework;

namespace SDownload.Dialogs
{
    /// <summary>
    /// A GUI for changing the application settings
    /// </summary>
    public partial class SettingsForm : Form
    {
        /// <summary>
        /// Initializes the form with the current application settings
        /// </summary>
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

            // Use Download Link
            useDownloadLink.Checked = Settings.UseDownloadLink;

            // Confirm Exit
            confirmExitCheckBox.Checked = Settings.ConfirmExit;

            // Check for Updates
            checkForUpdates.Checked = Settings.CheckForUpdates;

            // Beta Updates
            betaCheckBox.Checked = Settings.EnableBetaUpdates;

            // Save Settings
            saveBtn.Click += (sender, args) =>
                                 {
                                     Settings.DownloadFolder = downloadFolderBox.Text;

                                     if (!iTunesEnabled.Checked)
                                         Settings.TunesTransfer = Settings.TunesSetting.Nothing;
                                     else
                                     {
                                         Settings.TunesTransfer = iTunesCopy.Checked
                                                                      ? Settings.TunesSetting.Copy
                                                                      : Settings.TunesSetting.Move;
                                     }

                                     Settings.UseDownloadLink = useDownloadLink.Checked;
                                     Settings.ConfirmExit = confirmExitCheckBox.Checked;
                                     Settings.CheckForUpdates = checkForUpdates.Checked;
                                     Settings.EnableBetaUpdates = betaCheckBox.Checked;

                                     Close();
                                 };
        }
    }
}
