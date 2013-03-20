using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SDownloadExtensions
{
    public partial class ExtensionForm : Form
    {
        private const String ChromeVersion = "0.2";
        private const String ChromeKey = "Google Chrome";
        private const String ChromeUrl =
            "https://chrome.google.com/webstore/detail/sdownload/dkflmdcolphnomonabinogaegbjbnbbm";

        public ExtensionForm()
        {
            InitializeComponent();

            chromeInstallButton.Click += (sender, args) =>
                                             {
                                                 String chromeEXE = "chrome";
                                                 Process.Start(chromeEXE, ChromeUrl);
                                             };

            // Check for Chrome extension installed
            var extensionPath = Path.GetTempPath() + "..\\Google\\Chrome\\User Data\\Default\\Extensions\\";
            var versionFolder = from extension in Directory.EnumerateDirectories(extensionPath)
                                where
                                    (from version in Directory.EnumerateDirectories(extension)
                                     where version.Contains(ChromeVersion)
                                     select version).Any()
                                select extension;
            if (versionFolder.Any())
            {
                chromeInstallButton.Text = "Up to Date";
            }
        }
    }
}
