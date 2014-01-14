using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDownload.Framework;

namespace SDownload.Tests.Framework.Progress_Reporting
{
    [TestClass]
    public class InfoReportProxyTest : InfoReportProxy
    {
        private String output;

        [TestInitialize]
        public void BeforeTests()
        {
            output = "";
            Remote = (s) => { output = s; };
        }

        [TestMethod]
        public void TestUpdateProgress()
        {
            UpdateProgress(10);
            StringAssert.Equals(output, 10 + "%");
            UpdateProgress(0);
            StringAssert.Equals(output, 0 + "%");
            UpdateProgress(100);
            StringAssert.Equals(output, 100 + "%");
        }

        [TestMethod]
        public void TestReport()
        {
            Report("Information");
            StringAssert.Equals("Information", output);

            Report("");
            StringAssert.Equals("", output);

            Report(null);
            Assert.IsNull(output);
        }
    }
}
