using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SDownloadExtensions
{
    internal static class Master
    {
        [STAThread]
        private static void Main(String[] args)
        {
            Application.Run(new ExtensionForm());
        }
    }
}