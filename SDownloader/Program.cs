using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Threading;
using Alchemy;
using System;
using System.Windows.Forms;
using BugSense_WF;
using SDownload.Dialogs;
using SDownload.Framework;
using Resources = SDownload.Properties.Resources;

/* TODO: 
 * Ads
 **/

namespace SDownload
{
    /// <summary>
    /// Helper application that runs in the system tray and interacts with
    /// with a browser extension.
    /// </summary>
    public class Program : Form
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(String[] args)
        {
            // Initialize error handling
            const string uncaughtErrorMsg = 
                "ERROR:\n\nSDownload has encountered an unexpected bug and needs to stop what it was doing. This crash has been recorded in order to improve future versions.";

            BugSenseHandler.Instance.InitAndStartSession(ApiKey);
            Application.ThreadException += (sender, e) => HandledException.Throw(uncaughtErrorMsg, e.Exception, false); ;
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>HandledException.Throw(uncaughtErrorMsg, e.ExceptionObject as Exception, false); ;

            Application.Run(new Program(args));
        }

        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenu _mainMenu;

        private readonly WebSocketServer _listener;

        private SettingsForm _settingsForm;

        private const String ApiKey = "w8c7ad34";

        private const String SDownloadApi = "http://www.sdownload.com/api.php?action={0}";

        private const String ChromeDownloadUrl =
            "https://chrome.google.com/webstore/detail/sdownload/dkflmdcolphnomonabinogaegbjbnbbm";

        /// <summary>
        /// Set up the helper service and check for extension installation and updates
        /// </summary>
        /// <param name="args"></param>
        public Program(String[] args)
        {
            if (args.Length > 0)
            {
                var link = args[0].Contains("sdownload://") ? args[0].Substring(12) : args[0];
                if (!link.StartsWith("launch"))
                {
                    var sound = Sound.PrepareLink(link, null);
                    sound.Download();
                } 
            }

            _mainMenu = new ContextMenu();
            _mainMenu.MenuItems.Add("Settings", ShowSettings);
            _mainMenu.MenuItems.Add("Download Chrome Extension", DownloadChromeExtension);
            _mainMenu.MenuItems.Add("Close", ConfirmExitApplication);

            _trayIcon = new NotifyIcon
                           {
                               Text = Resources.ApplicationName, 
                               Icon = Resources.sdownload, 
                               ContextMenu = _mainMenu,
                               Visible = true
                           };

            _settingsForm = new SettingsForm
                               {
                                   Visible = false
                               };

            _listener = new WebSocketServer(7030, IPAddress.Parse("127.0.0.1"));
            _listener.OnReceive += context =>
                                      {
                                          var data = context.DataFrame.ToString();
                                          var sound = Sound.PrepareLink(data, context);
                                          sound.Download();
                                      };
            _listener.Start();

            // Asynchronously check for updates
            //if (Settings.CheckForUpdates)
                //new Thread(CheckVersionAsync).Start();

            // Check if Chrome extension installed
            var extensionPath = Path.GetTempPath() + "..\\Google\\Chrome\\User Data\\Default\\Extensions\\";
            var extensionFolder = from extension in Directory.EnumerateDirectories(extensionPath)
                                  where
                                      (from version in Directory.EnumerateDirectories(extension)
                                       where (from file in Directory.EnumerateFiles(version)
                                              where file.EndsWith("sdownload.txt")
                                              select file).Any()
                                       select version).Any()
                                  select extension;

            if (!extensionFolder.Any())
            {
                // Chrome extension is not installed
                var dialog =
                    new YesNoDialog("SDownload requires a browser extension for Chrome in order to function properly!",
                                    "Download", "Exit")
                        {
                            ResponseCallback = (result) =>
                                                   {
                                                       if (result)
                                                           DownloadChromeExtension(null, null);
                                                       else
                                                           Exit();
                                                   }
                        };
                dialog.Show();
            }

        }

