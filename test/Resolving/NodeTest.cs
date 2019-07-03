using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Makaretu.Dns.Resolving
{

    [TestClass]
    public class NodeTest
    {
        [TestMethod]
        public void Defaults()
        {
            var node = new Node();

            Assert.AreEqual(DomainName.Root, node.Name);
            Assert.AreEqual(0, node.Resources.Count);
            Assert.AreEqual("", node.ToString());
        }

        [TestMethod]
        public void DuplicateResources()
        {
            var node = new Node();
            var a = new PTRRecord { Name = "a", DomainName = "alpha" };
            var b = new PTRRecord { Name = "a", DomainName = "alpha" };
            Assert.AreEqual(a, b);

            node.Resources.Add(a);
            node.Resources.Add(b);
            node.Resources.Add(a);
            node.Resources.Add(b);
            Assert.AreEqual(1, node.Resources.Count);
        }
    }
}
