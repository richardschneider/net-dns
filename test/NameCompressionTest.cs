using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class NameCompressionTest
    {
        [TestMethod]
        public void Writing()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteDomainName("a");
            writer.WriteDomainName("b");
            writer.WriteDomainName("b");
            var bytes = ms.ToArray();
            var expected = new byte[]
            {
                0x01, (byte)'a', 0,
                0x01, (byte)'b', 0,
                0XC0, 3
            };
            CollectionAssert.AreEqual(expected, bytes);
        }

        [TestMethod]
        public void Writing_Labels()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteDomainName("a.b.c");
            writer.WriteDomainName("a.b.c");
            writer.WriteDomainName("b.c");
            writer.WriteDomainName("c");
            writer.WriteDomainName("x.b.c");
            var bytes = ms.ToArray();
            var expected = new byte[]
            {
                0x01, (byte)'a', 0x01, (byte)'b', 0x01, (byte)'c', 00,
                0xC0, 0x00,
                0xC0, 0x02,
                0xC0, 0x04,
                0x01, (byte)'x', 0xC0, 0x02,
            };
            CollectionAssert.AreEqual(expected, bytes);
        }

        [TestMethod]
        public void Writing_Past_MaxPointer()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteBytes(new byte[0x4000]);
            writer.WriteDomainName("a");
            writer.WriteDomainName("b");
            writer.WriteDomainName("b");

            ms.Position = 0;
            var reader = new WireReader(ms);
            reader.ReadBytes(0x4000);
            Assert.AreEqual("a", reader.ReadDomainName());
            Assert.AreEqual("b", reader.ReadDomainName());
            Assert.AreEqual("b", reader.ReadDomainName());
        }

        [TestMethod]
        public void Reading_Labels()
        {
            var bytes = new byte[]
            {
                0x01, (byte)'a', 0x01, (byte)'b', 0x01, (byte)'c', 00,
                0xC0, 0x00,
                0xC0, 0x02,
                0xC0, 0x04,
                0x01, (byte)'x', 0xC0, 0x02,
            };
            var ms = new MemoryStream(bytes);
            var reader = new WireReader(ms);
            Assert.AreEqual("a.b.c", reader.ReadDomainName());
            Assert.AreEqual("a.b.c", reader.ReadDomainName());
            Assert.AreEqual("b.c", reader.ReadDomainName());
            Assert.AreEqual("c", reader.ReadDomainName());
            Assert.AreEqual("x.b.c", reader.ReadDomainName());
        }

        [TestMethod]
        public void Reading()
        {
            var bytes = new byte[]
            {
                0x01, (byte)'a', 0,
                0x01, (byte)'b', 0,
                0XC0, 3
            };
            var ms = new MemoryStream(bytes);
            var reader = new WireReader(ms);
            Assert.AreEqual("a", reader.ReadDomainName());
            Assert.AreEqual("b", reader.ReadDomainName());
            Assert.AreEqual("b", reader.ReadDomainName());
        }

    }
}
