using System;
using Alchemy.Classes;
using SDownload.Framework;

namespace SDownload.Framework
{
    /// <summary>
    /// Provides an interface for communicating with a remote websocket client
    /// </summary>
    public class WSReportProxy : InfoReportProxy
    {
        /// <summary>
        /// The Websocket context
        /// </summary>
        internal readonly UserContext Context;

        /// <summary>
        /// Creates a report proxy that can send information to a
        /// websocket client
        /// </summary>
        /// <param name="context"></param>
        public WSReportProxy(UserContext context)
        {
            if (context == null) return;

            Context = context;
            Remote = WSReport;
        }

        /// <summary>
        /// Sends a string to the remote websocket client
        /// passing in default values for the second and third param
        /// </summary>
        /// <param name="info">The information to send</param>
        private void WSReport(String info)
        {
            try
            {
                Context.Send(info);
            }
            catch (Exception e)
            {
                CrashHandler.Throw("There was an issue communicating with the UI!", e);
            }
        }

        /// <summary>
        /// Close the connection to the remote client
        /// </summary>
        public override void Close()
        {
            Report("CLOSE");
        }
    }
}
