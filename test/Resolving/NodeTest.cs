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

            Assert.AreEqual("", node.Name);
            Assert.AreEqual(0, node.Resources.Count);
            Assert.AreEqual("", node.ToString());
        }

    }
}
