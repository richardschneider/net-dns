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

        [TestMethod]
        public void NamesCanBeNull()
        {
            Assert.IsTrue(DnsObject.NamesEquals(null, null));
            Assert.IsFalse(DnsObject.NamesEquals("foo", null));
            Assert.IsFalse(DnsObject.NamesEquals(null, "foo"));
        }

        [TestMethod]
        public void Length_EmptyMessage()
        {
            var message = new Message();
            Assert.AreEqual(Message.MinLength, message.Length());
        }
    }
}
