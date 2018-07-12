﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class CNAMERecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new CNAMERecord
            {
                Name = "emanon.org",
                Target = "somewhere.else.org"
            };
            var b = (CNAMERecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Target, b.Target);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new CNAMERecord
            {
                Name = "emanon.org",
                Target = "somewhere.else.org"
            };
            var b = (CNAMERecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Target, b.Target);
        }

        [TestMethod]
        public void Equality()
        {
            var a = new CNAMERecord
            {
                Name = "emanon.org",
                Target = "somewhere.else.org"
            };
            var b = new CNAMERecord
            {
                Name = "emanon.org",
                Target = "somewhere.org"
            };
            Assert.IsTrue(a.Equals(a));
            Assert.IsFalse(a.Equals(b));
        }

    }
}
