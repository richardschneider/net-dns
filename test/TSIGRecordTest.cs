using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class TSIGRecordTest
    {
        [TestMethod]
        public void Defaults()
        {
            var tsig = new TSIGRecord();
            Assert.AreEqual(DnsType.TSIG, tsig.Type);
            Assert.AreEqual(DnsClass.ANY, tsig.Class);
            Assert.AreEqual(TimeSpan.Zero, tsig.TTL);
            Assert.AreEqual(DateTimeKind.Utc, tsig.TimeSigned.Kind);
            Assert.AreEqual(0, tsig.TimeSigned.Millisecond);
            Assert.AreEqual(TimeSpan.FromSeconds(300), tsig.Fudge);
        }

        [TestMethod]
        public void Roundtrip()
        {
            var a = new TSIGRecord
            {
                Name = "host.example.com",
                Algorithm = TSIGRecord.HMACMD5,
                TimeSigned = new DateTime(1997, 1, 21, 3, 4, 5, DateTimeKind.Utc),
                Fudge = TimeSpan.FromSeconds(ushort.MaxValue),
                MAC = new byte[] { 1, 2, 3, 4 },
                OriginalMessageId = 0xfbad,
                Error = MessageStatus.BadTime,
                OtherData = new byte[] { 5, 6 }
            };
            var b = (TSIGRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Algorithm, b.Algorithm);
            Assert.AreEqual(a.TimeSigned, b.TimeSigned);
            Assert.AreEqual(a.Fudge, b.Fudge);
            CollectionAssert.AreEqual(a.MAC, b.MAC);
            Assert.AreEqual(a.OriginalMessageId, b.OriginalMessageId);
            Assert.AreEqual(a.Error, b.Error);
            CollectionAssert.AreEqual(a.OtherData, b.OtherData);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new TSIGRecord
            {
                Name = "host.example.com",
                Algorithm = TSIGRecord.HMACMD5,
                TimeSigned = new DateTime(1997, 1, 21, 3, 4, 5, DateTimeKind.Utc),
                Fudge = TimeSpan.FromSeconds(ushort.MaxValue),
                MAC = new byte[] { 1, 2, 3, 4 },
                OriginalMessageId = 0xfbad,
                Error = MessageStatus.BadTime,
            };
            var b = (TSIGRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Algorithm, b.Algorithm);
            Assert.AreEqual(a.TimeSigned, b.TimeSigned);
            CollectionAssert.AreEqual(a.MAC, b.MAC);
            Assert.AreEqual(a.OriginalMessageId, b.OriginalMessageId);
            Assert.AreEqual(a.Error, b.Error);
            CollectionAssert.AreEqual(a.OtherData, b.OtherData);
        }

    }
}
