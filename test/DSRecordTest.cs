using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class DSRecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new DSRecord
            {
                Name = "dskey.example.com",
                TTL = TimeSpan.FromSeconds(86400),
                KeyTag = 60485,
                Algorithm = SecurityAlgorithm.RSASHA1,
                HashAlgorithm = DigestType.Sha1,
                Digest = Base16.Decode("2BB183AF5F22588179A53B0AAD1A292118")
            };
            var b = (DSRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.KeyTag, b.KeyTag);
            Assert.AreEqual(a.Algorithm, b.Algorithm);
            Assert.AreEqual(a.HashAlgorithm, b.HashAlgorithm);
            CollectionAssert.AreEqual(a.Digest, b.Digest);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new DSRecord
            {
                Name = "dskey.example.com",
                TTL = TimeSpan.FromSeconds(86400),
                KeyTag = 60485,
                Algorithm = SecurityAlgorithm.RSASHA1,
                HashAlgorithm = DigestType.Sha1,
                Digest = Base16.Decode("2BB183AF5F22588179A53B0AAD1A292118")
            };
            var b = (DSRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.KeyTag, b.KeyTag);
            Assert.AreEqual(a.Algorithm, b.Algorithm);
            Assert.AreEqual(a.HashAlgorithm, b.HashAlgorithm);
            CollectionAssert.AreEqual(a.Digest, b.Digest);
        }

    }
}
