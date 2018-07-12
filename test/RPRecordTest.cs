using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class RPRecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new RPRecord
            {
                Name = "emanon.org",
                Mailbox = "nowon.emanon.org"
            };
            var b = (RPRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Mailbox, b.Mailbox);
            Assert.AreEqual(a.TextName, b.TextName);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new RPRecord
            {
                Name = "emanon.org",
                Mailbox = "nowon.emanon.org"
            };
            var b = (RPRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Mailbox, b.Mailbox);
            Assert.AreEqual(a.TextName, b.TextName);
        }

        [TestMethod]
        public void Equality()
        {
            var a = new RPRecord
            {
                Name = "emanon.org",
                Mailbox = "nowon.emanon.org"
            };
            var b = new RPRecord
            {
                Name = "emanon.org",
                Mailbox = "someone.emanon.org"
            };
            Assert.IsTrue(a.Equals(a));
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a.Equals(null));
        }

    }
}
