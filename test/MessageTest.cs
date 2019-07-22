using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class MessageTest
    {
        /// <summary>
        ///   From https://en.wikipedia.org/wiki/Multicast_DNS
        /// </summary>
        [TestMethod]
        public void DecodeQuery()
        {
            var bytes = new byte[]
            {
                0x00, 0x00,             // Transaction ID
                0x00, 0x00,             // Flags
                0x00, 0x01,             // Number of questions
                0x00, 0x00,             // Number of answers
                0x00, 0x00,             // Number of authority resource records
                0x00, 0x00,             // Number of additional resource records
                0x07, 0x61, 0x70, 0x70, 0x6c, 0x65, 0x74, 0x76, // "appletv"
                0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, // "local"
                0x00,                   // Terminator
                0x00, 0x01,             // Type (A record)
                0x00, 0x01              // Class
            };
            var msg = new Message();
            msg.Read(bytes, 0, bytes.Length);
            Assert.AreEqual(0, msg.Id);
            Assert.AreEqual(1, msg.Questions.Count);
            Assert.AreEqual(0, msg.Answers.Count);
            Assert.AreEqual(0, msg.AuthorityRecords.Count);
            Assert.AreEqual(0, msg.AdditionalRecords.Count);
            var question = msg.Questions.First();
            Assert.AreEqual("appletv.local", question.Name);
            Assert.AreEqual(DnsType.A, question.Type);
            Assert.AreEqual(DnsClass.IN, question.Class);
        }

        /// <summary>
        ///   From https://en.wikipedia.org/wiki/Multicast_DNS
        /// </summary>
        [TestMethod]
        public void DecodeResponse()
        {
            var bytes = new byte[]
            {
                0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x07, 0x61, 0x70, 0x70,
                0x6c, 0x65, 0x74, 0x76, 0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x00, 0x00, 0x01, 0x80, 0x01, 0x00,
                0x00, 0x78, 0x00, 0x00, 0x04, 0x99, 0x6d, 0x07, 0x5a, 0xc0, 0x0c, 0x00, 0x1c, 0x80, 0x01, 0x00,
                0x00, 0x78, 0x00, 0x00, 0x10, 0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x23, 0x32,
                0xff, 0xfe, 0xb1, 0x21, 0x52, 0xc0, 0x0c, 0x00, 0x2f, 0x80, 0x01, 0x00, 0x00, 0x78, 0x00, 0x00,
                0x08, 0xc0, 0x0c, 0x00, 0x04, 0x40, 0x00, 0x00, 0x08
            };
            var msg = new Message();
            msg.Read(bytes, 0, bytes.Length);

            Assert.IsTrue(msg.IsResponse);
            Assert.IsTrue(msg.AA);
            Assert.AreEqual(0, msg.Questions.Count);
            Assert.AreEqual(1, msg.Answers.Count);
            Assert.AreEqual(0, msg.AuthorityRecords.Count);
            Assert.AreEqual(2, msg.AdditionalRecords.Count);

            Assert.AreEqual("appletv.local", msg.Answers[0].Name);
            Assert.AreEqual(DnsType.A, msg.Answers[0].Type);
            Assert.AreEqual(0x8001, (ushort)msg.Answers[0].Class);
            Assert.AreEqual(TimeSpan.FromSeconds(30720), msg.Answers[0].TTL);
            Assert.IsInstanceOfType(msg.Answers[0], typeof(ARecord));
            Assert.AreEqual(IPAddress.Parse("153.109.7.90"), ((ARecord)msg.Answers[0]).Address);

            var aaaa = (AAAARecord)msg.AdditionalRecords[0];
            Assert.AreEqual("appletv.local", aaaa.Name);
            Assert.AreEqual(DnsType.AAAA, aaaa.Type);
            Assert.AreEqual(0x8001, (ushort)aaaa.Class);
            Assert.AreEqual(TimeSpan.FromSeconds(30720), aaaa.TTL);
            Assert.AreEqual(IPAddress.Parse("fe80::223:32ff:feb1:2152"), aaaa.Address);

            var nsec = (NSECRecord)msg.AdditionalRecords[1];
            Assert.AreEqual("appletv.local", nsec.Name);
            Assert.AreEqual(DnsType.NSEC, nsec.Type);
            Assert.AreEqual(0x8001, (ushort)nsec.Class);
            Assert.AreEqual(TimeSpan.FromSeconds(30720), nsec.TTL);
            Assert.AreEqual("appletv.local", nsec.NextOwnerName);
        }

        [TestMethod]
        public void Flags()
        {
            var expected = new Message
            {
                QR = true,
                Opcode = MessageOperation.Status,
                AA = true,
                TC = true,
                RD = true,
                RA = true,
                Z = 1,
                AD = true,
                CD = true,
                Status = MessageStatus.Refused
            };
            var actual = new Message();
            actual.Read(expected.ToByteArray());
            Assert.AreEqual(expected.QR, actual.QR);
            Assert.AreEqual(expected.Opcode, actual.Opcode);
            Assert.AreEqual(expected.AA, actual.AA);
            Assert.AreEqual(expected.TC, actual.TC);
            Assert.AreEqual(expected.RD, actual.RD);
            Assert.AreEqual(expected.RA, actual.RA);
            Assert.AreEqual(expected.Z, actual.Z);
            Assert.AreEqual(expected.AD, actual.AD);
            Assert.AreEqual(expected.CD, actual.CD);
            Assert.AreEqual(expected.Status, actual.Status);
        }

        [TestMethod]
        public void Response()
        {
            var query = new Message { Id = 1234, Opcode = MessageOperation.InverseQuery };
            query.Questions.Add(new Question { Name = "foo.org", Type = DnsType.A });
            var response = query.CreateResponse();
            Assert.IsTrue(response.IsResponse);
            Assert.AreEqual(query.Id, response.Id);
            Assert.AreEqual(query.Opcode, response.Opcode);
            Assert.AreEqual(1, response.Questions.Count);
            Assert.AreEqual(query.Questions[0], response.Questions[0]);
        }

        [TestMethod]
        public void Roundtrip()
        {
            var expected = new Message
            {
                AA = true,
                QR = true,
                Id = 1234
            };
            expected.Questions.Add(new Question { Name = "emanon.org" });
            expected.Answers.Add(new ARecord { Name = "emanon.org", Address = IPAddress.Parse("127.0.0.1") });
            expected.AuthorityRecords.Add(new SOARecord
            {
                Name = "emanon.org",
                PrimaryName = "erehwon",
                Mailbox = "hostmaster.emanon.org"
            });
            expected.AdditionalRecords.Add(new ARecord { Name = "erehwon", Address = IPAddress.Parse("127.0.0.1") });
            var actual = (Message)new Message().Read(expected.ToByteArray());
            Assert.AreEqual(expected.AA, actual.AA);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.IsQuery, actual.IsQuery);
            Assert.AreEqual(expected.IsResponse, actual.IsResponse);
            Assert.AreEqual(1, actual.Questions.Count);
            Assert.AreEqual(1, actual.Answers.Count);
            Assert.AreEqual(1, actual.AuthorityRecords.Count);
            Assert.AreEqual(1, actual.AdditionalRecords.Count);
        }

        [TestMethod]
        public void ExtendedOpcode()
        {
            var expected = new Message { Opcode = (MessageOperation)0xfff };
            Assert.AreEqual((MessageOperation)0xfff, expected.Opcode);
            Assert.AreEqual(1, expected.AdditionalRecords.OfType<OPTRecord>().Count());

            var actual = (Message)new Message().Read(expected.ToByteArray());
            Assert.AreEqual(expected.Opcode, actual.Opcode);
        }

        [TestMethod]
        public void Issue_11()
        {
            var bytes = Convert.FromBase64String("EjSBgAABAAEAAAAABGlwZnMCaW8AABAAAcAMABAAAQAAADwAPTxkbnNsaW5rPS9pcGZzL1FtWU5RSm9LR05IVHBQeENCUGg5S2tEcGFFeGdkMmR1TWEzYUY2eXRNcEhkYW8=");
            var msg = (Message)(new Message().Read(bytes));
        }

        [TestMethod]
        public void Issue_12()
        {
            var bytes = Convert.FromBase64String("AASBgAABAAQAAAABA3d3dwxvcGluaW9uc3RhZ2UDY29tAAABAAHADAAFAAEAAAA8AALAEMAQAAEAAQAAADwABCLAkCrANAABAAEAAAA8AAQ0NgUNwDQAAQABAAAAPAAEaxUAqgAAKQYAAAAAAAFlAAwBYQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var msg = (Message)(new Message().Read(bytes));
        }

        [TestMethod]
        public void Truncation_NotRequired()
        {
            var msg = new Message();
            var originalLength = msg.Length();
            msg.Truncate(int.MaxValue);
            Assert.AreEqual(originalLength, msg.Length());
            Assert.IsFalse(msg.TC);
        }

        [TestMethod]
        public void Truncation_Fails()
        {
            var msg = new Message();
            var originalLength = msg.Length();
            msg.Truncate(originalLength - 1);
            Assert.AreEqual(originalLength, msg.Length());
            Assert.IsTrue(msg.TC);
        }

        [TestMethod]
        public void Truncation_AdditionalRecords()
        {
            var msg = new Message();
            msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
            msg.AuthorityRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
            var originalLength = msg.Length();
            msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
            msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
            msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));

            msg.Truncate(originalLength);
            Assert.AreEqual(originalLength, msg.Length());
            Assert.AreEqual(1, msg.AdditionalRecords.Count);
            Assert.AreEqual(1, msg.AuthorityRecords.Count);
            Assert.IsFalse(msg.TC);
        }

        [TestMethod]
        public void AuthorityRecords()
        {
            var msg = new Message();
            msg.AuthorityRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
            var originalLength = msg.Length();
            msg.AuthorityRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
            msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
            msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));

            msg.Truncate(originalLength);
            Assert.AreEqual(originalLength, msg.Length());
            Assert.AreEqual(0, msg.AdditionalRecords.Count);
            Assert.AreEqual(1, msg.AuthorityRecords.Count);
            Assert.IsFalse(msg.TC);
        }

        [TestMethod]
        public void UseDnsSecurity()
        {
            var expected = new Message().UseDnsSecurity();
            var opt = expected.AdditionalRecords.OfType<OPTRecord>().Single();
            Assert.IsTrue(opt.DO, "dnssec ok");
        }

        [TestMethod]
        public void UseDnsSecurity_OPT_Exists()
        {
            var expected = new Message();
            expected.AdditionalRecords.Add(new OPTRecord());
            expected.UseDnsSecurity();
            var opt = expected.AdditionalRecords.OfType<OPTRecord>().Single();
            Assert.IsTrue(opt.DO, "dnssec ok");
        }

        [TestMethod]
        public void Dnssec_Bit()
        {
            var message = new Message();
            Assert.IsFalse(message.DO);
            Assert.AreEqual(0, message.AdditionalRecords.OfType<OPTRecord>().Count());

            message.DO = false;
            Assert.IsFalse(message.DO);
            Assert.AreEqual(1, message.AdditionalRecords.OfType<OPTRecord>().Count());

            message.DO = true;
            Assert.IsTrue(message.DO);
            Assert.AreEqual(1, message.AdditionalRecords.OfType<OPTRecord>().Count());
        }

        [TestMethod]
        public void Stringify()
        {
            var m = new Message
            {
                AA = true,
                QR = true,
                Id = 1234
            };
            m.Questions.Add(new Question { Name = "emanon.org", Type = DnsType.A });
            m.Answers.Add(new ARecord { Name = "emanon.org", Address = IPAddress.Parse("127.0.0.1") });
            m.AuthorityRecords.Add(new SOARecord
            {
                Name = "emanon.org",
                PrimaryName = "erehwon",
                Mailbox = "hostmaster.emanon.org"
            });

            var text = m.ToString();
            Console.WriteLine(text);
        }

        [TestMethod]
        public void Stringify_Edns()
        {
            var sample = "AH6FDwEAAAEAAAAAAAEEaXBmcwJpbwAAEAABAAApBQAAAAAAAFoACwAC1MAADABQ8bbi5IwN3llzr84N11j2dG7+7lE5aBzanfc1yvO3LcgvS0TuT3Xvz6yVWcVBa8YnFwehfSyT6YiaCEaV2BNlvIIG3YwUCCX4Dh6kpA9WmDI=";
            var buffer1 = Convert.FromBase64String(sample);
            var buffer2 = new byte[buffer1.Length - 2];
            Array.Copy(buffer1, 2, buffer2, 0, buffer2.Length);
            var m = new Message();
            m.Read(buffer2);

            var text = m.ToString();
            Console.WriteLine(text);
        }

        [TestMethod]
        public void AppleMessage()
        {
            // A MDNS query from an Apple Host.  It contains a UTF8 domain name
            // and an EDNS OPT-4 option.
            var sample = "AAAAAAAGAAAAAAABCF9ob21la2l0BF90Y3AFbG9jYWwAAAyAAQ9fY29tcGFuaW9uLWxpbmvAFQAMgAEIX2FpcnBsYXnAFQAMgAEFX3Jhb3DAFQAMgAEbQ2hyaXN0b3BoZXLigJlzIE1hY0Jvb2sgUHJvwCUAEIABDF9zbGVlcC1wcm94eQRfdWRwwBoADAABAAApBaAAABGUABIABAAOAJB6e4qbc5l4e4qbc5k=";
            var buffer1 = Convert.FromBase64String(sample);
            var m = new Message();
            m.Read(buffer1);

            Assert.AreEqual("Christopher’s MacBook Pro", m.Questions[4].Name.Labels[0]);
            Assert.AreEqual("_homekit._tcp.local CLASS32769 PTR", m.Questions[0].ToString());
        }
    }

}