        /// <summary>
        /// Checks if the current version is the newest version
        /// </summary>
        private static async void CheckVersionAsync()
        {
            try
            {
                // Get the version information
                var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

                // Query the remote API
                var request =
                    (HttpWebRequest)
                    WebRequest.Create(String.Format(SDownloadApi + "&current={1}", "version_check", fvi.ProductVersion));
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/json";

                // Process response
                var response = request.GetResponse().GetResponseStream();
                if (response == null)
                    throw new HandledException("There was an issue checking for updates!");
                var contract = new DataContractJsonSerializer(typeof (VersionResponseContract)).ReadObject(response) as
                                                   VersionResponseContract;
                if (contract == null)
                    throw new HandledException("Could not deserialize the version update information!", true);

                if (contract.UpToDate) return;

                // Current version is not up to date, download new version
                var downloadRequest = (FileWebRequest)WebRequest.Create(contract.UpdateUrl);
                downloadRequest.Method = WebRequestMethods.File.DownloadFile;
                var downloadResponse = downloadRequest.GetResponse().GetResponseStream();
                if (downloadResponse == null)
                    throw new HandledException("There was an issue checking for updates!");
                var installerBuffer = new byte[downloadResponse.Length];
                var downloadTask = downloadResponse.ReadAsync(installerBuffer, 0, installerBuffer.Length);
                var fileLocation = String.Format("{0}\\sdownload_version_{1}.exe", Path.GetTempPath(),
                                                 contract.NewestVersion);
                using (var installer = File.OpenWrite(fileLocation))
                {
                    await downloadTask;
                    // Save the installer to the disk
                    installer.Write(installerBuffer, 0, installerBuffer.Length);
                    var updateDialog = new UpdateAvailableDialog(fileLocation, contract);
                }
            }
            catch (Exception e)
            {
                HandledException.Throw("Unable to make a connection to the SDownload API to check for updates!", e);
            }
        }

        /// <summary>
        /// Opens the Chrome browser to the page to download the chrome extension
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="eventArgs">Not used</param>
        private void DownloadChromeExtension(object sender, EventArgs eventArgs)
        {
            // Open chrome and redirect to the extension download page
            Process.Start("chrome", ChromeDownloadUrl);
        }

        /// <summary>
        /// Show the settings form, creating one of it doesn't already exist
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="eventArgs">Not used</param>
        private void ShowSettings(object sender, EventArgs eventArgs)
        {
            // Only create a form if one doesn't already exist
            if (_settingsForm == null)
            {
                _settingsForm = new SettingsForm();
                _settingsForm.Closing += (o, args) => { _settingsForm = null; };
            }
            _settingsForm.Show();
        }

        /// <summary>
        /// Checks if confirmation is needed before exiting, then exits.
        /// (Unless cancelled by the user)
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="eventArgs">Not used</param>
        private void ConfirmExitApplication(object sender, EventArgs eventArgs)
        {
            if (Settings.ConfirmExit)
            {
                var dialog =
                    new YesNoDialog(
                        "Are you sure you want to exit? SDownload requires this application to be running in order to download any songs!",
                        "Close", "Cancel", CheckBoxState.NotChecked)
                        {
                            ResponseCallback = (result) =>
                                                   {
                                                       if (result)
                                                           Exit();
                                                   },
                            CheckBoxSettingCallback = (result) => Settings.ConfirmExit = !result
                        };
                dialog.Show();
            }
            else
                Exit();
        }

        /// <summary>
        /// Fully exits the application
        /// </summary>
        private void Exit()
        {
            _listener.Stop();
            Application.Exit();
        }

        /// <summary>
        /// This form should not be visible to the user, contains no data
        /// </summary>
        /// <param name="e">Not used</param>
        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        /// <summary>
        /// Closes any open forms before closing itself
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_settingsForm != null)
                _settingsForm.Close();

            _listener.Stop();
            base.OnClosing(e);
        }

        /// <summary>
        /// Disposes of the tray icon before disposing of itself
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _trayIcon.Dispose();

            base.Dispose(disposing);
        }
    }
}
