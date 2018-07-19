using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class AAAARecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new AAAARecord
            {
                Name = "emanon.org",
                Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb")
            };
            var b = (AAAARecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Address, b.Address);
        }

        [TestMethod]
        public void Roundtrip_ScopeId()
        {
            var a = new AAAARecord
            {
                Name = "emanon.org",
                Address = IPAddress.Parse("fe80::7573:b0a8:46b0:bfea%17")
            };
            var b = (AAAARecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(IPAddress.Parse("fe80::7573:b0a8:46b0:bfea"), b.Address);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new AAAARecord
            {
                Name = "emanon.org",
                Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb")
            };
            var b = (AAAARecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Address, b.Address);
        }

        [TestMethod]
        public void Equality()
        {
            var a = new AAAARecord
            {
                Name = "emanon.org",
                Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb")
            };
            var b = new AAAARecord
            {
                Name = "emanon.org",
                Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25ce")
            };
            Assert.IsTrue(a.Equals(a));
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a.Equals(null));
        }
    }
}
