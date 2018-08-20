using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class NSECRecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new NSECRecord
            {
                Name = "alfa.example.com",
                TTL = TimeSpan.FromDays(1),
                NextOwnerName = "host.example.com",
                Types = { DnsType.A, DnsType.MX, DnsType.RRSIG, DnsType.NSEC, (DnsType)1234 }
            };
            var b = (NSECRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.NextOwnerName, b.NextOwnerName);
            CollectionAssert.AreEqual(a.Types, b.Types);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new NSECRecord
            {
                Name = "alfa.example.com",
                TTL = TimeSpan.FromDays(1),
                NextOwnerName = "host.example.com",
                Types = { DnsType.A, DnsType.MX, DnsType.RRSIG, DnsType.NSEC, (DnsType)1234 }
            };
            var b = (NSECRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.NextOwnerName, b.NextOwnerName);
            CollectionAssert.AreEqual(a.Types, b.Types);
        }

    }
}
