using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class RRSIGRecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var now = new DateTime(2018, 8, 13, 23, 59, 59);
            var a = new RRSIGRecord
            {
                Name = "host.example.com",
                TTL = TimeSpan.FromDays(1),
                TypeCovered = DnsType.A,
                Algorithm = SecurityAlgorithm.RSASHA1,
                Labels = 3,
                OriginalTTL = TimeSpan.FromDays(2),
                SignatureExpiration = now.AddMinutes(15),
                SignatureInception = now,
                KeyTag = 2642,
                SignerName = "example.com",
                Signature = new byte[] { 1, 2, 3}
            };
            var b = (RRSIGRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.TypeCovered, b.TypeCovered);
            Assert.AreEqual(a.Algorithm, b.Algorithm);
            Assert.AreEqual(a.Labels, b.Labels);
            Assert.AreEqual(a.OriginalTTL, b.OriginalTTL);
            Assert.AreEqual(a.SignatureExpiration.ToUniversalTime(), b.SignatureExpiration);
            Assert.AreEqual(a.SignatureInception.ToUniversalTime(), b.SignatureInception);
            Assert.AreEqual(a.KeyTag, b.KeyTag);
            Assert.AreEqual(a.SignerName, b.SignerName);
            CollectionAssert.AreEqual(a.Signature, b.Signature);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var now = new DateTime(2018, 8, 13, 23, 59, 59);
            var a = new RRSIGRecord
            {
                Name = "host.example.com",
                TTL = TimeSpan.FromDays(1),
                TypeCovered = DnsType.A,
                Algorithm = SecurityAlgorithm.RSASHA1,
                Labels = 3,
                OriginalTTL = TimeSpan.FromDays(2),
                SignatureExpiration = now.AddMinutes(15),
                SignatureInception = now,
                KeyTag = 2642,
                SignerName = "example.com",
                Signature = new byte[] { 1, 2, 3 }
            };
            var b = (RRSIGRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.TypeCovered, b.TypeCovered);
            Assert.AreEqual(a.Algorithm, b.Algorithm);
            Assert.AreEqual(a.Labels, b.Labels);
            Assert.AreEqual(a.OriginalTTL, b.OriginalTTL);
            Assert.AreEqual(a.SignatureExpiration.ToUniversalTime(), b.SignatureExpiration);
            Assert.AreEqual(a.SignatureInception.ToUniversalTime(), b.SignatureInception);
            Assert.AreEqual(a.KeyTag, b.KeyTag);
            Assert.AreEqual(a.SignerName, b.SignerName);
            CollectionAssert.AreEqual(a.Signature, b.Signature);
        }

    }
}
