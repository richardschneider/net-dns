﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class NULLRecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new NULLRecord
            {
                Name = "emanon.org",
                Data = new byte[] { 1, 2, 3, 4}
            };
            var b = (NULLRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            CollectionAssert.AreEqual(a.Data, b.Data);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new NULLRecord
            {
                Name = "emanon.org",
                Data = new byte[] { 1, 2, 3, 4 }
            };
            var b = (NULLRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            CollectionAssert.AreEqual(a.Data, b.Data);
        }

        [TestMethod]
        public void Equality()
        {
            var a = new NULLRecord
            {
                Name = "emanon.org",
                Data = new byte[] { 1, 2, 3, 4 }
            };
            var b = new NULLRecord
            {
                Name = "emanon.org",
                Data = new byte[] { 1, 2, 3, 40 }
            };
            Assert.IsTrue(a.Equals(a));
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a.Equals(null));
        }

    }
}
