using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class ResourceRecordTest
    {
        [TestMethod]
        public void Defaults()
        {
            var rr = new ResourceRecord();
            Assert.AreEqual(Class.IN, rr.Class);
            Assert.AreEqual(ResourceRecord.DefaultTTL, rr.TTL);
        }

        [TestMethod]
        public void DataLength()
        {
            var rr = new ResourceRecord();
            Assert.AreEqual(0, rr.GetDataLength());
        }

        [TestMethod]
        public void DataLength_DerivedClass()
        {
            var a = new ARecord { Address = IPAddress.Parse("127.0.0.1") };
            Assert.AreEqual(4, a.GetDataLength());
        }

        [TestMethod]
        public void Data()
        {
            var rr = new ResourceRecord();
            Assert.AreEqual(0, rr.GetData().Length);
        }

        [TestMethod]
        public void Data_DerivedClass()
        {
            var a = new ARecord { Address = IPAddress.Parse("127.0.0.1") };
            Assert.AreNotEqual(0, a.GetData().Length);
        }

        [TestMethod]
        public void RoundTrip()
        {
            var a = new ResourceRecord
            {
                Name = "emanon.org",
                Class = Class.CH,
                Type = (DnsType)0xFFFF,
                TTL = TimeSpan.FromDays(2)
            };
            var b = (ResourceRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.GetDataLength(), b.GetDataLength());
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.IsInstanceOfType(b, typeof(ResourceRecord));
        }

        [TestMethod]
        public void Value_Equality()
        {
            var a0 = new ResourceRecord
            {
                Name = "alpha",
                Class = Class.IN,
                Type = DnsType.A,
                TTL = TimeSpan.FromSeconds(1)
            };
            var a1 = new ResourceRecord
            {
                Name = "alpha",
                Class = Class.IN,
                Type = DnsType.A,
                TTL = TimeSpan.FromSeconds(2)
            };
            var b = new ResourceRecord
            {
                Name = "beta",
                Class = Class.IN,
                Type = DnsType.A,
                TTL = TimeSpan.FromSeconds(1)
            };
            ResourceRecord c = null;
            ResourceRecord d = null;
            ResourceRecord e = new ResourceRecord();

            Assert.IsTrue(c == d);
            Assert.IsFalse(c == b);
            Assert.IsFalse(b == c);

            Assert.IsFalse(c != d);
            Assert.IsTrue(c != b);
            Assert.IsTrue(b != c);

#pragma warning disable 1718
            Assert.IsTrue(a0 == a0);
            Assert.IsTrue(a0 == a1);
            Assert.IsFalse(a0 == b);

            Assert.IsFalse(a0 != a0);
            Assert.IsFalse(a0 != a1);
            Assert.IsTrue(a0 != b);

            Assert.IsTrue(a0.Equals(a0));
            Assert.IsTrue(a0.Equals(a1));
            Assert.IsFalse(a0.Equals(b));

            Assert.AreEqual(a0, a0);
            Assert.AreEqual(a0, a1);
            Assert.AreNotEqual(a0, b);

            Assert.AreEqual<ResourceRecord>(a0, a0);
            Assert.AreEqual<ResourceRecord>(a0, a1);
            Assert.AreNotEqual<ResourceRecord>(a0, b);

            Assert.AreEqual(e, e);
            Assert.AreNotEqual(e, a0);

            Assert.AreEqual(a0.GetHashCode(), a0.GetHashCode());
            Assert.AreEqual(a0.GetHashCode(), a1.GetHashCode());
            Assert.AreNotEqual(a0.GetHashCode(), b.GetHashCode());
            Assert.AreEqual(e.GetHashCode(), e.GetHashCode());
        }

    }
}
