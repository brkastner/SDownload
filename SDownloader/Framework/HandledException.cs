using System;
using System.Globalization;
using System.Threading;
using BugSense;
using SDownload.Dialogs;

namespace SDownload.Framework
{
    /// <summary>
    /// Handles error reporting to the user
    /// </summary>
    [Serializable]
    public class HandledException : Exception
    {
        /// <summary>
        /// If the exception is critical enough to be reported to Bugsense
        /// </summary>
        public bool IsCritical;

        /// <summary>
        /// Handled exception that reports a message to the user and provides an option
        /// to report if necessary
        /// </summary>
        /// <param name="message">Message to show in the popup, null to use exception message</param>
        /// <param name="canReport">If the exception should be reported to Bugsense</param>
        public HandledException(String message, bool canReport = false) : base(message)
        {
            IsCritical = canReport;
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
                    BugSenseHandler.Instance.ClearCrashExtraData();
                    BugSenseHandler.Instance.ClearBreadCrumbs();
                }
            };
            dialog.Show();
        }

        public static void Throw(String message, bool canReport = true)
        {
            Throw(message, new HandledException(message, canReport), canReport);
        }
    }
}
