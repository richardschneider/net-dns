using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class TKEYRecordTest
    {
        [TestMethod]
        public void Defaults()
        {
            var tsig = new TKEYRecord();
            Assert.AreEqual(DnsType.TKEY, tsig.Type);
            Assert.AreEqual(DnsClass.ANY, tsig.Class);
            Assert.AreEqual(TimeSpan.Zero, tsig.TTL);
        }

        [TestMethod]
        public void Roundtrip()
        {
            var now = new DateTime(2018, 8, 13, 23, 59, 59, DateTimeKind.Utc);
            var a = new TKEYRecord
            {
                Name = "host.example.com",
                Algorithm = TSIGRecord.HMACSHA1,
                Inception = now,
                Expiration = now.AddSeconds(15),
                Mode = KeyExchangeMode.DiffieHellman,
                Key = new byte[] { 1, 2, 3, 4 },
                Error = MessageStatus.BadTime,
                OtherData = new byte[] { 5, 6 }
            };
            var b = (TKEYRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Algorithm, b.Algorithm);
            Assert.AreEqual(a.Inception, b.Inception);
            Assert.AreEqual(a.Expiration, b.Expiration);
            Assert.AreEqual(a.Mode, b.Mode);
            CollectionAssert.AreEqual(a.Key, b.Key);
            Assert.AreEqual(a.Error, b.Error);
            CollectionAssert.AreEqual(a.OtherData, b.OtherData);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var now = new DateTime(2018, 8, 13, 23, 59, 59, DateTimeKind.Utc);
            var a = new TKEYRecord
            {
                Name = "host.example.com",
                Algorithm = TSIGRecord.HMACSHA1,
                Inception = now,
                Expiration = now.AddSeconds(15),
                Mode = KeyExchangeMode.DiffieHellman,
                Key = new byte[] { 1, 2, 3, 4 },
                Error = MessageStatus.BadTime,
                OtherData = new byte[] { 5, 6 }
            };
            var b = (TKEYRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Algorithm, b.Algorithm);
            Assert.AreEqual(a.Inception, b.Inception);
            Assert.AreEqual(a.Expiration, b.Expiration);
            Assert.AreEqual(a.Mode, b.Mode);
            CollectionAssert.AreEqual(a.Key, b.Key);
            Assert.AreEqual(a.Error, b.Error);
            CollectionAssert.AreEqual(a.OtherData, b.OtherData);
        }

    }
}
