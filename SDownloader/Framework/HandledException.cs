using System;
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
        /// <param name="log">If the exception should be reported to Bugsense</param>
        public HandledException(String message, bool log = false) : base(message)
        {
            IsCritical = log;
        }

        /// <summary>
        /// Reports an error to the user, and provides the option to report if the crash is critical enough
        /// </summary>
        /// <param name="message">Message to show to the user</param>
        /// <param name="inner">The exception that was thrown/raised</param>
        /// <param name="log">If the crash should be allowed to be reported to Bugsense</param>
        public static void Throw(String message, Exception inner, bool log = true)
        {
            var handled = inner as HandledException;
            var criticalHandled = handled != null && handled.IsCritical;
            // Use the extension message if it is of type HandledException or a message wasn't passed through
            // If it is a HandledException, only provide the option to report if the exception is critical
            var dialog = new YesNoDialog("ERROR:\n" + (handled != null ? inner.Message : (message ?? inner.Message)),
                (handled == null && log) || criticalHandled ? "Report" : "Close",
                (handled == null && log) || criticalHandled ? "Close" : null)
            {
                ResponseCallback = (result) =>
                {
                    // Log exception only if 'Report' was chosen, and the exception was critical
                    if (result && log)
                        BugSenseHandler.Instance.SendExceptionAsync(inner);
                    BugSenseHandler.Instance.ClearCrashExtraData();
                    BugSenseHandler.Instance.ClearBreadCrumbs();
                }
            };
            dialog.Show();
        }
    }
}
