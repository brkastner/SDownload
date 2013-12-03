using System;
using Alchemy.Classes;
using SDownload.Framework;

namespace SDownload
{
    /// <summary>
    /// Provides an interface for communicating with a remote websocket client
    /// </summary>
    public class WSReportProxy : InfoReportProxy
    {
        /// <summary>
        /// The Websocket context
        /// </summary>
        private readonly UserContext _context;

        /// <summary>
        /// Creates a report proxy that can send information to a
        /// websocket client
        /// </summary>
        /// <param name="context"></param>
        public WSReportProxy(UserContext context)
        {
            if (context == null) return;

            _context = context;
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
                _context.Send(info);
            }
            catch (Exception e)
            {
                throw new Exception("There was an issue communicating with the UI!", e);
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
