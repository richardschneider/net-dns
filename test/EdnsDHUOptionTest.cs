using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class EdnsDHUOptionTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var opt1 = new OPTRecord();
            var expected = new EdnsDHUOption
            {
                Algorithms = { DigestType.GostR34_11_94, DigestType.Sha512 }
            };
            Assert.AreEqual(EdnsOptionType.DHU, expected.Type);
            opt1.Options.Add(expected);

            var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
            var actual = (EdnsDHUOption)opt2.Options[0];
            Assert.AreEqual(expected.Type, actual.Type);
            CollectionAssert.AreEqual(expected.Algorithms, actual.Algorithms);
        }

        [TestMethod]
        public void Create()
        {
            var option = EdnsDHUOption.Create();
            Assert.AreEqual(EdnsOptionType.DHU, option.Type);
            CollectionAssert.AreEqual(DigestRegistry.Digests.Keys, option.Algorithms);
        }

    }
}
