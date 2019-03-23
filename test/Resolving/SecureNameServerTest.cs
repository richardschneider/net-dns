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
    public class SecureNameServerTest
    {
        Catalog example = new Catalog();

        public SecureNameServerTest()
        {
            example.IncludeZone(new PresentationReader(new StringReader(SecureCatalogTest.examplZoneText)));
        }

        [TestMethod]
        public async Task SupportDnssec()
        {
            var resolver = new NameServer { Catalog = example };
            var request = new Message();
            request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });

            var response = await resolver.ResolveAsync(request);
            Assert.IsFalse(response.DO);

            request.UseDnsSecurity();
            response = await resolver.ResolveAsync(request);
            Assert.IsTrue(response.DO);
        }

        [TestMethod]
        public async Task QueryWithoutDo()
        {
            var resolver = new NameServer { Catalog = example };
            var request = new Message();
            request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.IsFalse(response.DO);
        }

        [TestMethod]
        public async Task QueryWithDo()
        {
            var resolver = new NameServer { Catalog = example };
            var request = new Message().UseDnsSecurity();
            request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.IsTrue(response.DO);
        }

        [TestMethod]
        public async Task SecureQueryHasSignature()
        {
            // See https://tools.ietf.org/html/rfc4035#appendix-B.1

            var resolver = new NameServer { Catalog = example };
            var request = new Message().UseDnsSecurity();
            request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });
            var response = await resolver.ResolveAsync(request);

            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(MessageStatus.NoError, response.Status);
            Assert.IsTrue(response.AA);
            Assert.IsTrue(response.DO);

            Assert.AreEqual(2, response.Answers.Count);
            Assert.AreEqual(1, response.Answers.OfType<MXRecord>().Count());
            Assert.AreEqual(1, response.Answers.OfType<RRSIGRecord>().Count());

            Assert.AreEqual(3, response.AuthorityRecords.Count);
            Assert.AreEqual(2, response.AuthorityRecords.OfType<NSRecord>().Count());
            Assert.AreEqual(1, response.AuthorityRecords.OfType<RRSIGRecord>().Count());

        }
    }
}
