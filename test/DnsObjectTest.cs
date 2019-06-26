using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class DnsObjectTest
    {
        [TestMethod]
        public void Length_EmptyMessage()
        {
            var message = new Message();
            Assert.AreEqual(Message.MinLength, message.Length());
        }

        [TestMethod]
        public void Clone()
        {
            var m1 = new Message
            {
                Questions = { new Question { Name = "example.com" } }
            };
            var m2 = (Message)m1.Clone();
            CollectionAssert.AreEqual(m1.ToByteArray(), m2.ToByteArray());
        }

        [TestMethod]
        public void Clone_Typed()
        {
            var m1 = new Message
            {
                Questions = { new Question { Name = "example.com" } }
            };
            var m2 = m1.Clone<Message>();
            CollectionAssert.AreEqual(m1.ToByteArray(), m2.ToByteArray());
        }
    }
}
