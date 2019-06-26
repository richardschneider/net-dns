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
        public void IsSubdomain()
        {
            var zone = new DomainName("example.org");
            Assert.IsFalse(zone.IsSubdomain(zone));
            Assert.IsTrue(new DomainName("a.example.org").IsSubdomain(zone));
            Assert.IsTrue(new DomainName("a.b.example.org").IsSubdomain(zone));
            Assert.IsTrue(new DomainName("a.Example.org").IsSubdomain(zone));
            Assert.IsTrue(new DomainName("a.b.Example.ORG").IsSubdomain(zone));
            Assert.IsFalse(new DomainName(@"a\.example.org").IsSubdomain(zone));
            Assert.IsTrue(new DomainName(@"a\.b.example.org").IsSubdomain(zone));
            Assert.IsTrue(new DomainName(@"a\.b.example.ORG").IsSubdomain(zone));
            Assert.IsFalse(new DomainName("a.org").IsSubdomain(zone));
            Assert.IsFalse(new DomainName("a.b.org").IsSubdomain(zone));
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
    }
}
