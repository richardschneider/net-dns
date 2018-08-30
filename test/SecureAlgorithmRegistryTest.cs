using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;

namespace Makaretu.Dns
{

    [TestClass]
    public class SecurityAlgorithmRegistryTest
    {
        [TestMethod]
        public void Exists()
        {
            Assert.AreNotEqual(0, SecurityAlgorithmRegistry.Algorithms.Count);
        }

        [TestMethod]
        public void RSASHA1()
        {
            var metadata = SecurityAlgorithmRegistry.GetMetadata(SecurityAlgorithm.RSASHA1);
            Assert.IsNotNull(metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void UnknownAlgorithm()
        {
            SecurityAlgorithmRegistry.GetMetadata((SecurityAlgorithm)0xBA);
        }

    }
}
