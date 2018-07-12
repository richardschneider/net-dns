using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class AFSDBRecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new AFSDBRecord
            {
                Name = "emanon.org",
                Subtype = 1,
                Target = "afs.emanon.org"
            };
            var b = (AFSDBRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Subtype, b.Subtype);
            Assert.AreEqual(a.Target, b.Target);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new AFSDBRecord
            {
                Name = "emanon.org",
                Subtype = 1,
                Target = "afs.emanon.org"
            };
            var b = (AFSDBRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Subtype, b.Subtype);
            Assert.AreEqual(a.Target, b.Target);
        }

        [TestMethod]
        public void Equality()
        {
            var a = new AFSDBRecord
            {
                Name = "emanon.org",
                Subtype = 1,
                Target = "afs.emanon.org"
            };
            var b = new AFSDBRecord
            {
                Name = "emanon.org",
                Subtype = 2,
                Target = "afs.emanon.org"
            };
            Assert.IsTrue(a.Equals(a));
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a.Equals(null));
        }

    }
}
