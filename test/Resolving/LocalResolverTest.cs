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

        [TestMethod]
        public async Task AnyClass()
        {
            var resolver = new LocalResolver { Catalog = dotcom };
            var question = new Question { Name = "ns.example.com", Class = Class.ANY, Type = DnsType.A };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsFalse(response.AA);
            Assert.AreEqual(1, response.Answers.Count);
        }

        [TestMethod]
        public async Task Alias()
        {
            var resolver = new LocalResolver { Catalog = dotcom };
            var question = new Question { Name = "www.example.com", Type = DnsType.A };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(MessageStatus.NoError, response.Status);

            Assert.AreEqual(2, response.Answers.Count);
            Assert.AreEqual(DnsType.CNAME, response.Answers[0].Type);
            Assert.AreEqual(DnsType.A, response.Answers[1].Type);
        }

        [TestMethod]
        public async Task Alias_BadZoneTarget()
        {
            var resolver = new LocalResolver { Catalog = dotcom };
            var ftp = new Node { Name = "ftp.example.com", Authoritative = true };
            ftp.Resources.Add(new CNAMERecord { Name = ftp.Name, Target = "ftp-server.example.com" });
            resolver.Catalog.TryAdd(ftp.Name, ftp);
            var question = new Question { Name = "ftp.example.com", Type = DnsType.A };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(MessageStatus.NameError, response.Status);

            Assert.AreEqual(1, response.Answers.Count);
            Assert.AreEqual(DnsType.CNAME, response.Answers[0].Type);

            Assert.AreEqual(1, response.AuthorityRecords.Count);
            var authority = response.AuthorityRecords.OfType<SOARecord>().First();
            Assert.AreEqual("example.com", authority.Name);
        }

        [TestMethod]
        public async Task Alias_BadInterZoneTarget()
        {
            var resolver = new LocalResolver { Catalog = dotcom };
            var bad = new Node { Name = "bad.example.com", Authoritative = true };
            bad.Resources.Add(new CNAMERecord { Name = bad.Name, Target = "somewhere-else.org" });
            resolver.Catalog.TryAdd(bad.Name, bad);
            var question = new Question { Name = "bad.example.com", Type = DnsType.A };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(MessageStatus.NameError, response.Status);

            Assert.AreEqual(1, response.Answers.Count);
            Assert.AreEqual(DnsType.CNAME, response.Answers[0].Type);

            Assert.AreEqual(0, response.AuthorityRecords.Count);
        }

        [TestMethod]
        public async Task MultipleQuestions_AnswerAny()
        {
            var resolver = new LocalResolver { Catalog = dotcom };
            var request = new Message();
            request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.A });
            request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.AAAA });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(1, response.Answers.Count);
        }

        [TestMethod]
        public async Task MultipleQuestions_AnswerAll()
        {
            var resolver = new LocalResolver { Catalog = dotcom, AnswerAllQuestions = true};
            var request = new Message();
            request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.A });
            request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.AAAA });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(2, response.Answers.Count);
        }
    }
}
