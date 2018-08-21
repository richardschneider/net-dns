using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class DNSKEYRecordTest
    {
        byte[] key = Convert.FromBase64String("AQPSKmynfzW4kyBv015MUG2DeIQ3Cbl+BBZH4b/0PY1kxkmvHjcZc8nokfzj31GajIQKY+5CptLr3buXA10hWqTkF7H6RfoRqXQeogmMHfpftf6zMv1LyBUgia7za6ZEzOJBOztyvhjL742iU/TpPSEDhm2SNKLijfUppn1UaNvv4w==");

        [TestMethod]
        public void Roundtrip()
        {
            var a = new DNSKEYRecord
            {
                Name = "example.com",
                TTL = TimeSpan.FromDays(2),
                Flags = 256,
                Protocol = 3,
                Algorithm = SecurityAlgorithm.RSASHA1,
                PublicKey = key
            };
            var b = (DNSKEYRecord)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Flags, b.Flags);
            Assert.AreEqual(a.Protocol, b.Protocol);
            Assert.AreEqual(a.Algorithm, b.Algorithm);
            CollectionAssert.AreEqual(a.PublicKey, b.PublicKey);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new DNSKEYRecord
            {
                Name = "example.com",
                TTL = TimeSpan.FromDays(2),
                Flags = 256,
                Protocol = 3,
                Algorithm = SecurityAlgorithm.RSASHA1,
                PublicKey = key
            };
            var b = (DNSKEYRecord)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.Flags, b.Flags);
            Assert.AreEqual(a.Protocol, b.Protocol);
            Assert.AreEqual(a.Algorithm, b.Algorithm);
            CollectionAssert.AreEqual(a.PublicKey, b.PublicKey);
        }

        [TestMethod]
        public void KeyTag()
        {
            // From https://tools.ietf.org/html/rfc4034#section-5.4
            var a = new DNSKEYRecord
            {
                Name = "dskey.example.com",
                TTL = TimeSpan.FromSeconds(86400),
                Flags = 256,
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
            Assert.AreEqual(60485, a.KeyTag());
        }

    }
}
