using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Makaretu.Dns
{

    [TestClass]
    public class PresentationReaderTest
    {
        [TestMethod]
        public void ReadString()
        {
            var reader = new PresentationReader(new StringReader("  alpha   beta   omega"));
            Assert.AreEqual("alpha", reader.ReadString());
            Assert.AreEqual("beta", reader.ReadString());
            Assert.AreEqual("omega", reader.ReadString());
        }

        [TestMethod]
        public void ReadQuotedStrings()
        {
            var reader = new PresentationReader(new StringReader("  \"a b c\"  \"x y z\""));
            Assert.AreEqual("a b c", reader.ReadString());
            Assert.AreEqual("x y z", reader.ReadString());
        }

        [TestMethod]
        public void ReadEscapedStrings()
        {
            var reader = new PresentationReader(new StringReader("  alpha\\ beta   omega"));
            Assert.AreEqual("alpha beta", reader.ReadString());
            Assert.AreEqual("omega", reader.ReadString());
        }

        [TestMethod]
        public void ReadDecimalEscapedString()
        {
            var reader = new PresentationReader(new StringReader("a\\098c"));
            Assert.AreEqual("abc", reader.ReadString());
        }

        [TestMethod]
        public void ReadInvalidDecimalEscapedString()
        {
            var reader = new PresentationReader(new StringReader("a\\256c"));
            ExceptionAssert.Throws<FormatException>(() => reader.ReadString());
        }

        [TestMethod]
        public void ReadResource()
        {
            var reader = new PresentationReader(new StringReader("me A 127.0.0.1"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("me", resource.Name);
            Assert.AreEqual(DnsClass.IN, resource.Class);
            Assert.AreEqual(DnsType.A, resource.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(ARecord));
        }

        [TestMethod]
        public void ReadResourceWithNameOfType()
        {
            var reader = new PresentationReader(new StringReader("A A 127.0.0.1"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("A", resource.Name);
            Assert.AreEqual(DnsClass.IN, resource.Class);
            Assert.AreEqual(DnsType.A, resource.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(ARecord));
        }

        [TestMethod]
        public void ReadResourceWithNameOfClass()
        {
            var reader = new PresentationReader(new StringReader("CH A 127.0.0.1"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("CH", resource.Name);
            Assert.AreEqual(DnsClass.IN, resource.Class);
            Assert.AreEqual(DnsType.A, resource.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(ARecord));
        }

        [TestMethod]
        public void ReadResourceWithClassAndTTL()
        {
            var reader = new PresentationReader(new StringReader("me CH 63 A 127.0.0.1"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("me", resource.Name);
            Assert.AreEqual(DnsClass.CH, resource.Class);
            Assert.AreEqual(DnsType.A, resource.Type);
            Assert.AreEqual(TimeSpan.FromSeconds(63), resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(ARecord));
        }

        [TestMethod]
        public void ReadResourceWithUnknownClass()
        {
            var reader = new PresentationReader(new StringReader("me CLASS1234 A 127.0.0.1"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("me", resource.Name);
            Assert.AreEqual(1234, (int)resource.Class);
            Assert.AreEqual(DnsType.A, resource.Type);
            Assert.IsInstanceOfType(resource, typeof(ARecord));
        }

        [TestMethod]
        public void ReadResourceWithUnknownType()
        {
            var reader = new PresentationReader(new StringReader("me CH TYPE1234 \\# 0"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("me", resource.Name);
            Assert.AreEqual(DnsClass.CH, resource.Class);
            Assert.AreEqual(1234, (int)resource.Type);
            Assert.IsInstanceOfType(resource, typeof(UnknownRecord));
        }

        [TestMethod]
        public void ReadResourceMissingName()
        {
            var reader = new PresentationReader(new StringReader("  NS ns1"));
            ExceptionAssert.Throws<InvalidDataException>(() => reader.ReadResourceRecord());
        }

        [TestMethod]
        public void ReadResourceWithComment()
        {
            var reader = new PresentationReader(new StringReader("; comment\r\nme A 127.0.0.1"));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("me", resource.Name);
            Assert.AreEqual(DnsClass.IN, resource.Class);
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
            var reader = new PresentationReader(new StringReader(text));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("emanon.org", resource.Name);
            Assert.AreEqual(DnsClass.IN, resource.Class);
            Assert.AreEqual(DnsType.PTR, resource.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(PTRRecord));
        }

        [TestMethod]
        public void ReadResourceWithEscapedOrigin()
        {
            var text = @"
$ORIGIN emanon\.org. ; no such place\r\n
@ PTR localhost
";
            var reader = new PresentationReader(new StringReader(text));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual(@"emanon\.org", resource.Name);
            Assert.AreEqual(DnsClass.IN, resource.Class);
            Assert.AreEqual(DnsType.PTR, resource.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, resource.TTL);
            Assert.IsInstanceOfType(resource, typeof(PTRRecord));
            Assert.AreEqual(1, resource.Name.Labels.Count);
        }

        [TestMethod]
        public void ReadResourceWithTTL()
        {
            var text = @"
$TTL 120 ; 2 minutes\r\n
emanon.org PTR localhost
";
            var reader = new PresentationReader(new StringReader(text));
            var resource = reader.ReadResourceRecord();
            Assert.AreEqual("emanon.org", resource.Name);
            Assert.AreEqual(DnsClass.IN, resource.Class);
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
            var reader = new PresentationReader(new StringReader(text));
            var a = reader.ReadResourceRecord();
            Assert.AreEqual("emanon.org", a.Name);
            Assert.AreEqual(DnsClass.IN, a.Class);
            Assert.AreEqual(DnsType.A, a.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, a.TTL);
            Assert.IsInstanceOfType(a, typeof(ARecord));

            var aaaa = reader.ReadResourceRecord();
            Assert.AreEqual("emanon.org", aaaa.Name);
            Assert.AreEqual(DnsClass.IN, aaaa.Class);
            Assert.AreEqual(DnsType.AAAA, aaaa.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, aaaa.TTL);
            Assert.IsInstanceOfType(aaaa, typeof(AAAARecord));
        }

        [TestMethod]
        public void ReadResourceWithPreviousEscapedDomain()
        {
            var text = @"
emanon\126.org A 127.0.0.1
           AAAA ::1
";
            var reader = new PresentationReader(new StringReader(text));
            var a = reader.ReadResourceRecord();
            Assert.AreEqual("emanon~.org", a.Name);
            Assert.AreEqual(DnsClass.IN, a.Class);
            Assert.AreEqual(DnsType.A, a.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, a.TTL);
            Assert.IsInstanceOfType(a, typeof(ARecord));
            Assert.AreEqual(2, a.Name.Labels.Count);

            var aaaa = reader.ReadResourceRecord();
            Assert.AreEqual("emanon~.org", aaaa.Name);
            Assert.AreEqual(DnsClass.IN, aaaa.Class);
            Assert.AreEqual(DnsType.AAAA, aaaa.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, aaaa.TTL);
            Assert.IsInstanceOfType(aaaa, typeof(AAAARecord));
            Assert.AreEqual(2, a.Name.Labels.Count);
        }

        [TestMethod]
        public void ReadResourceWithLeadingEscapedDomainName()
        {
            var text = @"\126emanon.org A 127.0.0.1";
            var reader = new PresentationReader(new StringReader(text));
            var a = reader.ReadResourceRecord();
            Assert.AreEqual("~emanon.org", a.Name);
            Assert.AreEqual(DnsClass.IN, a.Class);
            Assert.AreEqual(DnsType.A, a.Type);
            Assert.AreEqual(ResourceRecord.DefaultTTL, a.TTL);
            Assert.IsInstanceOfType(a, typeof(ARecord));
            Assert.AreEqual(2, a.Name.Labels.Count);
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
wwwtest       IN  CNAME www                   ; wwwtest.example.com is another alias for www.example.com mail          IN  A     192.0.2.3             ; IPv4 address for mail.example.com
mail          IN  A     192.0.2.3             ; IPv4 address for mail.example.com
mail2         IN  A     192.0.2.4             ; IPv4 address for mail2.example.com
mail3         IN  A     192.0.2.5             ; IPv4 address for mail3.example.com
";
            var reader = new PresentationReader(new StringReader(text));
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
            var reader = new PresentationReader(new StringReader("\\# 0"));
            var rdata = reader.ReadResourceData();
            Assert.AreEqual(0, rdata.Length);

            reader = new PresentationReader(new StringReader("\\# 3 abcdef"));
            rdata = reader.ReadResourceData();
            CollectionAssert.AreEqual(new byte[] { 0xab, 0xcd, 0xef }, rdata);

            reader = new PresentationReader(new StringReader("\\# 3 ab cd ef"));
            rdata = reader.ReadResourceData();
            CollectionAssert.AreEqual(new byte[] { 0xab, 0xcd, 0xef }, rdata);

            reader = new PresentationReader(new StringReader("\\# 3 abcd (\r\n  ef )"));
            rdata = reader.ReadResourceData();
            CollectionAssert.AreEqual(new byte[] { 0xab, 0xcd, 0xef }, rdata);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ReadResourceData_MissingLeadin()
        {
            var reader = new PresentationReader(new StringReader("0"));
            var _ = reader.ReadResourceData();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ReadResourceData_BadHex_BadDigit()
        {
            var reader = new PresentationReader(new StringReader("\\# 3 ab cd ez"));
            var _ = reader.ReadResourceData();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ReadResourceData_BadHex_NotEven()
        {
            var reader = new PresentationReader(new StringReader("\\# 3 ab cd e"));
            var _ = reader.ReadResourceData();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ReadResourceData_BadHex_TooFew()
        {
            var reader = new PresentationReader(new StringReader("\\# 3 abcd"));
            var _ = reader.ReadResourceData();
        }

        [TestMethod]
        public void ReadType()
        {
            var reader = new PresentationReader(new StringReader("A TYPE1 MX"));
            Assert.AreEqual(DnsType.A, reader.ReadDnsType());
            Assert.AreEqual(DnsType.A, reader.ReadDnsType());
            Assert.AreEqual(DnsType.MX, reader.ReadDnsType());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void ReadType_BadName()
        {
            var reader = new PresentationReader(new StringReader("BADNAME"));
            reader.ReadDnsType();
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ReadType_BadDigit()
        {
            var reader = new PresentationReader(new StringReader("TYPEX"));
            reader.ReadDnsType();
        }

        [TestMethod]
        public void ReadMultipleStrings()
        {
            var expected = new List<string> { "abc", "def", "ghi" };
            var reader = new PresentationReader(new StringReader("abc def (\r\nghi)\r\n"));
            var actual = new List<string>();
            while (!reader.IsEndOfLine())
            {
                actual.Add(reader.ReadString());
            }
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ReadMultipleStrings2()
        {
            var expected = new List<string> { "abc", "def", "ghi", "jkl"};
            var reader = new PresentationReader(new StringReader("abc def (\r\nghi) jkl   \r\n"));
            var actual = new List<string>();
            while (!reader.IsEndOfLine())
            {
                actual.Add(reader.ReadString());
            }
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ReadMultipleStrings3()
        {
            var expected = new List<string> { "abc", "def", "ghi" };
            var reader = new PresentationReader(new StringReader("abc def (\rghi)\r"));
            var actual = new List<string>();
            while (!reader.IsEndOfLine())
            {
                actual.Add(reader.ReadString());
            }
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ReadMultipleStrings_LF()
        {
            var expected = new List<string> { "abc", "def" };
            var reader = new PresentationReader(new StringReader("abc def\rghi"));
            var actual = new List<string>();
            while (!reader.IsEndOfLine())
            {
                actual.Add(reader.ReadString());
            }
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ReadMultipleStrings_CRLF()
        {
            var expected = new List<string> { "abc", "def" };
            var reader = new PresentationReader(new StringReader("abc def\r\nghi"));
            var actual = new List<string>();
            while (!reader.IsEndOfLine())
            {
                actual.Add(reader.ReadString());
            }
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ReadBase64String()
        {
            var expected = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            var reader = new PresentationReader(new StringReader("AAECAwQFBgcICQoLDA0ODw=="));
            CollectionAssert.AreEqual(expected, reader.ReadBase64String());

            reader = new PresentationReader(new StringReader("AAECAwQFBg  cICQoLDA0ODw=="));
            CollectionAssert.AreEqual(expected, reader.ReadBase64String());

            reader = new PresentationReader(new StringReader("AAECAwQFBg  (\r\n  cICQo\r\n  LDA0ODw\r\n== )"));
            CollectionAssert.AreEqual(expected, reader.ReadBase64String());
        }

        [TestMethod]
        public void ReadDateTime()
        {
            DateTime expected = new DateTime(2004, 9, 16);

            var reader = new PresentationReader(new StringReader("1095292800 20040916000000"));
            Assert.AreEqual(expected, reader.ReadDateTime());
            Assert.AreEqual(expected, reader.ReadDateTime());
        }

        [TestMethod]
        public void ReadDomainName_Escaped()
        {
            var foo = new DomainName("foo.com");
            var drSmith = new DomainName(@"dr\. smith.com");
            var reader = new PresentationReader(new StringReader(@"dr\.\032smith.com foo.com"));
            Assert.AreEqual(drSmith, reader.ReadDomainName());
            Assert.AreEqual(foo, reader.ReadDomainName());
        }

    }
}
