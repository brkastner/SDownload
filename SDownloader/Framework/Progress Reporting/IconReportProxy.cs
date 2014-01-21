using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDownload.Framework
{
    /// <summary>
    /// Report proxy for a system tray icon
    /// </summary>
    public class IconReportProxy : InfoReportProxy
    {
        /// <summary>
        /// The icon to show the tooltip from
        /// </summary>
        private NotifyIcon _icon;

        /// <summary>
        /// Creates a report proxy to a tray icon
        /// </summary>
        /// <param name="icon">The tray icon to use</param>
        public IconReportProxy(NotifyIcon icon)
        {
            _icon = icon;
            Remote = IconReport;
        }

        /// <summary>
        /// Displays a tooltip from the tray icon
        /// </summary>
        /// <param name="message">The message to show</param>
        private void IconReport(String message)
        {
            lock (_icon)
            {
                _icon.BalloonTipTitle = "Downloading... ";
                _icon.BalloonTipText = message;
                _icon.ShowBalloonTip(300);
            }
        }
    }
}
