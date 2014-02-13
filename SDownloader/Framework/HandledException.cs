using System;

namespace SDownload.Framework
{
    /// <summary>
    /// Represents an exception that was created manually due to an issue that occured during execution
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
    }
}
