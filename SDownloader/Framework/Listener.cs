using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SDownload.Framework.Streams;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SDownload.Framework
{
    public class Listener : WebSocketService
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            StreamFactory.DownloadTrack(e.Data, new WsReportProxy(this));

            Updater.CheckVersion();
        }

        public void Message(String data)
        {
            Send(data);
        }
    }
}
