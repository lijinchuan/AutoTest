using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoTest.Biz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Biz.Tests
{
    [TestClass()]
    public class CronHelperTests
    {
        [TestMethod()]
        public void GetNextDateTimeTest()
        {
            var t = CronHelper.GetNextDateTime("0 0 6 * * ? *", new DateTime(2022, 5, 5, 6, 0, 0));
            Assert.Fail();
        }
    }
}