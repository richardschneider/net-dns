using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Makaretu.Dns
{

    [TestClass]
    public class MessageValidationTest
    {
        [TestMethod]
        public async Task EnsureValidResponse()
        {
            var query = new Message
            {
                Id = 1234,
                Questions = { new Question { Name = "foo.bar", Type = DnsType.A } }
            };
            var response = query.CreateResponse();
            response.Answers.Add(AddressRecord.Create("foo.bar", IPAddress.Loopback));

            await query.EnsureValidResponseAsync(response, null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task EnsureValidResponse_Throws()
        {
            var query = new Message
            {
                Id = 1234,
                Questions = { new Question { Name = "foo.bar", Type = DnsType.A } }
            };
            var response = query.CreateResponse();
            response.Id = 4321;
            response.Answers.Add(AddressRecord.Create("foo.bar", IPAddress.Loopback));

            await query.EnsureValidResponseAsync(response, null);
        }


        [TestMethod]
        public async Task ValidateResponse()
        {
            var query = new Message
            {
                Id = 1234,
                Questions = { new Question { Name = "foo.bar", Type = DnsType.A } }
            };
            var response = query.CreateResponse();
            response.Answers.Add(AddressRecord.Create("foo.bar", IPAddress.Loopback));

            var failure = await query.ValidateResponseAsync(response, null);
            Assert.AreEqual(null, failure);
        }

        [TestMethod]
        public async Task ValidateResponse_WrongID()
        {
            var query = new Message
            {
                Id = 1234,
                Questions = { new Question { Name = "foo.bar", Type = DnsType.A } }
            };
            var response = query.CreateResponse();
            response.Id = 4321;
            response.Answers.Add(AddressRecord.Create("foo.bar", IPAddress.Loopback));

            var failure = await query.ValidateResponseAsync(response, null);
            Assert.IsNotNull(failure);
        }

        [TestMethod]
        public async Task ValidateResponse_QR()
        {
            var query = new Message
            {
                Id = 1234,
                Questions = { new Question { Name = "foo.bar", Type = DnsType.A } }
            };

            var failure = await query.ValidateResponseAsync(query, null);
            Assert.IsNotNull(failure);
        }

        [TestMethod]
        public async Task ValidateResponse_MissingAnswer()
        {
            var query = new Message
            {
                Id = 1234,
                Questions = { new Question { Name = "foo.bar", Type = DnsType.A } }
            };
            var response = query.CreateResponse();

            var failure = await query.ValidateResponseAsync(response, null);
            Assert.IsNotNull(failure);
        }
    }
}
