using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class UnknownEdnsOptionTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var opt1 = new OPTRecord();
            var expected = new UnknownEdnsOption
            {
                Type = EdnsOptionType.ExperimentalMin,
                Data = new byte[] { 10, 11, 12 }
            };
            opt1.Options.Add(expected);

            var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
            var actual = (UnknownEdnsOption)opt2.Options[0];
            Assert.AreEqual(expected.Type, actual.Type);
            CollectionAssert.AreEqual(expected.Data, actual.Data);
        }

    }
}
