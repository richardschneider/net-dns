using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class NSRecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new NSRecord
            {
                Name = "emanon.org",
                Authority = "mydomain.name"
            };
            var b = (NSRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Authority, b.Authority);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new NSRecord
            {
                Name = "emanon.org",
                Authority = "mydomain.name"
            };
            var b = (NSRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Authority, b.Authority);
        }

        [TestMethod]
        public void Equality()
        {
            var a = new NSRecord
            {
                Name = "emanon.org",
                Authority = "mydomain.name"
            };
            var b = new NSRecord
            {
                Name = "emanon.org",
                Authority = "mydomainx.name"
            };
            Assert.IsTrue(a.Equals(a));
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a.Equals(null));
        }

    }
}
