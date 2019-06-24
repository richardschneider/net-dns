using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class WireReaderWriterTest
    {
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const ulong uint48MaxValue = 0XFFFFFFFFFFFFul;

        [TestMethod]
        public void Roundtrip()
        {
            var someBytes = new byte[] { 1, 2, 3 };
            var someDate = new DateTime(1997, 1, 21, 3, 4, 5, DateTimeKind.Utc);
            
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteDomainName("emanon.org");
            writer.WriteString("alpha");
            writer.WriteTimeSpan32(TimeSpan.FromHours(3));
            writer.WriteUInt16(ushort.MaxValue);
            writer.WriteUInt32(uint.MaxValue);
            writer.WriteUInt48(0XFFFFFFFFFFFFul);
            writer.WriteBytes(someBytes);
            writer.WriteByteLengthPrefixedBytes(someBytes);
            writer.WriteByteLengthPrefixedBytes(null);
            writer.WriteIPAddress(IPAddress.Parse("127.0.0.1"));
            writer.WriteIPAddress(IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb"));
            writer.WriteDateTime32(someDate);
            writer.WriteDateTime48(someDate);

            ms.Position = 0;
            var reader = new WireReader(ms);
            Assert.AreEqual("emanon.org", reader.ReadDomainName());
            Assert.AreEqual("alpha", reader.ReadString());
            Assert.AreEqual(TimeSpan.FromHours(3), reader.ReadTimeSpan32());
            Assert.AreEqual(ushort.MaxValue, reader.ReadUInt16());
            Assert.AreEqual(uint.MaxValue, reader.ReadUInt32());
            Assert.AreEqual(0XFFFFFFFFFFFFul, reader.ReadUInt48());
            CollectionAssert.AreEqual(someBytes, reader.ReadBytes(3));
            CollectionAssert.AreEqual(someBytes, reader.ReadByteLengthPrefixedBytes());
            CollectionAssert.AreEqual(new byte[0], reader.ReadByteLengthPrefixedBytes());
            Assert.AreEqual(IPAddress.Parse("127.0.0.1"), reader.ReadIPAddress());
            Assert.AreEqual(IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb"), reader.ReadIPAddress(16));
            Assert.AreEqual(someDate, reader.ReadDateTime32());
            Assert.AreEqual(someDate, reader.ReadDateTime48());
        }

        [TestMethod]
        public void Write_DomainName()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteDomainName("a.b");

            ms.Position = 0;
            Assert.AreEqual(1, ms.ReadByte(), "length of 'a'");
            Assert.AreEqual('a', (char)ms.ReadByte());
            Assert.AreEqual(1, ms.ReadByte(), "length of 'b'");
            Assert.AreEqual('b', (char)ms.ReadByte());
            Assert.AreEqual(0, ms.ReadByte(), "trailing nul");
        }

        [TestMethod]
        public void Write_EscapedDomainName()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteDomainName(@"a\.b");

            ms.Position = 0;
            Assert.AreEqual(3, ms.ReadByte(), "length of 'a.b'");
            Assert.AreEqual('a', (char)ms.ReadByte());
            Assert.AreEqual('.', (char)ms.ReadByte());
            Assert.AreEqual('b', (char)ms.ReadByte());
            Assert.AreEqual(0, ms.ReadByte(), "trailing nul");
        }

        [TestMethod]
        public void BufferOverflow_Byte()
        {
            var ms = new MemoryStream(new byte[0]);
            var reader = new WireReader(ms);
            ExceptionAssert.Throws<EndOfStreamException>(() => reader.ReadByte());
        }

        [TestMethod]
        public void BufferOverflow_Bytes()
        {
            var ms = new MemoryStream(new byte[] { 1, 2 });
            var reader = new WireReader(ms);
            ExceptionAssert.Throws<EndOfStreamException>(() => reader.ReadBytes(3));
        }

        [TestMethod]
        public void BufferOverflow_DomainName()
        {
            var ms = new MemoryStream(new byte[] { 1, (byte)'a' });
            var reader = new WireReader(ms);
            ExceptionAssert.Throws<EndOfStreamException>(() => reader.ReadDomainName());
        }

        [TestMethod]
        public void BufferOverflow_String()
        {
            var ms = new MemoryStream(new byte[] { 10, 1 });
            var reader = new WireReader(ms);
            ExceptionAssert.Throws<EndOfStreamException>(() => reader.ReadString());
        }

        [TestMethod]
        public void BytePrefixedArray_TooBig()
        {
            var bytes = new byte[byte.MaxValue + 1];
            var writer = new WireWriter(new MemoryStream());
            ExceptionAssert.Throws<ArgumentException>(() => writer.WriteByteLengthPrefixedBytes(bytes));
        }

        [TestMethod]
        public void LengthPrefixedScope()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteString("abc");
            writer.PushLengthPrefixedScope();
            writer.WriteDomainName("a");
            writer.WriteDomainName("a");
            writer.PopLengthPrefixedScope();

            ms.Position = 0;
            var reader = new WireReader(ms);
            Assert.AreEqual("abc", reader.ReadString());
            Assert.AreEqual(5, reader.ReadUInt16());
            Assert.AreEqual("a", reader.ReadDomainName());
            Assert.AreEqual("a", reader.ReadDomainName());
        }

        [TestMethod]
        public void EmptyDomainName()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteDomainName("");
            writer.WriteString("abc");

            ms.Position = 0;
            var reader = new WireReader(ms);
            Assert.AreEqual("", reader.ReadDomainName());
            Assert.AreEqual("abc", reader.ReadString());
        }

        [TestMethod]
        public void CanonicalDomainName()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms) { CanonicalForm = true };
            writer.WriteDomainName("FOO");
            writer.WriteDomainName("FOO");
            Assert.AreEqual(5 * 2, writer.Position);

            ms.Position = 0;
            var reader = new WireReader(ms);
            Assert.AreEqual("foo", reader.ReadDomainName());
            Assert.AreEqual("foo", reader.ReadDomainName());
        }

        [TestMethod]
        public void NullDomainName_String()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteDomainName((string) null);
            writer.WriteString("abc");

            ms.Position = 0;
            var reader = new WireReader(ms);
            Assert.AreEqual("", reader.ReadDomainName());
            Assert.AreEqual("abc", reader.ReadString());
        }

        [TestMethod]
        public void NullDomainName_Class()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteDomainName((DomainName)null);
            writer.WriteString("abc");

            ms.Position = 0;
            var reader = new WireReader(ms);
            Assert.AreEqual("", reader.ReadDomainName());
            Assert.AreEqual("abc", reader.ReadString());
        }

        [TestMethod]
        public void Read_EscapedDotDomainName()
        {
            var domainName = @"a\.b";
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteDomainName(domainName);

            ms.Position = 0;
            var reader = new WireReader(ms);
            var name = reader.ReadDomainName();
            Assert.AreEqual(domainName, name);
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
            var reader = new WireReader(ms);
            var first = new ushort[] { 1, 15, 46, 47 };
            var second = new ushort[] { 1234 };
            CollectionAssert.AreEqual(first, reader.ReadBitmap());
            CollectionAssert.AreEqual(second, reader.ReadBitmap());

            ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteBitmap(new ushort[] { 1, 15, 46, 47, 1234 });
            CollectionAssert.AreEqual(wire, ms.ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Uint48TooBig()
        {
            var ms = new MemoryStream();
            var writer = new WireWriter(ms);
            writer.WriteUInt48(0X1FFFFFFFFFFFFul);
        }

        [TestMethod]
        public void ReadDateTime48()
        {
            // From https://tools.ietf.org/html/rfc2845 section 3.3
            var expected = new DateTime(1997, 1, 21, 0, 0, 0, DateTimeKind.Utc);
            var ms = new MemoryStream(new byte[] { 0x00, 0x00, 0x32, 0xe4, 0x07, 0x00 });
            var reader = new WireReader(ms);
            Assert.AreEqual(expected, reader.ReadDateTime48());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteString_NotAscii()
        {
            var writer = new WireWriter(Stream.Null);
            writer.WriteString("δοκιμή"); // test in Greek
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteString_TooBig()
        {
            var writer = new WireWriter(Stream.Null);
            writer.WriteString(new string('a', 0x100));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void ReadString_NotAscii()
        {
            var ms = new MemoryStream(new byte[] { 1, 0xFF });
            var reader = new WireReader(ms);
            reader.ReadString();
        }

        [TestMethod]
        public void WriteDateTime32_TooManySeconds()
        {
            var writer = new WireWriter(Stream.Null);
            writer.WriteDateTime32(UnixEpoch);
            writer.WriteDateTime32(UnixEpoch.AddSeconds(uint.MaxValue));

            ExceptionAssert.Throws<OverflowException>(() =>
            {
                writer.WriteDateTime32(UnixEpoch.AddSeconds((long)(uint.MaxValue) + 1));
            });
        }

    }
}
