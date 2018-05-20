using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class EdnsKeepaliveOptionTest
    {
        [TestMethod]
        public void Roundtrip()
        {
            var opt1 = new OPTRecord();
            var expected = new EdnsKeepaliveOption
            {
                Timeout = TimeSpan.FromSeconds(3)
            };
            Assert.AreEqual(EdnsOptionType.Keepalive, expected.Type);
            opt1.Options.Add(expected);

            var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
            var actual = (EdnsKeepaliveOption)opt2.Options[0];
            Assert.AreEqual(expected.Type, actual.Type);
            Assert.AreEqual(expected.Timeout.Value, actual.Timeout.Value);
        }

        [TestMethod]
        public void Roundtrip_Empty()
        {
            var opt1 = new OPTRecord();
            var expected = new EdnsKeepaliveOption();
            Assert.AreEqual(EdnsOptionType.Keepalive, expected.Type);
            Assert.AreEqual(false, expected.Timeout.HasValue);
            opt1.Options.Add(expected);

            var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
            var actual = (EdnsKeepaliveOption)opt2.Options[0];
            Assert.AreEqual(expected.Type, actual.Type);
            Assert.AreEqual(expected.Timeout.HasValue, actual.Timeout.HasValue);
        }
    }
}
