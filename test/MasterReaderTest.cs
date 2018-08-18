using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class MasterReaderTest
    {
        [TestMethod]
        public void ReadString()
        {
            var reader = new MasterReader(new StringReader("  alpha   beta   omega"));
            Assert.AreEqual("alpha", reader.ReadString());
            Assert.AreEqual("beta", reader.ReadString());
            Assert.AreEqual("omega", reader.ReadString());
        }

        [TestMethod]
        public void ReadQuotedStrings()
        {
            var reader = new MasterReader(new StringReader("  \"a b c\"  \"x y z\""));
            Assert.AreEqual("a b c", reader.ReadString());
            Assert.AreEqual("x y z", reader.ReadString());
        }

        [TestMethod]
        public void ReadEscapedStrings()
        {
            var reader = new MasterReader(new StringReader("  alpha\\ beta   omega"));
            Assert.AreEqual("alpha beta", reader.ReadString());
            Assert.AreEqual("omega", reader.ReadString());
        }

        [TestMethod]
        public void ReadResource()
        {
            var reader = new MasterReader(new StringReader("me A 127.0.0.1"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("me", resource.Name);
            Assert.AreEqual(Class.IN, resource.Class);
            Assert.AreEqual(DnsType.A, resource.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(ARecord));
        }

        [TestMethod]
        public void ReadResourceWithClassAndTTL()
        {
            var reader = new MasterReader(new StringReader("me CH 63 A 127.0.0.1"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("me", resource.Name);
            Assert.AreEqual(Class.CH, resource.Class);
            Assert.AreEqual(DnsType.A, resource.Type);
            Assert.AreEqual(TimeSpan.FromSeconds(63), resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(ARecord));
        }

        [TestMethod]
        public void ReadResourceWithUnknownClass()
        {
            var reader = new MasterReader(new StringReader("me CLASS1234 A 127.0.0.1"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("me", resource.Name);
            Assert.AreEqual(1234, (int)resource.Class);
            Assert.AreEqual(DnsType.A, resource.Type);
            Assert.IsInstanceOfType(resource, typeof(ARecord));
        }

        [TestMethod]
        public void ReadResourceWithUnknownType()
        {
            var reader = new MasterReader(new StringReader("me CH TYPE1234 \\# 0"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("me", resource.Name);
            Assert.AreEqual(Class.CH, resource.Class);
            Assert.AreEqual(1234, (int)resource.Type);
            Assert.IsInstanceOfType(resource, typeof(UnknownRecord));
        }

        [TestMethod]
        public void ReadResourceWithComment()
        {
            var reader = new MasterReader(new StringReader("; comment\r\nme A 127.0.0.1"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("me", resource.Name);
            Assert.AreEqual(Class.IN, resource.Class);
            Assert.AreEqual(DnsType.A, resource.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(ARecord));
        }

        [TestMethod]
        public void ReadResourceWithOrigin()
        {
            var text = @"
$ORIGIN emanon.org. ; no such place\r\n
@ PTR localhost
";
            var reader = new MasterReader(new StringReader(text));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("emanon.org", resource.Name);
            Assert.AreEqual(Class.IN, resource.Class);
            Assert.AreEqual(DnsType.PTR, resource.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(PTRRecord));
        }

        [TestMethod]
        public void ReadResourceWithTTL()
        {
            var text = @"
$TTL 120 ; 2 minutes\r\n
emanon.org PTR localhost
";
            var reader = new MasterReader(new StringReader(text));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("emanon.org", resource.Name);
            Assert.AreEqual(Class.IN, resource.Class);
            Assert.AreEqual(DnsType.PTR, resource.Type);
            Assert.AreEqual(TimeSpan.FromMinutes(2), resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(PTRRecord));
        }

        [TestMethod]
        public void ReadResourceWithPreviousDomain()
        {
            var text = @"
emanon.org A 127.0.0.1
           AAAA ::1
";
            var reader = new MasterReader(new StringReader(text));
            var a = reader.ReadResourceRecord();
            Assert.AreEqual("emanon.org", a.Name);
            Assert.AreEqual(Class.IN, a.Class);
            Assert.AreEqual(DnsType.A, a.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, a.TTL);
            Assert.IsInstanceOfType(a, typeof(ARecord));

            var aaaa = reader.ReadResourceRecord();
            Assert.AreEqual("emanon.org", aaaa.Name);
            Assert.AreEqual(Class.IN, aaaa.Class);
            Assert.AreEqual(DnsType.AAAA, aaaa.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, aaaa.TTL);
            Assert.IsInstanceOfType(aaaa, typeof(AAAARecord));
        }

        [TestMethod]
        public void ReadZoneFile()
        {
            var text = @"
 $ORIGIN example.com.     ; designates the start of this zone file in the namespace
 $TTL 3600                  ; default expiration time of all resource records without their own TTL value
 ; example.com.  IN  SOA   ns.example.com. username.example.com. ( 2007120710 1d 2h 4w 1h )
 example.com.  IN  SOA   ns.example.com. username.example.com. ( 2007120710 1 2 4 1 )
 example.com.  IN  NS    ns                    ; ns.example.com is a nameserver for example.com
 example.com.  IN  NS    ns.somewhere.example. ; ns.somewhere.example is a backup nameserver for example.com
 example.com.  IN  MX    10 mail.example.com.  ; mail.example.com is the mailserver for example.com
 @             IN  MX    20 mail2.example.com. ; equivalent to above line, '@' represents zone origin
 @             IN  MX    50 mail3              ; equivalent to above line, but using a relative host name
 example.com.  IN  A     192.0.2.1             ; IPv4 address for example.com
               IN  AAAA  2001:db8:10::1        ; IPv6 address for example.com
 ns            IN  A     192.0.2.2             ; IPv4 address for ns.example.com
               IN  AAAA  2001:db8:10::2        ; IPv6 address for ns.example.com
 www           IN  CNAME example.com.          ; www.example.com is an alias for example.com
 wwwtest       IN  CNAME www                   ; wwwtest.example.com is another alias for www.example.com
 mail          IN  A     192.0.2.3             ; IPv4 address for mail.example.com
 mail2         IN  A     192.0.2.4             ; IPv4 address for mail2.example.com
 mail3         IN  A     192.0.2.5             ; IPv4 address for mail3.example.com
";
            var reader = new MasterReader(new StringReader(text));
            var resources = new List<ResourceRecord>();
            while (true)
            {
                var r = reader.ReadResourceRecord();
                if (r == null)
                {
                    break;
                }
                resources.Add(r);
            }
            Assert.AreEqual(15, resources.Count);
        }

        [TestMethod]
        public void ReadResourceData()
        {
            var reader = new MasterReader(new StringReader("\\# 0"));
            var rdata = reader.ReadResourceData();
            Assert.AreEqual(0, rdata.Length);

            reader = new MasterReader(new StringReader("\\# 3 abcdef"));
            rdata = reader.ReadResourceData();
            CollectionAssert.AreEqual(new byte[] { 0xab, 0xcd, 0xef }, rdata);

            reader = new MasterReader(new StringReader("\\# 3 ab cd ef"));
            rdata = reader.ReadResourceData();
            CollectionAssert.AreEqual(new byte[] { 0xab, 0xcd, 0xef }, rdata);

            reader = new MasterReader(new StringReader("\\# 3 abcd (\r\n  ef )"));
            rdata = reader.ReadResourceData();
            CollectionAssert.AreEqual(new byte[] { 0xab, 0xcd, 0xef }, rdata);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ReadResourceData_MissingLeadin()
        {
            var reader = new MasterReader(new StringReader("0"));
            var _ = reader.ReadResourceData();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ReadResourceData_BadHex_BadDigit()
        {
            var reader = new MasterReader(new StringReader("\\# 3 ab cd ez"));
            var _ = reader.ReadResourceData();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ReadResourceData_BadHex_NotEven()
        {
            var reader = new MasterReader(new StringReader("\\# 3 ab cd e"));
            var _ = reader.ReadResourceData();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ReadResourceData_BadHex_TooFew()
        {
            var reader = new MasterReader(new StringReader("\\# 3 abcd"));
            var _ = reader.ReadResourceData();
        }
    }
}
