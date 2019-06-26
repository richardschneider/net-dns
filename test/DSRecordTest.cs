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
                Digest = Base16.Decode("2BB183AF5F22588179A53B0A98631FAD1A292118")
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
                Digest = Base16.Decode("2BB183AF5F22588179A53B0A98631FAD1A292118")
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

        [TestMethod]
        public void FromDNSKEY()
        {
            // From https://tools.ietf.org/html/rfc4034#section-5.4
            var key = new DNSKEYRecord
            {
                Name = "dskey.example.com",
                TTL = TimeSpan.FromSeconds(86400),
                Flags = DNSKEYFlags.ZoneKey,
                Algorithm = SecurityAlgorithm.RSASHA1,
                PublicKey = Convert.FromBase64String(
                    @"AQOeiiR0GOMYkDshWoSKz9Xz
                      fwJr1AYtsmx3TGkJaNXVbfi/
                      2pHm822aJ5iI9BMzNXxeYCmZ
                      DRD99WYwYqUSdjMmmAphXdvx
                      egXd/M5+X7OrzKBaMbCVdFLU
                      Uh6DhweJBjEVv5f2wwjM9Xzc
                      nOf+EPbtG9DMBmADjFDc2w/r
                      ljwvFw==")
            };
            var ds = new DSRecord(key, force: true);
            Assert.AreEqual(key.Name, ds.Name);
            Assert.AreEqual(key.Class, ds.Class);
            Assert.AreEqual(DnsType.DS, ds.Type);
            Assert.AreEqual(key.TTL, ds.TTL);
            Assert.AreEqual(60485, ds.KeyTag);
            Assert.AreEqual(SecurityAlgorithm.RSASHA1, ds.Algorithm);
            Assert.AreEqual(DigestType.Sha1, ds.HashAlgorithm);
            CollectionAssert.AreEqual(Base16.Decode("2BB183AF5F22588179A53B0A98631FAD1A292118"), ds.Digest);
        }

        [TestMethod]
        public void FromDNSKEY_Missing_ZK()
        {
            var key = new DNSKEYRecord
            {
                Name = "example.com",
                Flags = DNSKEYFlags.SecureEntryPoint,
                Algorithm = SecurityAlgorithm.RSASHA1,
                PublicKey = Convert.FromBase64String(
                    @"AQOeiiR0GOMYkDshWoSKz9Xz
                      fwJr1AYtsmx3TGkJaNXVbfi/
                      2pHm822aJ5iI9BMzNXxeYCmZ
                      DRD99WYwYqUSdjMmmAphXdvx
                      egXd/M5+X7OrzKBaMbCVdFLU
                      Uh6DhweJBjEVv5f2wwjM9Xzc
                      nOf+EPbtG9DMBmADjFDc2w/r
                      ljwvFw==")
            };
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var ds = new DSRecord(key);
            });
        }

        [TestMethod]
        public void FromDNSKEY_Missing_SEP()
        {
            var key = new DNSKEYRecord
            {
                Name = "example.com",
                Flags = DNSKEYFlags.ZoneKey,
                Algorithm = SecurityAlgorithm.RSASHA1,
                PublicKey = Convert.FromBase64String(
                    @"AQOeiiR0GOMYkDshWoSKz9Xz
                      fwJr1AYtsmx3TGkJaNXVbfi/
                      2pHm822aJ5iI9BMzNXxeYCmZ
                      DRD99WYwYqUSdjMmmAphXdvx
                      egXd/M5+X7OrzKBaMbCVdFLU
                      Uh6DhweJBjEVv5f2wwjM9Xzc
                      nOf+EPbtG9DMBmADjFDc2w/r
                      ljwvFw==")
            };
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var ds = new DSRecord(key);
            });
        }

    }
}
