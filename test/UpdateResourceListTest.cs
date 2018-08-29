using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class UpdateResourceListTest
    {
        [TestMethod]
        public void AddResource()
        {
            var rr = new ARecord
            {
                Name = "local",
                Class = DnsClass.IN,
                Address = IPAddress.Parse("127.0.0.0")
            };
            var updates = new UpdateResourceList()
                .AddResource(rr);
            var p = updates.First() as ResourceRecord;
            Assert.IsNotNull(p);
            Assert.AreEqual(rr.Class, p.Class);
            Assert.AreEqual(rr.Name, p.Name);
            Assert.AreEqual(rr.TTL, p.TTL);
            Assert.AreEqual(rr.Type, p.Type);
            Assert.AreEqual(rr.GetDataLength(), p.GetDataLength());
            Assert.IsTrue(rr.GetData().SequenceEqual(p.GetData()));
        }

        [TestMethod]
        public void DeleteResource_Name()
        {
            var updates = new UpdateResourceList()
                .DeleteResource("www.example.org");
            var p = updates.First() as ResourceRecord;
            Assert.IsNotNull(p);
            Assert.AreEqual(DnsClass.ANY, p.Class);
            Assert.AreEqual("www.example.org", p.Name);
            Assert.AreEqual(TimeSpan.Zero, p.TTL);
            Assert.AreEqual(DnsType.ANY, p.Type);
            Assert.AreEqual(0, p.GetDataLength());
        }

        [TestMethod]
        public void DeleteResource_Name_Type()
        {
            var updates = new UpdateResourceList()
                .DeleteResource("www.example.org", DnsType.A);
            var p = updates.First() as ResourceRecord;
            Assert.IsNotNull(p);
            Assert.AreEqual(DnsClass.ANY, p.Class);
            Assert.AreEqual("www.example.org", p.Name);
            Assert.AreEqual(TimeSpan.Zero, p.TTL);
            Assert.AreEqual(DnsType.A, p.Type);
            Assert.AreEqual(0, p.GetDataLength());
        }

        [TestMethod]
        public void DeleteResource_Name_Typename()
        {
            var updates = new UpdateResourceList()
                .DeleteResource<ARecord>("www.example.org");
            var p = updates.First() as ResourceRecord;
            Assert.IsNotNull(p);
            Assert.AreEqual(DnsClass.ANY, p.Class);
            Assert.AreEqual("www.example.org", p.Name);
            Assert.AreEqual(TimeSpan.Zero, p.TTL);
            Assert.AreEqual(DnsType.A, p.Type);
            Assert.AreEqual(0, p.GetDataLength());
        }

        [TestMethod]
        public void DeleteResource()
        {
            var rr = new ARecord
            {
                Name = "local",
                Class = DnsClass.IN,
                Address = IPAddress.Parse("127.0.0.0")
            };
            var updates = new UpdateResourceList()
                .DeleteResource(rr);
            var p = updates.First() as ResourceRecord;
            Assert.IsNotNull(p);
            Assert.AreEqual(DnsClass.None, p.Class);
            Assert.AreEqual(rr.Name, p.Name);
            Assert.AreEqual(TimeSpan.Zero, p.TTL);
            Assert.AreEqual(rr.Type, p.Type);
            Assert.AreEqual(rr.GetDataLength(), p.GetDataLength());
            Assert.IsTrue(rr.GetData().SequenceEqual(p.GetData()));
        }

    }
}
