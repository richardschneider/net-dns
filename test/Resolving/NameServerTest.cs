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
    public class NameServerTest
    {
        Catalog dotcom = new Catalog();
        Catalog dotorg = new Catalog();

        public NameServerTest()
        {
            dotcom.IncludeZone(new PresentationReader(new StringReader(CatalogTest.exampleDotComZoneText)));
            dotorg.IncludeZone(new PresentationReader(new StringReader(CatalogTest.exampleDotOrgZoneText)));
        }

        [TestMethod]
        public async Task Simple()
        {
            var resolver = new NameServer { Catalog = dotcom };
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
            var resolver = new NameServer { Catalog = dotcom };
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
            var resolver = new NameServer { Catalog = dotcom };
            var question = new Question { Name = "ns.example.com", Type = DnsType.MX };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NameError, response.Status);
        }

        [TestMethod]
        public async Task Missing_Class()
        {
            var resolver = new NameServer { Catalog = dotcom };
            var question = new Question { Name = "ns.example.com", Class = DnsClass.CH };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NameError, response.Status);
        }

        [TestMethod]
        public async Task AnyType()
        {
            var resolver = new NameServer { Catalog = dotcom };
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
            var resolver = new NameServer { Catalog = dotcom };
            var question = new Question { Name = "ns.example.com", Class = DnsClass.ANY, Type = DnsType.A };
            var response = await resolver.ResolveAsync(question);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsFalse(response.AA);
            Assert.AreEqual(1, response.Answers.Count);
        }

        [TestMethod]
        public async Task Alias()
        {
            var resolver = new NameServer { Catalog = dotcom };
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
            var resolver = new NameServer { Catalog = dotcom };
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
            var resolver = new NameServer { Catalog = dotcom };
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
            var resolver = new NameServer { Catalog = dotcom };
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
        public async Task MultipleQuestions_SomeQuestionNoAnswer_AnswerAny()
        {
            var resolver = new NameServer { Catalog = dotcom };
            var request = new Message();
            request.Questions.Add(new Question { Name = "unknown-name.com", Type = DnsType.AAAA });
            request.Questions.Add(new Question { Name = "unknown-name.example.com", Type = DnsType.AAAA });
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
            var resolver = new NameServer { Catalog = dotcom, AnswerAllQuestions = true};
            var request = new Message();
            request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.A });
            request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.AAAA });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(2, response.Answers.Count);
        }

        [TestMethod]
        public async Task MultipleQuestions_SomeQuestionNoAnswer_AnswerAll()
        {
            var resolver = new NameServer { Catalog = dotcom, AnswerAllQuestions = true };
            var request = new Message();
            request.Questions.Add(new Question { Name = "unknown-name.com", Type = DnsType.AAAA });
            request.Questions.Add(new Question { Name = "unknown-name.example.com", Type = DnsType.AAAA });
            request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.AAAA });
            request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.A });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(2, response.Answers.Count);
            Assert.AreEqual(3, response.AuthorityRecords.Count);
        }

        [TestMethod]
        public async Task AdditionalRecords_PTR_WithAddresses()
        {
            var resolver = new NameServer { Catalog = dotorg };
            var request = new Message();
            request.Questions.Add(new Question { Name = "x.example.org", Type = DnsType.PTR });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(1, response.Answers.Count);

            Assert.AreEqual(2, response.AdditionalRecords.Count);
            Assert.AreEqual(DnsType.A, response.AdditionalRecords[0].Type);
            Assert.AreEqual("ns1.example.org", response.AdditionalRecords[0].Name);
        }

        [TestMethod]
        public async Task AdditionalRecords_PTR_WithSRV()
        {
            var resolver = new NameServer { Catalog = dotorg };
            var request = new Message();
            request.Questions.Add(new Question { Name = "_http._tcp.example.org", Type = DnsType.PTR });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(1, response.Answers.Count);

            Assert.IsTrue(response.AdditionalRecords.Any(a => a.Type == DnsType.SRV));
            Assert.IsTrue(response.AdditionalRecords.Any(a => a.Type == DnsType.TXT));
            Assert.IsTrue(response.AdditionalRecords.Any(a => a.Type == DnsType.A));
        }

        [TestMethod]
        public async Task AdditionalRecords_NS()
        {
            var resolver = new NameServer { Catalog = dotorg };
            var request = new Message();
            request.Questions.Add(new Question { Name = "example.org", Type = DnsType.NS });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(2, response.Answers.Count);

            Assert.AreEqual(2, response.AdditionalRecords.Count);
            Assert.IsTrue(response.AdditionalRecords.All(r => r.Type == DnsType.A));
        }

        [TestMethod]
        public async Task AdditionalRecords_SOA()
        {
            var resolver = new NameServer { Catalog = dotorg };
            var request = new Message();
            request.Questions.Add(new Question { Name = "example.org", Type = DnsType.SOA });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(1, response.Answers.Count);

            Assert.AreEqual(2, response.AdditionalRecords.Count);
            Assert.AreEqual(DnsType.A, response.AdditionalRecords[0].Type);
            Assert.AreEqual("ns1.example.org", response.AdditionalRecords[0].Name);
        }

        [TestMethod]
        public async Task AdditionalRecords_SRV()
        {
            var resolver = new NameServer { Catalog = dotorg };
            var request = new Message();
            request.Questions.Add(new Question { Name = "a._http._tcp.example.org", Type = DnsType.SRV });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(1, response.Answers.Count);

            Assert.IsTrue(response.AdditionalRecords.OfType<TXTRecord>().Any());
            Assert.IsTrue(response.AdditionalRecords.OfType<ARecord>().Any());
        }

        [TestMethod]
        public async Task AdditionalRecords_A()
        {
            var resolver = new NameServer { Catalog = dotcom };
            var request = new Message();
            request.Questions.Add(new Question { Name = "example.com", Type = DnsType.A });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(1, response.Answers.Count);
            Assert.IsTrue(response.Answers.All(r => r.Type == DnsType.A));

            Assert.IsTrue(response.AdditionalRecords.Any(r => 
                r.Name == "example.com" && r.Type == DnsType.AAAA));
        }

        [TestMethod]
        public async Task AdditionalRecords_AAAA()
        {
            var resolver = new NameServer { Catalog = dotcom };
            var request = new Message();
            request.Questions.Add(new Question { Name = "example.com", Type = DnsType.AAAA });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(1, response.Answers.Count);
            Assert.IsTrue(response.Answers.All(r => r.Type == DnsType.AAAA));

            Assert.IsTrue(response.AdditionalRecords.Any(r =>
                r.Name == "example.com" && r.Type == DnsType.A));
        }

        [TestMethod]
        public async Task AdditionalRecords_NoDuplicates()
        {
            var resolver = new NameServer { Catalog = dotorg,  AnswerAllQuestions = true };
            var request = new Message();
            request.Questions.Add(new Question { Name = "example.org", Type = DnsType.NS });
            request.Questions.Add(new Question { Name = "ns1.example.org", Type = DnsType.A });
            request.Questions.Add(new Question { Name = "ns2.example.org", Type = DnsType.A });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.AreEqual(4, response.Answers.Count);

            Assert.AreEqual(0, response.AdditionalRecords.Count);
        }

        [TestMethod]
        public async Task EscapedDotDomainName()
        {
            var catalog = new Catalog
            {
                new ARecord
                {
                    Name = "a.b",
                    Address = IPAddress.Parse("127.0.0.2")
                },
                new ARecord
                {
                    Name = @"a\.b",
                    Address = IPAddress.Parse("127.0.0.3")
                }
            };
            var resolver = new NameServer { Catalog = catalog };

            var request = new Message();
            request.Questions.Add(new Question { Name = "a.b", Type = DnsType.A });
            var response = await resolver.ResolveAsync(request);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            var answer = response.Answers.OfType<ARecord>().First();
            Assert.AreEqual("127.0.0.2", answer.Address.ToString());

            request = new Message();
            request.Questions.Add(new Question { Name = @"a\.b", Type = DnsType.A });
            response = await resolver.ResolveAsync(request);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            answer = response.Answers.OfType<ARecord>().First();
            Assert.AreEqual("127.0.0.3", answer.Address.ToString());
        }

        [TestMethod]
        public async Task RoundTrip_EscapedDotDomainName()
        {
            var catalog = new Catalog
            {
                new ARecord
                {
                    Name = "a.b",
                    Address = IPAddress.Parse("127.0.0.2")
                },
                new ARecord
                {
                    Name = @"a\.b",
                    Address = IPAddress.Parse("127.0.0.3")
                }
            };
            var resolver = new NameServer { Catalog = catalog };

            var request = new Message();
            request.Questions.Add(new Question { Name = @"a\.b", Type = DnsType.A });
            var bin = request.ToByteArray();
            var r1 = new Message();
            r1.Read(bin);

            var response = await resolver.ResolveAsync(r1);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            var answer = response.Answers.OfType<ARecord>().First();
            Assert.AreEqual("127.0.0.3", answer.Address.ToString());

        }
    }
}
