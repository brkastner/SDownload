using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using SDownload.Framework;
using SDownload.Framework.Models;
using System.Net;

namespace SDownload.Dialogs
{
    /// <summary>
    /// Dialog shown when an update is available
    /// </summary>
    public partial class UpdateAvailableDialog : Form
    {
        /// <summary>
        /// The URL of the file to download
        /// </summary>
        private readonly String _fileUrl;

        /// <summary>
        /// Only keep one instance of the update dialog
        /// </summary>
        private static UpdateAvailableDialog _form;

        /// <summary>
        /// Initializes the dialog
        /// </summary>
        /// <param name="fileUrl">The URL of the new version to download</param>
        /// <param name="contracts">The response received from the SDownload regarding the new version</param>
        public UpdateAvailableDialog(String fileUrl, IReadOnlyList<GithubReleaseItemContract> contracts)
        {
            InitializeComponent();

            _fileUrl = fileUrl;

            // Set newest version number
            versionNumberLabel.Text = contracts[0].Name;

            // Combine all changelogs since the current version
            var sb = new StringBuilder();
            foreach (var release in contracts)
            {
                sb.AppendLine(release.Name + "\r\n---------");
                sb.AppendLine(release.Body.Replace("-F", "-").Replace("-N", "-") + "\r\n");
            }
            changeLogBox.Text = sb.ToString();

            // Set up the buttons
            noResponseButton.Click += (sender, args) => Close();
            yesResponseButton.Click += (sender, args) => DownloadAndInstall(contracts[0].Assets[0].Size);
        }

        /// <summary>
        /// Downloads and installs the newest version
        /// </summary>
        /// <param name="size">The size of the new version to download</param>
        private void DownloadAndInstall(int size)
        {
            var downloader = new DownloadProgressDialog(_fileUrl, size);
            var fileLocation = String.Format("{0}\\sdownload_update.exe", Path.GetTempPath());

            try
            {
                LogUpdate();
            }
            catch (Exception) { }

            if (downloader.Download(fileLocation))
            {
                // Launch the installer and close the running instance
                try
                {
                    Process.Start(fileLocation);
                } 
                catch (Exception)
                {
                    HandledException.Throw("There was an issue launching the update! You'll need to manually start the file: " + fileLocation, false);
                }
                Close();
                Application.Exit();
            }
            else
            {
                // There was an issue downloading the file
                HandledException.Throw("There was an issue downloading the update!", downloader.LastException);
                Close();
            }
        }

        /// <summary>
        /// Log the update using Google Analytics
        /// </summary>
        private async void LogUpdate()
        {
            const String logFormat = "v=1&tid=UA-44166717-4&cid=555&t=event&ec=Downloads&ea=Update&el={0}&ev=1";
            byte[] dataStream = Encoding.UTF8.GetBytes(String.Format(logFormat, versionNumberLabel.Text));
            WebRequest webRequest = WebRequest.Create("http://www.google-analytics.com/collect");
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = dataStream.Length;  
            Stream newStream=webRequest.GetRequestStream();
            // Send the data.
            await newStream.WriteAsync(dataStream,0,dataStream.Length);
            newStream.Close();
        }



        /// <summary>
        /// Creates an update dialog to prompt the user
        /// </summary>
        /// <param name="fileUrl">The URL of the new version to download</param>
        /// <param name="contracts">The response received from the SDownload regarding the new version</param>
        public static void Prompt(String fileUrl, List<GithubReleaseItemContract> contracts)
        {
            if (_form == null || _form.IsDisposed)
                _form = new UpdateAvailableDialog(fileUrl, contracts);

            _form.Show();
        }
    }
}
