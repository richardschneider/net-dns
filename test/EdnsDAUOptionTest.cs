using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class EdnsDAUOptionTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var opt1 = new OPTRecord();
            var expected = new EdnsDAUOption
            {
                Algorithms = { SecurityAlgorithm.ED25519, SecurityAlgorithm.ECCGOST }
            };
            Assert.AreEqual(EdnsOptionType.DAU, expected.Type);
            opt1.Options.Add(expected);

            var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
            var actual = (EdnsDAUOption)opt2.Options[0];
            Assert.AreEqual(expected.Type, actual.Type);
            CollectionAssert.AreEqual(expected.Algorithms, actual.Algorithms);
        }

        [TestMethod]
        public void Create()
        {
            var option = EdnsDAUOption.Create();
            Assert.AreEqual(EdnsOptionType.DAU, option.Type);
            CollectionAssert.AreEqual(SecurityAlgorithmRegistry.Algorithms.Keys, option.Algorithms);
        }

    }
}
