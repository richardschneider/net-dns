using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;

namespace Makaretu.Dns
{

    [TestClass]
    public class DigestRegistryTest
    {
        [TestMethod]
        public void Exists()
        {
            Assert.AreNotEqual(0, DigestRegistry.Digests.Count);
        }

        [TestMethod]
        public void Sha256()
        {
            var hasher = DigestRegistry.Create(DigestType.Sha256);
            Assert.IsInstanceOfType(hasher, typeof(HashAlgorithm));
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void Gost()
        {
            DigestRegistry.Create(DigestType.GostR34_11_94);
        }

        [TestMethod]
        public void RsaSha256()
        {
            var hasher = DigestRegistry.Create(SecurityAlgorithm.RSASHA256);
            Assert.IsInstanceOfType(hasher, typeof(HashAlgorithm));
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void EccGost()
        {
            DigestRegistry.Create(SecurityAlgorithm.ECCGOST);
        }
    }
}
