using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class DnsObjectTest
    {
        [TestMethod]
        public void NamesAreCaseInsenstive()
        {
            Assert.IsTrue(DnsObject.NamesEquals("fOo", "FoO"));
            Assert.IsFalse(DnsObject.NamesEquals("foo", "bar"));
        }
    }
}
