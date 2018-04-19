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
        public void Value_Equality()
        {
            var a0 = new ResourceRecord
            {
                Name = "alpha",
                Class = Class.IN,
                Type = 1,
                TTL = TimeSpan.FromSeconds(1)
            };
            var a1 = new ResourceRecord
            {
                Name = "alpha",
                Class = Class.IN,
                Type = 1,
                TTL = TimeSpan.FromSeconds(2)
            };
            var b = new ResourceRecord
            {
                Name = "beta",
                Class = Class.IN,
                Type = 1,
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
