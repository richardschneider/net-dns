using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;

namespace Makaretu.Dns
{

    [TestClass]
    public class ResourceRegistryTest
    {
        [TestMethod]
        public void Exists()
        {
            Assert.AreNotEqual(0, ResourceRegistry.Records.Count);
        }

        [TestMethod]
        public void Create()
        {
            var rr = ResourceRegistry.Create(DnsType.NS);
            Assert.IsInstanceOfType(rr, typeof(NSRecord));

            rr = ResourceRegistry.Create((DnsType)1234);
            Assert.IsInstanceOfType(rr, typeof(UnknownRecord));
        }

    }
}
