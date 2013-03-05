using System;
using System.Threading;
using System.Windows.Forms;

namespace SDownload
{
    public class NotifyHandler
    {
        private readonly NotifyIcon _icon;

        public String Text
        {
            set { _icon.Text = value; }
        }

        public NotifyHandler()
        {
            _icon = new NotifyIcon {Icon=Properties.Resources.sdownload, Text="Fetching Information...", BalloonTipTitle="Soundcloud Downloader", Visible = true};
        }

        public void UpdateText(String msg)
        {
            _icon.Text = msg.Substring(0, (msg.Length > 63 ? 63 : msg.Length));
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
            _icon.Visible = false;
            Application.Exit();
        }
    }
}
