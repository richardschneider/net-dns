using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class EdnsNSIDOptionTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var opt1 = new OPTRecord();
            var expected = new EdnsNSIDOption
            {
                Id = new byte[] { 1, 2, 3, 4 }
            };
            Assert.AreEqual(EdnsOptionType.NSID, expected.Type);
            opt1.Options.Add(expected);

            var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
            var actual = (EdnsNSIDOption)opt2.Options[0];
            Assert.AreEqual(expected.Type, actual.Type);
            CollectionAssert.AreEqual(expected.Id, actual.Id);
        }

    }
}
