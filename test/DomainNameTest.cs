using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Makaretu.Dns
{
    [TestClass]
    public class DomainNameTest
    {
        [TestMethod]
        public void Standard()
        {
            var name = new DomainName("my.example.org");

            Assert.AreEqual(3, name.Labels.Count);
            Assert.AreEqual("my", name.Labels[0]);
            Assert.AreEqual("example", name.Labels[1]);
            Assert.AreEqual("org", name.Labels[2]);

            Assert.AreEqual("my.example.org", name.ToString());
        }

        [TestMethod]
        public void TopLevelDomain()
        {
            var name = new DomainName("org");

            Assert.AreEqual(1, name.Labels.Count);
            Assert.AreEqual("org", name.Labels[0]);

            Assert.AreEqual("org", name.ToString());
        }

        [TestMethod]
        public void Root()
        {
            var name = new DomainName("");

            Assert.AreEqual(0, name.Labels.Count);

            Assert.AreEqual("", name.ToString());
        }

        [TestMethod]
        public void EscapedDotCharacter()
        {
            var name = new DomainName(@"my\.example.org");

            Assert.AreEqual(2, name.Labels.Count);
            Assert.AreEqual("my.example", name.Labels[0]);
            Assert.AreEqual("org", name.Labels[1]);

            Assert.AreEqual(@"my\.example.org", name.ToString());
        }

        [TestMethod]
        public void EscapedDotDigits()
        {
            var name = new DomainName(@"my\046example.org");

            Assert.AreEqual(2, name.Labels.Count);
            Assert.AreEqual("my.example", name.Labels[0]);
            Assert.AreEqual("org", name.Labels[1]);

            Assert.AreEqual(@"my\.example.org", name.ToString());
        }


        [TestMethod]
        public void ImplicitParsingOfString()
        {
            DomainName name = @"my\046example.org";
            Assert.AreEqual(2, name.Labels.Count);
            Assert.AreEqual("my.example", name.Labels[0]);
            Assert.AreEqual("org", name.Labels[1]);

            name = @"my\.example.org";
            Assert.AreEqual(2, name.Labels.Count);
            Assert.AreEqual("my.example", name.Labels[0]);
            Assert.AreEqual("org", name.Labels[1]);

            name = @"my.example.org";
            Assert.AreEqual(3, name.Labels.Count);
            Assert.AreEqual("my", name.Labels[0]);
            Assert.AreEqual("example", name.Labels[1]);
            Assert.AreEqual("org", name.Labels[2]);
        }

        [TestMethod]
        public void FromLabels()
        {
            var name = new DomainName("my.example", "org");

            Assert.AreEqual(2, name.Labels.Count);
            Assert.AreEqual("my.example", name.Labels[0]);
            Assert.AreEqual("org", name.Labels[1]);

            Assert.AreEqual(@"my\.example.org", name.ToString());
        }

        [TestMethod]
        public void Equality()
        {
            var a = new DomainName(@"my\.example.org");
            var b = new DomainName("my.example", "org");
            var c = new DomainName(@"my\046example.org");
            var d = new DomainName(@"My\.EXAMPLe.ORg");
            var other1 = new DomainName("example.org");
            var other2 = new DomainName("org");

            Assert.AreEqual(a, b);
            Assert.AreEqual(a, c);
            Assert.AreEqual(a, d);
            Assert.AreNotEqual(a, other1);
            Assert.AreNotEqual(a, other2);
            Assert.AreNotEqual(a, null);

            Assert.IsTrue(a == b);
            Assert.IsTrue(a == c);
            Assert.IsTrue(a == d);
            Assert.IsFalse(a == other1);
            Assert.IsFalse(a == other2);
            Assert.IsFalse(a == null);

            Assert.IsFalse(a != b);
            Assert.IsFalse(a != c);
            Assert.IsFalse(a != d);
            Assert.IsTrue(a != other1);
            Assert.IsTrue(a != other2);
            Assert.IsTrue(a != null);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals(c));
            Assert.IsTrue(a.Equals(d));
            Assert.IsFalse(a.Equals(other1));
            Assert.IsFalse(a.Equals(other2));
            Assert.IsFalse(a.Equals(null));
        }

        [TestMethod]
        public void HashEquality()
        {
            var a = new DomainName(@"my\.example.org");
            var b = new DomainName("my.example", "org");
            var c = new DomainName(@"my\046example.org");
            var d = new DomainName(@"My\.EXAMPLe.ORg");
            var other1 = new DomainName("example.org");
            var other2 = new DomainName("org");

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreEqual(a.GetHashCode(), d.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), other1.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), other2.GetHashCode());
        }

        [TestMethod]
        public void ToCanonical()
        {
            var a = new DomainName("My.EXAMPLe.ORg");
            Assert.AreEqual("My.EXAMPLe.ORg", a.ToString());
            Assert.AreEqual("my.example.org", a.ToCanonical().ToString());
        }

        [TestMethod]
        public void IsSubdomainOf()
        {
            var zone = new DomainName("example.org");
            Assert.IsFalse(zone.IsSubdomainOf(zone));
            Assert.IsTrue(new DomainName("a.example.org").IsSubdomainOf(zone));
            Assert.IsTrue(new DomainName("a.b.example.org").IsSubdomainOf(zone));
            Assert.IsTrue(new DomainName("a.Example.org").IsSubdomainOf(zone));
            Assert.IsTrue(new DomainName("a.b.Example.ORG").IsSubdomainOf(zone));
            Assert.IsFalse(new DomainName(@"a\.example.org").IsSubdomainOf(zone));
            Assert.IsTrue(new DomainName(@"a\.b.example.org").IsSubdomainOf(zone));
            Assert.IsTrue(new DomainName(@"a\.b.example.ORG").IsSubdomainOf(zone));
            Assert.IsFalse(new DomainName("a.org").IsSubdomainOf(zone));
            Assert.IsFalse(new DomainName("a.b.org").IsSubdomainOf(zone));
        }

        [TestMethod]
        public void BelongsTo()
        {
            var zone = new DomainName("example.org");
            Assert.IsTrue(zone.BelongsTo(zone));
            Assert.IsTrue(new DomainName("ExamPLE.Org").BelongsTo(zone));
            Assert.IsTrue(new DomainName("A.ExamPLE.Org").BelongsTo(zone));
            Assert.IsTrue(new DomainName("a.example.org").BelongsTo(zone));
            Assert.IsTrue(new DomainName("a.b.example.org").BelongsTo(zone));
            Assert.IsTrue(new DomainName("a.Example.org").BelongsTo(zone));
            Assert.IsTrue(new DomainName("a.b.Example.ORG").BelongsTo(zone));
            Assert.IsFalse(new DomainName(@"a\.example.org").BelongsTo(zone));
            Assert.IsTrue(new DomainName(@"a\.b.example.org").BelongsTo(zone));
            Assert.IsTrue(new DomainName(@"a\.b.example.ORG").BelongsTo(zone));
            Assert.IsFalse(new DomainName("a.org").BelongsTo(zone));
            Assert.IsFalse(new DomainName("a.b.org").BelongsTo(zone));
        }

        [TestMethod]
        public void Parent()
        {
            var name = new DomainName(@"a.b\.c.example.org");
            var expected = new DomainName(@"b\.c.example.org");
            Assert.AreEqual(expected, name.Parent());

            name = new DomainName("org");
            expected = new DomainName("");
            Assert.AreEqual(expected, name.Parent());

            Assert.IsNull(expected.Parent());
        }

        [TestMethod]
        public void Joining()
        {
            var a = new DomainName(@"foo\.bar");
            var b = new DomainName("x.y.z");
            var c = DomainName.Join(a, b);
            Assert.AreEqual(4, c.Labels.Count);
            Assert.AreEqual("foo.bar", c.Labels[0]);
            Assert.AreEqual("x", c.Labels[1]);
            Assert.AreEqual("y", c.Labels[2]);
            Assert.AreEqual("z", c.Labels[3]);
        }

        [TestMethod]
        public void Rfc4343_Section_2()
        {
            Assert.AreEqual(new DomainName("foo.example.net."), new DomainName("Foo.ExamplE.net."));
            Assert.AreEqual(new DomainName("69.2.0.192.in-addr.arpa."), new DomainName("69.2.0.192.in-ADDR.ARPA."));
        }

        [TestMethod]
        public void Rfc4343_Section_21_Backslash()
        {
            var aslashb = new DomainName(@"a\\b");
            Assert.AreEqual(1, aslashb.Labels.Count);
            Assert.AreEqual(@"a\b", aslashb.Labels[0]);
            Assert.AreEqual(@"a\092b", aslashb.ToString());

            Assert.AreEqual(aslashb, new DomainName(@"a\092b"));
        }

        [TestMethod]
        public void Rfc4343_Section_21_4Digits()
        {
            var a = new DomainName(@"a\\4");
            var b = new DomainName(@"a\0924");
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void Rfc4343_Section_22_SpacesAndDots()
        {
            var a = new DomainName(@"Donald\032E\.\032Eastlake\0323rd.example");
            Assert.AreEqual(2, a.Labels.Count);
            Assert.AreEqual("Donald E. Eastlake 3rd", a.Labels[0]);
            Assert.AreEqual("example", a.Labels[1]);
        }

        [TestMethod]
        public void Rfc4343_Section_22_Binary()
        {
            var a = new DomainName(@"a\000\\\255z.example");
            Assert.AreEqual(2, a.Labels.Count);
            Assert.AreEqual('a', a.Labels[0][0]);
            Assert.AreEqual((char)0, a.Labels[0][1]);
            Assert.AreEqual('\\', a.Labels[0][2]);
            Assert.AreEqual((char)0xff, a.Labels[0][3]);
            Assert.AreEqual('z', a.Labels[0][4]);
            Assert.AreEqual("example", a.Labels[1]);

            Assert.AreEqual(@"a\000\092\255z.example", a.ToString());
            Assert.AreEqual(a, new DomainName(a.ToString()));
        }

        [TestMethod]
        public void FormattedString()
        {
            var name = new DomainName(@"foo ~ \.bar-12A.org");
            Assert.AreEqual(@"foo\032~\032\.bar-12A.org", name.ToString());
        }
    }
}
