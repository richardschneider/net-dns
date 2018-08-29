using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class UpdateMessageTest
    {
        [TestMethod]
        public void Defaults()
        {
            var m = new UpdateMessage();
            Assert.AreEqual(0, m.AdditionalResources.Count);
            Assert.AreEqual(0, m.Id);
            Assert.AreEqual(false, m.IsResponse);
            Assert.AreEqual(true, m.IsUpdate);
            Assert.AreEqual(MessageOperation.Update, m.Opcode);
            Assert.AreEqual(0, m.Prerequisites.Count);
            Assert.AreEqual(false, m.QR);
            Assert.AreEqual(MessageStatus.NoError, m.Status);
            Assert.AreEqual(0, m.Updates.Count);
            Assert.AreEqual(0, m.Z);
            Assert.AreNotEqual(null, m.Zone);
            Assert.AreEqual(DnsType.SOA, m.Zone.Type, "must be SOA");
            Assert.AreEqual(DnsClass.IN, m.Zone.Class);
        }

        [TestMethod]
        public void Flags()
        {
            var expected = new UpdateMessage
            {
                Id = 1234,
                Zone = new Question { Name = "erehwon.org"},
                QR = true,
                Z = 0x7F,
                Status = MessageStatus.NotImplemented
            };
            var actual = new UpdateMessage();
            actual.Read(expected.ToByteArray());
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.QR, actual.QR);
            Assert.AreEqual(expected.Opcode, actual.Opcode);
            Assert.AreEqual(expected.Z, actual.Z);
            Assert.AreEqual(expected.Status, actual.Status);
            Assert.AreEqual(expected.Zone.Name, actual.Zone.Name);
            Assert.AreEqual(expected.Zone.Class, actual.Zone.Class);
            Assert.AreEqual(expected.Zone.Type, actual.Zone.Type);
        }

        [TestMethod]
        public void Response()
        {
            var update = new UpdateMessage { Id = 1234 };
            var response = update.CreateResponse();
            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(update.Id, response.Id);
            Assert.AreEqual(update.Opcode, response.Opcode);
        }

        [TestMethod]
        public void Roundtrip()
        {
            var expected = new UpdateMessage
            {
                Id = 1234
            };
            expected.Zone.Name = "emanon.org";
            expected.Prerequisites
                .MustExist("foo.emanon.org")
                .MustNotExist("bar.emanon.org");
            expected.Updates
                .AddResource(new ARecord { Name = "bar.emanon.org", Address = IPAddress.Parse("127.0.0.1") })
                .DeleteResource("foo.emanon.org");
            var actual = (UpdateMessage)new UpdateMessage().Read(expected.ToByteArray());
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.IsUpdate, actual.IsUpdate);
            Assert.AreEqual(expected.IsResponse, actual.IsResponse);
            Assert.AreEqual(expected.Opcode, actual.Opcode);
            Assert.AreEqual(expected.QR, actual.QR);
            Assert.AreEqual(expected.Status, actual.Status);
            Assert.AreEqual(expected.Zone.Name, actual.Zone.Name);
            Assert.AreEqual(expected.Zone.Class, actual.Zone.Class);
            Assert.AreEqual(expected.Zone.Type, actual.Zone.Type);
            Assert.IsTrue(expected.Prerequisites.SequenceEqual(actual.Prerequisites));
            Assert.IsTrue(expected.Updates.SequenceEqual(actual.Updates));
            Assert.IsTrue(expected.AdditionalResources.SequenceEqual(actual.AdditionalResources));
        }
    }
}
