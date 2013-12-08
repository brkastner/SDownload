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
        private bool CanSend = true;

        /// <summary>
        /// Report a string to the remote proxy
        /// </summary>
        /// <param name="info">The information to send</param>
        /// <param name="close">Whether to close the connection afterwards or not</param>
        public void Report(String info, bool close = false)
        {
            if (Remote == null || !CanSend) return;

            try
            {
                Remote(info);
                if (close)
                    Close();
            }
            catch (Exception e)
            {
                CanSend = false;
                HandledException.Throw(e.Message, e);
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
