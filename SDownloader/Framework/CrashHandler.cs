using System;
using System.Globalization;
using System.Threading;
using BugSense;
using BugSense.Core.Model;
using SDownload.Dialogs;

namespace SDownload.Framework
{
    /// <summary>
    /// Handles error reporting
    /// </summary>
    public class CrashHandler
    {
        /// <summary>
        /// Sets the User ID that accompanies any crashes
        /// </summary>
        public static void SetUserIdentifier()
        {
            var enabled = Settings.IncludeSupportEmail && !String.IsNullOrWhiteSpace(Settings.SupportEmail);
            BugSenseHandler.Instance.UserIdentifier = enabled ? Settings.SupportEmail : "";
        }

        /// <summary>
        /// Reports an error to the user, and provides the option to report if the crash is critical enough
        /// </summary>
        /// <param name="message">Message to show to the user</param>
        /// <param name="inner">The exception that was thrown/raised</param>
        /// <param name="canReport">If the crash should be allowed to be reported to Bugsense</param>
        public static void Throw(String message, Exception inner, bool canReport = true)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            var handled = inner as HandledException;
            var criticalHandled = handled != null && handled.IsCritical;
            // Use the exception message if it is of type HandledException or a message wasn't passed through
            // If it is a HandledException, only provide the option to report if the exception is critical
            var dialog = new YesNoDialog("ERROR:\n" + (handled != null ? inner.Message : (message ?? inner.Message)),
                (handled == null && canReport) || criticalHandled ? "Report" : "Close",
                (handled == null && canReport) || criticalHandled ? "Close" : null)
            {
                ResponseCallback = result =>
                {
                    // Log exception only if 'Report' was chosen, and the exception was critical
                    if (result && canReport)
                        BugSenseHandler.Instance.SendExceptionAsync(inner);
                    ClearExtras();
                }
            };
            dialog.Show();
        }

        /// <summary>
        /// Reports an error to the user, and provides the option to report if the crash is critical enough
        /// </summary>
        /// <param name="message">The error message to show to the user</param>
        /// <param name="canReport">If the crash should be allowed to be reported to Bugsense</param>
        public static void Throw(String message, bool canReport = true)
        {
            Throw(message, new HandledException(message, canReport), canReport);
        }

        /// <summary>
        /// Adds debug information to be sent with any crashes that occur in the future
        /// </summary>
        /// <param name="key">Identifier for the debug information</param>
        /// <param name="value">The debug information to send</param>
        public static void AddExtra(String key, String value)
        {
            BugSenseHandler.Instance.AddCrashExtraData(new CrashExtraData { Key = key, Value = value });
        }

        /// <summary>
        /// Wipes all debug information that was to be sent if a crash occured
        /// </summary>
        public static void ClearExtras()
        {
            BugSenseHandler.Instance.ClearCrashExtraData();
            BugSenseHandler.Instance.ClearBreadCrumbs();
        }
    }
}
