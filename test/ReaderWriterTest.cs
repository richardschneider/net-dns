using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class ReaderWriterTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var someBytes = new byte[] { 1, 2, 3 };
            var ms = new MemoryStream();
            var writer = new DnsWriter(ms);
            writer.WriteDomainName("emanon.org");
            writer.WriteString("alpha");
            writer.WriteTimeSpan(TimeSpan.FromHours(3));
            writer.WriteUInt16(ushort.MaxValue);
            writer.WriteUInt32(uint.MaxValue);
            writer.WriteBytes(someBytes);
            writer.WriteByteLengthPrefixedBytes(someBytes);
            writer.WriteByteLengthPrefixedBytes(null);
            writer.WriteIPAddress(IPAddress.Parse("127.0.0.1"));
            writer.WriteIPAddress(IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb"));

            ms.Position = 0;
            var reader = new DnsReader(ms);
            Assert.AreEqual("emanon.org", reader.ReadDomainName());
            Assert.AreEqual("alpha", reader.ReadString());
            Assert.AreEqual(TimeSpan.FromHours(3), reader.ReadTimeSpan());
            Assert.AreEqual(ushort.MaxValue, reader.ReadUInt16());
            Assert.AreEqual(uint.MaxValue, reader.ReadUInt32());
            CollectionAssert.AreEqual(someBytes, reader.ReadBytes(3));
            CollectionAssert.AreEqual(someBytes, reader.ReadByteLengthPrefixedBytes());
            CollectionAssert.AreEqual(new byte[0], reader.ReadByteLengthPrefixedBytes());
            Assert.AreEqual(IPAddress.Parse("127.0.0.1"), reader.ReadIPAddress());
            Assert.AreEqual(IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb"), reader.ReadIPAddress(16));
        }

        [TestMethod]
        public void BufferOverflow_Byte()
        {
            var ms = new MemoryStream(new byte[0]);
            var reader = new DnsReader(ms);
            ExceptionAssert.Throws<EndOfStreamException>(() => reader.ReadByte());
        }

        [TestMethod]
        public void BufferOverflow_Bytes()
        {
            var ms = new MemoryStream(new byte[] { 1, 2 });
            var reader = new DnsReader(ms);
            ExceptionAssert.Throws<EndOfStreamException>(() => reader.ReadBytes(3));
        }

        [TestMethod]
        public void BufferOverflow_DomainName()
        {
            var ms = new MemoryStream(new byte[] { 1, (byte)'a' });
            var reader = new DnsReader(ms);
            ExceptionAssert.Throws<EndOfStreamException>(() => reader.ReadDomainName());
        }

        [TestMethod]
        public void BufferOverflow_String()
        {
            var ms = new MemoryStream(new byte[] { 10, 1 });
            var reader = new DnsReader(ms);
            ExceptionAssert.Throws<EndOfStreamException>(() => reader.ReadString());
        }

        [TestMethod]
        public void BytePrefixedArray_TooBig()
        {
            var bytes = new byte[byte.MaxValue + 1];
            var writer = new DnsWriter(new MemoryStream());
            ExceptionAssert.Throws<ArgumentException>(() => writer.WriteByteLengthPrefixedBytes(bytes));
        }

        [TestMethod]
        public void LengthPrefixedScope()
        {
            var ms = new MemoryStream();
            var writer = new DnsWriter(ms);
            writer.WriteString("abc");
            writer.PushLengthPrefixedScope();
            writer.WriteDomainName("a");
            writer.WriteDomainName("a");
            writer.PopLengthPrefixedScope();

            ms.Position = 0;
            var reader = new DnsReader(ms);
            Assert.AreEqual("abc", reader.ReadString());
            Assert.AreEqual(5, reader.ReadUInt16());
            Assert.AreEqual("a", reader.ReadDomainName());
            Assert.AreEqual("a", reader.ReadDomainName());
        }

        [TestMethod]
        public void EmptyDomainName()
        {
            var ms = new MemoryStream();
            var writer = new DnsWriter(ms);
            writer.WriteDomainName("");
            writer.WriteString("abc");

            ms.Position = 0;
            var reader = new DnsReader(ms);
            Assert.AreEqual("", reader.ReadDomainName());
            Assert.AreEqual("abc", reader.ReadString());
        }

        [TestMethod]
        public void CanonicalDomainName()
        {
            var ms = new MemoryStream();
            var writer = new DnsWriter(ms) { CanonicalForm = true };
            writer.WriteDomainName("FOO");
            writer.WriteDomainName("FOO");
            Assert.AreEqual(5 * 2, writer.Position);

            ms.Position = 0;
            var reader = new DnsReader(ms);
            Assert.AreEqual("foo", reader.ReadDomainName());
            Assert.AreEqual("foo", reader.ReadDomainName());
        }

        [TestMethod]
        public void NullDomainName()
        {
            var ms = new MemoryStream();
            var writer = new DnsWriter(ms);
            writer.WriteDomainName(null);
            writer.WriteString("abc");

            ms.Position = 0;
            var reader = new DnsReader(ms);
            Assert.AreEqual("", reader.ReadDomainName());
            Assert.AreEqual("abc", reader.ReadString());
        }

        [TestMethod]
        public void Bitmap()
        {
            // From https://tools.ietf.org/html/rfc3845#section-2.3
            var wire = new byte[]
            {
                0x00, 0x06, 0x40, 0x01, 0x00, 0x00, 0x00, 0x03,
                0x04, 0x1b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x20
            };
            var ms = new MemoryStream(wire, false);
            var reader = new DnsReader(ms);
            var first = new ushort[] { 1, 15, 46, 47 };
            var second = new ushort[] { 1234 };
            CollectionAssert.AreEqual(first, reader.ReadBitmap());
            CollectionAssert.AreEqual(second, reader.ReadBitmap());

            ms = new MemoryStream();
            var writer = new DnsWriter(ms);
            writer.WriteBitmap(new ushort[] { 1, 15, 46, 47, 1234 });
            CollectionAssert.AreEqual(wire, ms.ToArray());
        }
    }
}
