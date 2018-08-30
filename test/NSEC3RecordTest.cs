using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class NSEC3RecordTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var a = new NSEC3Record
            {
                Name = "2t7b4g4vsa5smi47k61mv5bv1a22bojr.example",
                TTL = TimeSpan.FromDays(1),
                HashAlgorithm = DigestType.Sha1,
                Flags = NSEC3Flags.OptOut,
                Iterations = 12,
                Salt = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd },
                NextHashedOwnerName = Base32.ExtendedHex.Decode("2vptu5timamqttgl4luu9kg21e0aor3s"),
                Types = { DnsType.A, DnsType.RRSIG }
            };
            var b = (NSEC3Record)new ResourceRecord().Read(a.ToByteArray());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.HashAlgorithm, b.HashAlgorithm);
            Assert.AreEqual(a.Flags, b.Flags);
            Assert.AreEqual(a.Iterations, b.Iterations);
            CollectionAssert.AreEqual(a.Salt, b.Salt);
            CollectionAssert.AreEqual(a.NextHashedOwnerName, b.NextHashedOwnerName);
            CollectionAssert.AreEqual(a.Types, b.Types);
        }

        [TestMethod]
        public void Roundtrip_Master()
        {
            var a = new NSEC3Record
            {
                Name = "2t7b4g4vsa5smi47k61mv5bv1a22bojr.example",
                TTL = TimeSpan.FromDays(1),
                HashAlgorithm = DigestType.Sha1,
                Flags = NSEC3Flags.OptOut,
                Iterations = 12,
                Salt = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd },
                NextHashedOwnerName = Base32.ExtendedHex.Decode("2vptu5timamqttgl4luu9kg21e0aor3s"),
                Types = { DnsType.A, DnsType.RRSIG }
            };
            var b = (NSEC3Record)new ResourceRecord().Read(a.ToString());
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Class, b.Class);
            Assert.AreEqual(a.Type, b.Type);
            Assert.AreEqual(a.TTL, b.TTL);
            Assert.AreEqual(a.HashAlgorithm, b.HashAlgorithm);
            Assert.AreEqual(a.Flags, b.Flags);
            Assert.AreEqual(a.Iterations, b.Iterations);
            CollectionAssert.AreEqual(a.Salt, b.Salt);
            CollectionAssert.AreEqual(a.NextHashedOwnerName, b.NextHashedOwnerName);
            CollectionAssert.AreEqual(a.Types, b.Types);
        }

    }
}
