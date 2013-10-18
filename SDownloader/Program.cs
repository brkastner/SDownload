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
            // Only start if there isn't an instance already running
            if (IsAlreadyRunning())
                return;

            // Initialize error handling
            const string uncaughtErrorMsg =
                "SDownload has encountered an unexpected bug and needs to stop what it was doing. This crash has been recorded in order to improve future versions.";

            BugSenseHandler.Instance.InitAndStartSession(ApiKey);
            Application.ThreadException += (sender, e) => HandledException.Throw(uncaughtErrorMsg, e.Exception, false);
            AppDomain.CurrentDomain.UnhandledException +=
                (sender, e) => HandledException.Throw(uncaughtErrorMsg, e.ExceptionObject as Exception, false);

            Application.Run(new Program(args));
        }

        private static Mutex _mutex;

        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenu _mainMenu;

        private readonly WebSocketServer _listener;

        private SettingsForm _settingsForm;

        private const String ApiKey = "w8c7ad34";

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
            _mainMenu.MenuItems.Add("Check for Updates", (sender, eargs) => CheckVersionAsync());
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
            if (Settings.CheckForUpdates)
                CheckVersionAsync();

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

            if (extensionFolder.Any()) return;

            // Chrome extension is not installed
            var dialog =
                new YesNoDialog("SDownload requires a browser extension for Chrome in order to function properly!",
                                "Download", "Exit")
                    {
                        ResponseCallback = result =>
                                               {
                                                   if (result)
                                                       DownloadChromeExtension(null, null);
                                                   else
                                                       Exit();
                                               }
                    };
            dialog.Show();
        }

        /// <summary>
        /// Checks if the current version is the newest version
        /// </summary>
        private static async void CheckVersionAsync()
        {
            try
            {
                // Query the remote Github API for newer releases
                var request =
                    (HttpWebRequest) WebRequest.Create("https://api.github.com/repos/brkastner/SDownload/releases");
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/vnd.github.manifold-preview";

                // Process response
                var response = request.GetResponse().GetResponseStream();
                if (response == null)
                    throw new HandledException("There was an issue checking for updates!");

                var contract =
                    new DataContractJsonSerializer(typeof (GithubReleaseItemContract[])).ReadObject(response) as
                    GithubReleaseItemContract[];
                if (contract == null)
                    throw new HandledException("Could not deserialize the version update information!", true);

                var currentVersion = new int[3];
                var i = 0;
                foreach (var num in (Application.ProductVersion).Split('.'))
                    currentVersion[i++] = Int32.Parse(num);

                // Combine any new releases to get the changelog from each
                var newerReleases = (from release in contract
                                     let versionNumbers = (release.TagName.Remove(0, 1)).Split('.')
                                     where (Int32.Parse(versionNumbers[0]) > currentVersion[0] ||
                                            Int32.Parse(versionNumbers[0]) == currentVersion[0] &&
                                            Int32.Parse(versionNumbers[1]) > currentVersion[1] ||
                                            Int32.Parse(versionNumbers[0]) == currentVersion[0] &&
                                            Int32.Parse(versionNumbers[1]) == currentVersion[1] &&
                                            Int32.Parse(versionNumbers[2]) > currentVersion[2]) && !release.Draft
                                     select release).ToList();

                if (newerReleases.Count < 1) return;

                // Current version is not up to date, download new version
                var downloadRequest = (HttpWebRequest) WebRequest.Create(newerReleases[0].Assets[0].Url);
                downloadRequest.MediaType = "application/octet-stream";
                downloadRequest.Accept = "application/vnd.github.manifold-preview";
                downloadRequest.Method = WebRequestMethods.Http.Get;
                var downloadResponse = downloadRequest.GetResponse().GetResponseStream();
                if (downloadResponse == null)
                    throw new HandledException("There was an issue checking for updates!");
                var installerBuffer = new byte[newerReleases[0].Assets[0].Size];
                var downloadTask = downloadResponse.ReadAsync(installerBuffer, 0, installerBuffer.Length);
                var fileLocation = String.Format("{0}\\sdownload_version_{1}.exe", Path.GetTempPath(),
                                                 newerReleases[0].TagName);
                using (var installer = File.OpenWrite(fileLocation))
                {
                    await downloadTask;
                    // Save the installer to the disk
                    installer.Write(installerBuffer, 0, installerBuffer.Length);
                    UpdateAvailableDialog.Prompt(fileLocation, newerReleases);
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
                            ResponseCallback = result =>
                                                   {
                                                       if (result)
                                                           Exit();
                                                   },
                            CheckBoxSettingCallback = result => Settings.ConfirmExit = !result
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
        /// <param name="e">Not used</param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_settingsForm != null)
                _settingsForm.Close();

            _listener.Stop();
            _mutex.ReleaseMutex();
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

        /// <summary>
        /// Check if SDownload is already running
        /// </summary>
        /// <returns>True if the application is already running</returns>
        private static bool IsAlreadyRunning()
        {
            var name = new FileInfo(Assembly.GetExecutingAssembly().Location).Name;

            _mutex = new Mutex(true, "Global\\" + name);

            GC.KeepAlive(_mutex);

            try
            {
                return _mutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException)
            {
                _mutex.ReleaseMutex();
                return _mutex.WaitOne(0, false);
            }
        }
    }
}
