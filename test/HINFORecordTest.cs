﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class HINFORecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new HINFORecord
            {
                Name = "emanaon.org",
                Cpu = "DEC-2020",
                OS = "TOPS20"
            };
            var b = (HINFORecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Cpu, b.Cpu);
            Assert.AreEqual(a.OS, b.OS);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new HINFORecord
            {
                Name = "emanaon.org",
                Cpu = "DEC-2020",
                OS = "TOPS20"
            };
            var b = (HINFORecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Cpu, b.Cpu);
            Assert.AreEqual(a.OS, b.OS);
        }

        [TestMethod]
        public void Equality()
        {
            var a = new HINFORecord
            {
                Name = "emanaon.org",
                Cpu = "DEC-2020",
                OS = "TOPS20"
            };
            var b = new HINFORecord
            {
                Name = "emanaon.org",
                Cpu = "DEC-2040",
                OS = "TOPS20"
            };
            Assert.IsTrue(a.Equals(a));
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a.Equals(null));
        }

    }
}
