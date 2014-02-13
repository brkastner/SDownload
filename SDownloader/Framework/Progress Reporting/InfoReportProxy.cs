using System;

namespace SDownload.Framework
{
    /// <summary>
    /// Provides a base proxy for communicating progress updates to a client.
    /// </summary>
    public class InfoReportProxy
    {
        /// <summary>
        /// The function to send the updated information
        /// </summary>
        protected Action<String> Remote;

        /// <summary>
        /// If information can still be sent to the view
        /// </summary>
        private bool _canSend = true;

        /// <summary>
        /// Reports the current progress to the remote proxy
        /// </summary>
        /// <param name="percentage">The percentage completed</param>
        public virtual void UpdateProgress(int percentage)
        {
            Report(String.Format("{0}%", percentage));
        }

        /// <summary>
        /// Report a string to the remote proxy
        /// </summary>
        /// <param name="info">The information to send</param>
        /// <param name="close">Whether to close the connection afterwards or not</param>
        public virtual void Report(String info, bool close = false)
        {
            if (Remote == null || !_canSend) return;

            try
            {
                Remote(info);
                if (close)
                    Close();
            }
            catch (Exception e)
            {
                _canSend = false;
                CrashHandler.Throw(e.Message, e);
            }
        }

        /// <summary>
        /// Close the connection to the client
        /// </summary>
        public virtual void Close()
        {
            // Do nothing by default
        }
    }
}
