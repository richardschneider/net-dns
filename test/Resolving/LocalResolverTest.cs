using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Makaretu.Dns.Resolving
{

    [TestClass]
    public class LocalResolverTest
    {
        Catalog dotcom = new Catalog();

        public LocalResolverTest()
        {
            dotcom.IncludeZone(new MasterReader(new StringReader(CatalogTest.exampleDotComZoneText)));
        }

        [TestMethod]
        public async Task Simple()
        {
            var resolver = new LocalResolver { Catalog = dotcom };
            var question = new Question { Name = "ns.example.com", Type = DnsType.A };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(1, response.Answers.Count);
            Assert.AreEqual(DnsType.A, response.Answers[0].Type);
        }

        [TestMethod]
        public async Task Missing_Name()
        {
            var resolver = new LocalResolver { Catalog = dotcom };
            var question = new Question { Name = "foo.bar.example.com", Type = DnsType.A };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NameError, response.Status);

            Assert.IsTrue(response.AuthorityRecords.Count > 0);
            var authority = response.AuthorityRecords.OfType<SOARecord>().First();
            Assert.AreEqual("example.com", authority.Name);
        }

        [TestMethod]
        public async Task Missing_Type()
        {
            var resolver = new LocalResolver { Catalog = dotcom };
            var question = new Question { Name = "ns.example.com", Type = DnsType.MX };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NameError, response.Status);
        }

        [TestMethod]
        public async Task Missing_Class()
        {
            var resolver = new LocalResolver { Catalog = dotcom };
            var question = new Question { Name = "ns.example.com", Class = Class.CH };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NameError, response.Status);
        }

        [TestMethod]
        public async Task AnyType()
        {
            var resolver = new LocalResolver { Catalog = dotcom };
            var question = new Question { Name = "ns.example.com", Type = DnsType.ANY };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(2, response.Answers.Count);
        }
    }
}
