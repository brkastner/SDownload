using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
        /// <param name="contracts">The response received from the SDownload regarding the new version</param>
        public UpdateAvailableDialog(String fileUrl, List<GithubReleaseItemContract> contracts)
        {
            InitializeComponent();

            // Set newest version number
            versionNumberLabel.Text = contracts[0].Name;

            // Combine all changelogs since the current version
            var sb = new StringBuilder();
            foreach (var release in contracts)
            {
                sb.AppendLine(release.Name + "\r\n---------");
                sb.AppendLine(release.Body + "\r\n");
            }
            changeLogBox.Text = sb.ToString();

            // Set up the buttons
            noResponseButton.Click += (sender, args) => Close();
            yesResponseButton.Click += (sender, args) =>
                                           {
                                               Process.Start(fileUrl);
                                               Close();
                                               Application.Exit();
                                           };
        }

        /// <summary>
        /// Creates an update dialog to prompt the user
        /// </summary>
        /// <param name="fileUrl">The URL of the new version to download</param>
        /// <param name="contracts">The response received from the SDownload regarding the new version</param>
        public static void Prompt(String fileUrl, List<GithubReleaseItemContract> contracts)
        {
            new UpdateAvailableDialog(fileUrl, contracts).Show();

        }
    }
}
