using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class OPTRecordTest
    {
        [TestMethod]
        public void Defaults()
        {
            var opt = new OPTRecord();
            Assert.AreEqual("", opt.Name);
            Assert.AreEqual(1280, opt.RequestorPayloadSize);
            Assert.AreEqual((ushort)opt.Class, opt.RequestorPayloadSize);
            Assert.AreEqual(0, opt.Opcode8);
            Assert.AreEqual(0, opt.Version);
            Assert.AreEqual(false, opt.DO);
            Assert.AreEqual(0, opt.Options.Count);
        }

        [TestMethod]
        public void Roundtrip()
        {
            var a = new OPTRecord
            {
                RequestorPayloadSize = 512,
                Opcode8 = 2,
                Version = 3,
                DO = true
            };
            var b = (OPTRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.RequestorPayloadSize, b.RequestorPayloadSize);
            Assert.AreEqual(a.Opcode8, b.Opcode8);
            Assert.AreEqual(a.Version, b.Version);
            Assert.AreEqual(a.DO, b.DO);
        }

        [TestMethod]
        public void Roundtrip_NoOptions()
        {
            var a = new OPTRecord();
            var b = (OPTRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
        }

        [TestMethod]
        public void Equality()
        {
            var a = new OPTRecord
            {
            };
            var b = new OPTRecord
            {
                RequestorPayloadSize = 512
            };
            Assert.IsTrue(a.Equals(a));
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a.Equals(null));
        }

    }
}
