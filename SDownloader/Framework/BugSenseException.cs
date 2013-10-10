using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDownload.Framework
{
    /// <summary>
    /// Builds upon an exception to send additional information to Bugsense for analysis
    /// </summary>
    public class BugSenseException : Exception
    {
        private String _url;

        public BugSenseException(String url, String message = "") : base(message)
        {
            _url = url;
        }

        public override string Message
        {
            get
            {
                return String.Format("(URL:{0})\n{1}", _url, base.Message);
            }
        }
    }
}
