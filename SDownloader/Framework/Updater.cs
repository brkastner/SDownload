using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SDownload.Dialogs;
using SDownload.Framework.Models;

namespace SDownload.Framework
{
    /// <summary>
    /// Check if the current version is the newest
    /// </summary>
    public class Updater
    {
        public static void CheckVersion(bool force = false)
        {
            // Don't check the version if the setting is disabled and we aren't being forced
            if (!force || !Settings.CheckForUpdates)
                return;

            try
            {
                // Query the remote Github API for newer releases
                var request =
                    (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/brkastner/SDownload/releases");
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/vnd.github.v3+json";
                request.UserAgent = "SDownload";

                // Process response
                var response = request.GetResponse().GetResponseStream();
                if (response == null)
                    throw new HandledException("There was an issue checking for updates!");

                var contract =
                    new DataContractJsonSerializer(typeof(GithubReleaseItemContract[])).ReadObject(response) as
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
                                            Int32.Parse(versionNumbers[2]) > currentVersion[2]))
                                            && !release.Draft                                       // Ignore drafts
                                     select release).ToList();

                // Remove beta updates if the option is disabled
                if (!Settings.EnableBetaUpdates)
                    newerReleases = (from release in newerReleases where !release.PreRelease select release).ToList();

                if (newerReleases.Count < 1) return;

                // Current version is not up to date, prompt the user to download the new version
                UpdateAvailableDialog.Prompt(newerReleases[0].Assets[0].Url, newerReleases);
            }
            catch (WebException e)
            {
                CrashHandler.Throw("Unable to make a connection to the SDownload API to check for updates!", e);
            }
        }
    }
}
