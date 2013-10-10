using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using SDownload.Framework;

namespace SDownload.Dialogs
{
    /// <summary>
    /// Dialog shown when an update is available
    /// </summary>
    public partial class UpdateAvailableDialog : Form
    {
        /// <summary>
        /// Initializes the dialog
        /// </summary>
        /// <param name="fileUrl">The URL of the new version to download</param>
        /// <param name="contract">The response received from the SDownload regarding the new version</param>
        public UpdateAvailableDialog(String fileUrl, VersionResponseContract contract)
        {
            InitializeComponent();

            // Load new version information from the contract
            versionNumberLabel.Text = contract.NewestVersion;
            changeLogBox.Text = contract.Changelog;

            // Set up the buttons
            noResponseButton.Click += (sender, args) => Close();
            yesResponseButton.Click += (sender, args) =>
                                           {
                                               Process.Start(fileUrl);
                                               Close();
                                           };

            // Load the sidebar ad
            new Thread(() => adWebBrowser.Navigate(fileUrl)).Start();
        }
    }
}
