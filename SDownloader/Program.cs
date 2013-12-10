using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Alchemy;
using System;
using System.Windows.Forms;
using BugSense;
using BugSense.Model;
using SDownload.Dialogs;
using SDownload.Framework;
using SDownload.Framework.Streams;
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
        /// Mutex for ensuring only one instance is running at a time
        /// </summary>
        private static Mutex _mutex;

        /// <summary>
        /// The icon for SDownload in the task bar
        /// </summary>
        private readonly NotifyIcon _trayIcon;

        /// <summary>
        /// Main context menu shown when the taskbar icon is right-clicked
        /// </summary>
        private readonly ContextMenu _mainMenu;

        /// <summary>
        /// Listener for song download messages
        /// </summary>
        private WebSocketServer _listener;

        /// <summary>
        /// UI for configuring SDownload
        /// </summary>
        private SettingsForm _settingsForm;

        /// <summary>
        /// BugSense API key
        /// </summary>
        private const String BugSenseApiKey = "w8c7ad34";

        /// <summary>
        /// URL for downloading the helper extension for Chrome
        /// </summary>
        private const String ChromeDownloadUrl =
            "https://chrome.google.com/webstore/detail/sdownload/dkflmdcolphnomonabinogaegbjbnbbm";

        private delegate void CheckVersionDelegate();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(String[] args)
        {
            // Initialize error handling
            const string uncaughtErrorMsg =
                "SDownload has encountered an unexpected bug and needs to stop what it was doing. This crash has been recorded in order to improve future versions.";
            var exceptionManager = new ExceptionManager();
            exceptionManager.ThreadException += (sender, e) => HandledException.Throw(uncaughtErrorMsg, e.Exception, false);
            exceptionManager.UnhandledException +=
                (sender, e) => HandledException.Throw(uncaughtErrorMsg, e.ExceptionObject as Exception, false);
            BugSenseHandler.Instance.InitAndStartSession(exceptionManager, BugSenseApiKey);

            Application.Run(new Program(args));
        }

        /// <summary>
        /// Parse the given link and send it to the appropriate stream downloader
        /// </summary>
        /// <param name="url">The link provided to the application from the view</param>
        /// <param name="view">The view to report progress back to</param>
        private static async void DownloadUrl(String url, InfoReportProxy view)
        {
            var sound = new SCStream(url, view);
            var download = sound.Download();

            // Log the song genre to see how SDownload is used
            if (sound.Genre != null && !sound.Genre.Equals(String.Empty))
                BugSenseHandler.Instance.SendEvent(sound.Genre);

            if (download != null && await download)
                sound.Finish();
        }

        /// <summary>
        /// Set up the helper service and check for extension installation and updates
        /// </summary>
        /// <param name="args"></param>
        public Program(String[] args)
        {
            // Only start if there isn't an instance already running
            if (IsAlreadyRunning() && !Settings.FirstRun)
            {
                var dialog =
                    new YesNoDialog(
                        "Only one instance of SDownload can be running at a time!",
                        "Close", null)
                        {
                            ResponseCallback = result => Exit()
                        };
                dialog.Show();
            }
            else
            {
                if (Settings.FirstRun)
                    Settings.FirstRun = false;

                if (args.Length > 0)
                {
                    var link = args[0].Contains("sdownload://") ? args[0].Substring(12) : args[0];
                    if (!link.StartsWith("launch"))
                        DownloadUrl(link, new InfoReportProxy());
                }

                const String donateUrl =
                    "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=HCGUGKSBR7XMS";

                _mainMenu = new ContextMenu();
                _mainMenu.MenuItems.Add("Donate", (sender, eargs) => Process.Start("explorer.exe", donateUrl));
                _mainMenu.MenuItems.Add("Check for Updates", (sender, eargs) => CheckVersion());
                _mainMenu.MenuItems.Add("Download Chrome Extension", DownloadChromeExtension);
                _mainMenu.MenuItems.Add("Settings", ShowSettings);
                _mainMenu.MenuItems.Add("Exit", ConfirmExitApplication);

                _trayIcon = new NotifyIcon
                                {
                                    Text = Resources.ApplicationName,
                                    Icon = Resources.ApplicationIcon,
                                    ContextMenu = _mainMenu,
                                    Visible = true
                                };

                _settingsForm = new SettingsForm
                                    {
                                        Visible = false
                                    }; 

                SetupListener();

                // Asynchronously check for updates
                if (Settings.CheckForUpdates)
                    CheckVersion();

                // Check if Chrome extension installed
                ValidateChromeInstallation();
            }
        }

        private void SetupListener()
        {
            _listener = new WebSocketServer(7030, IPAddress.Parse("127.0.0.1"));
            _listener.OnReceive += context =>
            {
                var data = context.DataFrame.ToString();
                DownloadUrl(data, new WSReportProxy(context));

                // Check for updates after the song has already started downloading
                if (Settings.CheckForUpdates)
                    CheckVersion();
            };
            _listener.Start();
        }

        /// <summary>
        /// Check for a Google Chrome installation and make sure the helper
        /// extension is installed
        /// </summary>
        private void ValidateChromeInstallation()
        {
            var extensionPath = Path.GetTempPath() + "..\\Google\\Chrome\\User Data\\Default\\Extensions\\";

            // Ensure the user has chrome installed
            if (!Directory.Exists(extensionPath))
            {
                var errordialog = new YesNoDialog("SDownload requires Google Chrome to be installed!", "Install", "Exit")
                                 {
                                     ResponseCallback = result =>
                                                            {
                                                                if (result)
                                                                    Process.Start("explorer.exe", "http://www.google.com/chrome");
                                                                Exit();
                                                            }
                                 };
                errordialog.Show();
                return;
            }

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
        private void CheckVersion()
        {
            try
            {
                // Query the remote Github API for newer releases
                var request =
                    (HttpWebRequest) WebRequest.Create("https://api.github.com/repos/brkastner/SDownload/releases");
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/vnd.github.v3+json";
                request.UserAgent = "SDownload";

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
                                     where ((Int32.Parse(versionNumbers[0]) > currentVersion[0]) || // Major
                                            (Int32.Parse(versionNumbers[0]) == currentVersion[0] && // Minor
                                            Int32.Parse(versionNumbers[1]) > currentVersion[1]) ||
                                            (Int32.Parse(versionNumbers[0]) == currentVersion[0] && // Incremental
                                            Int32.Parse(versionNumbers[1]) == currentVersion[1] &&
                                            Int32.Parse(versionNumbers[2]) > currentVersion[2])) && !release.Draft
                                     select release).ToList();

                if (newerReleases.Count < 1) return;

                // Current version is not up to date, prompt the user to download the new version
                UpdateAvailableDialog.Prompt(newerReleases[0].Assets[0].Url, newerReleases);
            }
            catch (WebException e)
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
			if (_settingsForm == null || _settingsForm.IsDisposed)
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
            if (_listener != null)
            {
                _listener.Stop();
            }

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
            base.OnClosing(e);
        }

        /// <summary>
        /// Disposes of the tray icon before disposing of itself
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_listener != null)
                    _listener.Dispose();

                if (_trayIcon != null)
                    _trayIcon.Dispose();

                try
                {
                    if (_mutex != null)
                        _mutex.ReleaseMutex();
                } catch (ApplicationException)
                {
                    // Do nothing since the exception is due to attempting to release
                    // a mutex that we do not own
                }
            }
            
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

            try
            {
                return !_mutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException)
            {
                _mutex.ReleaseMutex();
                return !_mutex.WaitOne(0, false);
            }
        }
    }
}
