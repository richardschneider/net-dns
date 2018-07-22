using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Makaretu.Dns.Resolving
{

    [TestClass]
    public class CatalogTest
    {
        public const string exampleDotOrgZoneText = @"
 $ORIGIN example.org.
 $TTL 3600
 @    SOA   ns1 username.example.org. ( 2007120710 1 2 4 1 )
      NS    ns1
      NS    ns2
      MX    10 mail
 ns1  A     192.0.2.1
 ns2  A     192.0.2.2
 mail A     192.0.2.3
 x    PTR   ns1
_http._tcp   PTR a._http._tcp
a._http._tcp SRV 0 5 80 mail
             TXT needhttps=false needcredential=true" + "\r\n";

        public const string exampleDotComZoneText = @"
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


        [TestMethod]
        public void IncludeZone()
        {
            var catalog = new Catalog();
            var reader = new MasterReader(new StringReader(exampleDotComZoneText));
            var zone = catalog.IncludeZone(reader);
            Assert.AreEqual("example.com", zone.Name);
            Assert.IsTrue(zone.Authoritative);

            Assert.IsTrue(catalog.ContainsKey("example.com"));
            Assert.IsTrue(catalog.ContainsKey("ns.example.com"));
            Assert.IsTrue(catalog.ContainsKey("www.example.com"));
            Assert.IsTrue(catalog.ContainsKey("wwwtest.example.com"));
            Assert.IsTrue(catalog.ContainsKey("mail.example.com"));
            Assert.IsTrue(catalog.ContainsKey("mail2.example.com"));
            Assert.IsTrue(catalog.ContainsKey("mail3.example.com"));

            Assert.IsTrue(catalog["mail.example.com"].Authoritative);
        }

        [TestMethod]
        public void IncludeZone_AlreadyExists()
        {
            var catalog = new Catalog();
            var reader = new MasterReader(new StringReader(exampleDotComZoneText));
            var zone = catalog.IncludeZone(reader);
            Assert.AreEqual("example.com", zone.Name);

            reader = new MasterReader(new StringReader(exampleDotComZoneText));
            ExceptionAssert.Throws<InvalidDataException>(() => catalog.IncludeZone(reader));
        }

        [TestMethod]
        public void IncludeZone_NoResources()
        {
            var catalog = new Catalog();
            var reader = new MasterReader(new StringReader(""));
            ExceptionAssert.Throws<InvalidDataException>(() => catalog.IncludeZone(reader));
        }

        [TestMethod]
        public void IncludeZone_MissingSOA()
        {
            var text = "foo.org A 127.0.0.1";
            var catalog = new Catalog();
            var reader = new MasterReader(new StringReader(text));
            ExceptionAssert.Throws<InvalidDataException>(() => catalog.IncludeZone(reader));
        }

        [TestMethod]
        public void IncludeZone_InvalidName()
        {
            var text = exampleDotOrgZoneText + " not.in.zone. A 127.0.0.1 ; bad";
            var catalog = new Catalog();
            var reader = new MasterReader(new StringReader(text));
            ExceptionAssert.Throws<InvalidDataException>(() => catalog.IncludeZone(reader));
        }

        [TestMethod]
        public void MultipleZones()
        {
            var catalog = new Catalog();

            var reader = new MasterReader(new StringReader(exampleDotComZoneText));
            var zone = catalog.IncludeZone(reader);
            Assert.AreEqual("example.com", zone.Name);

            reader = new MasterReader(new StringReader(exampleDotOrgZoneText));
            zone = catalog.IncludeZone(reader);
            Assert.AreEqual("example.org", zone.Name);
        }

        [TestMethod]
        public void RemoveZone()
        {
            var catalog = new Catalog();

            var reader = new MasterReader(new StringReader(exampleDotComZoneText));
            var zone = catalog.IncludeZone(reader);
            Assert.AreEqual("example.com", zone.Name);
            Assert.AreEqual(7, catalog.Count);

            reader = new MasterReader(new StringReader(exampleDotOrgZoneText));
            zone = catalog.IncludeZone(reader);
            Assert.AreEqual("example.org", zone.Name);
            Assert.AreEqual(7 + 7, catalog.Count);

            catalog.RemoveZone("example.org");
            Assert.AreEqual(7, catalog.Count);

            catalog.RemoveZone("example.com");
            Assert.AreEqual(0, catalog.Count);
        }

        [TestMethod]
        public void NamesAreCaseInsenstive()
        {
            var catalog = new Catalog();
            var reader = new MasterReader(new StringReader(exampleDotComZoneText));
            catalog.IncludeZone(reader);

            Assert.IsTrue(catalog.ContainsKey("EXAMPLE.COM"));
            Assert.IsTrue(catalog.ContainsKey("NS.EXAMPLE.COM"));
            Assert.IsTrue(catalog.ContainsKey("WWW.EXAMPLE.COM"));
            Assert.IsTrue(catalog.ContainsKey("WWWTEST.EXAMPLE.COM"));
            Assert.IsTrue(catalog.ContainsKey("MAIL.EXAMPLE.COM"));
            Assert.IsTrue(catalog.ContainsKey("MAIL2.EXAMPLE.COM"));
            Assert.IsTrue(catalog.ContainsKey("MAIL3.EXAMPLE.COM"));
        }

        [TestMethod]
        public void AddResource()
        {
            var a = AddressRecord.Create("foo", IPAddress.Loopback);
            var aaaa = AddressRecord.Create("foo", IPAddress.IPv6Loopback);
            var catalog = new Catalog();

            var n1 = catalog.Add(a, true);
            Assert.IsTrue(n1.Authoritative);
            Assert.IsTrue(n1.Resources.Contains(a));

            var n2 = catalog.Add(aaaa, false);
            Assert.AreSame(n1, n2);
            Assert.IsTrue(n1.Authoritative);
            Assert.IsTrue(n1.Resources.Contains(a));
            Assert.IsTrue(n1.Resources.Contains(aaaa));
        }

        [TestMethod]
        public void AddResource_Same()
        {
            var a = AddressRecord.Create("foo", IPAddress.Loopback);
            var catalog = new Catalog();

            var n1 = catalog.Add(a);
            Assert.IsTrue(n1.Resources.Contains(a));

            var n2 = catalog.Add(a);
            Assert.AreSame(n1, n2);
            Assert.IsTrue(n1.Resources.Contains(a));
            Assert.AreEqual(1, n1.Resources.Count);
        }

        [TestMethod]
        public void AddResource_Duplicate()
        {
            var a = AddressRecord.Create("foo", IPAddress.Loopback);
            var b = AddressRecord.Create("foo", IPAddress.Loopback);
            Assert.AreEqual(a, b);
            var catalog = new Catalog();

            var n1 = catalog.Add(a);
            Assert.IsTrue(n1.Resources.Contains(a));

            var n2 = catalog.Add(b);
            Assert.AreSame(n1, n2);
            Assert.IsTrue(n1.Resources.Contains(a));
            Assert.IsTrue(n1.Resources.Contains(b));
            Assert.AreEqual(1, n1.Resources.Count);
        }

        [TestMethod]
        public void AddResource_Latest()
        {
            var a = AddressRecord.Create("foo", IPAddress.Loopback);
            var b = AddressRecord.Create("foo", IPAddress.Loopback);
            a.TTL = TimeSpan.FromHours(2);
            b.CreationTime = a.CreationTime + TimeSpan.FromHours(1);
            b.TTL = TimeSpan.FromHours(3);
            Assert.AreEqual(a, b);
            var catalog = new Catalog();

            var n1 = catalog.Add(a);
            Assert.IsTrue(n1.Resources.Contains(a));

            var n2 = catalog.Add(b);
            Assert.AreSame(n1, n2);
            Assert.IsTrue(n1.Resources.Contains(a));
            Assert.IsTrue(n1.Resources.Contains(b));
            Assert.AreEqual(1, n1.Resources.Count);
            Assert.AreEqual(b.CreationTime, n1.Resources.First().CreationTime);
            Assert.AreEqual(b.TTL, n1.Resources.First().TTL);
        }

        [TestMethod]
        public void RootHints()
        {
            var catalog = new Catalog();
            var root = catalog.IncludeRootHints();
            Assert.AreEqual("", root.Name);
            Assert.IsTrue(root.Authoritative);
            Assert.IsTrue(root.Resources.OfType<NSRecord>().Any());
        }
    }
}