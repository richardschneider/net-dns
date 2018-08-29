using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class UnknownRecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new UnknownRecord
            {
                Name = "emanon.org",
                Data = new byte[] { 10, 11, 12  },
            };
            var b = (UnknownRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            CollectionAssert.AreEqual(a.Data, b.Data);
        }

        [TestMethod]
        public void Equality()
        {
            var a = new UnknownRecord
            {
                Name = "emanon.org",
                Data = new byte[] { 1, 2, 3, 4 }
            };
            var b = new UnknownRecord
            {
                Name = "emanon.org",
                Data = new byte[] { 1, 2, 3, 40 }
            };
            Assert.IsTrue(a.Equals(a));
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a.Equals(null));
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new UnknownRecord
            {
                Name = "a.example",
                Class = (DnsClass)32,
                Type = (DnsType)731,
                Data = new byte[] { 0xab, 0xcd, 0xef, 0x01, 0x23, 0x45 }
            };
            var b = (UnknownRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            CollectionAssert.AreEqual(a.Data, b.Data);
        }

    }
}
