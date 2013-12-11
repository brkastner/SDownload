using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace SDownload.Dialogs
{
    /// <summary>
    /// Downloads a remote resource to a local file
    /// </summary>
    public partial class DownloadProgressDialog : Form
    {
        /// <summary>
        /// The remote URL to download
        /// </summary>
        private readonly String _url;

        /// <summary>
        /// The last exception that was thrown when downloading the file
        /// null if no exception was thrown
        /// </summary>
        public Exception LastException { get; private set; }

        /// <summary>
        /// The size of the remote resource to download
        /// </summary>
        private readonly int _size;

        /// <summary>
        /// Create the download progress dialog
        /// </summary>
        /// <param name="url">The remote url to download</param>
        /// <param name="size">The size of the remote resource to download</param>
        public DownloadProgressDialog(String url, int size)
        {
            InitializeComponent();

            _size = size;
            _url = url;
            LastException = null;

            Show();
        }

        /// <summary>
        /// Download the remote resource to the given local location
        /// </summary>
        /// <param name="fileLocation">The local location to save the file to</param>
        /// <returns>Whether the file was successfully downloaded or not</returns>
        public bool Download(String fileLocation)
        {
            try
            {
                // Setup the connection
                var downloadRequest = (HttpWebRequest) WebRequest.Create(_url);
                downloadRequest.Accept = "application/octet-stream";
                downloadRequest.Method = WebRequestMethods.Http.Get;
                downloadRequest.UserAgent = "SDownload";

                // Start the connection to the github api
                var downloadResponse = downloadRequest.GetResponse();

                // Delete the local file if it already exists
                if (File.Exists(fileLocation))
                    File.Delete(fileLocation);

                // Open the file for writing
                using (var installer = File.OpenWrite(fileLocation))
                {
                    var installerBuffer = new byte[4096];
                    var responseStream = downloadResponse.GetResponseStream();
                    if (responseStream == null)
                        return false;
                    progressBar.Maximum = _size;
                    int read;
                    while ((read = responseStream.Read(installerBuffer, 0, installerBuffer.Length)) > 0)
                    {
                        // Write the info from the stream
                        installer.Write(installerBuffer, 0, read);

                        // Update the progress
                        progressBar.Step = read;
                        progressBar.PerformStep();
                    }
                }
            } 
            catch (Exception e)
            {
                // Store the last exception
                LastException = e;
                return false;
            }
            return true;
        }
    }
}
