using System;
using System.Threading;
using System.Windows.Forms;

namespace SDownload
{
    public class NotifyHandler
    {
        private readonly NotifyIcon _icon;

        public NotifyHandler()
        {
            _icon = new NotifyIcon {Icon=Properties.Resources.NotifyIcon1, BalloonTipTitle="Soundcloud Downloader", Visible = true};
        }

        public void Show(String msg, bool close = false)
        {
            _icon.BalloonTipText = msg;
            _icon.ShowBalloonTip(500);

            if (!close) return;

            Thread.Sleep(500);
            Close();
        }

        public void Close()
        {
            Application.Exit();
        }
    }
}
