using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Makaretu.Dns
{
    /// <summary>
    /// A domain name consists of one or more parts, <see cref="Labels"/>, that are 
    /// conventionally delimited by dots, such as "example.org".
    /// </summary>
    /// <remarks>
    ///   Equality is based on the number of and the case-insenstive contents of <see cref="Labels"/>.
    /// </remarks>
    public class DomainName : IEquatable<DomainName>
    {
        const string dot = ".";
        const char dotChar = '.';
        const string escapedDot = @"\.";

        /// <summary>
        ///   A sequence of labels that make up the domain name.
        /// </summary>
        /// <value>
        ///   A sequece of strings.
        /// </value>
        /// <remarks>
        ///   The last label is the TLD (top level domain).
        /// </remarks>
        public List<string> Labels { get; set; } = new List<string>();

        /// <summary>
        ///   Creates a new instance of the <see cref="DomainName"/> class from
        ///   the specified name.
        /// </summary>
        /// <param name="name">
        ///   The dot separated labels; such as "example.org".
        /// </param>
        /// <remarks>
        ///   The name can contain backslash to escape a character.
        ///   See <see href="https://tools.ietf.org/html/rfc4343">RFC 4343</see> 
        ///   for the character escaping rules.
        ///   <note>
        ///   To use us backslash in a domain name (highly unusaual), you must use a double backslash.
        ///   </note>
        /// </remarks>
        public DomainName(string name)
        {
            Parse(name);
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="DomainName"/> class from
        ///   the sequence of label.
        /// </summary>
        /// <param name="labels">
        ///   The <see cref="Labels"/>.
        /// </param>
        /// <remarks>
        ///   The labels are not parsed; character escaping is not performed.
        /// </remarks>
        public DomainName(params string[] labels)
        {
            Labels.AddRange(labels);
        }

        /// <summary>
        ///   Returns the textual representation.
        /// </summary>
        /// <returns>
        ///   The concatenation of the <see cref="Labels"/> separated by a dot.
        /// </returns>
        /// <remarks>
        ///   If a label contains a dot, then it is escaped with a backslash.
        /// </remarks>
        public override string ToString()
        {
            return string.Join(dot, Labels.Select(label => label.Replace(dot, escapedDot)));
        }

        /// <summary>
        ///   Gets the canonical form of the domain name.
        /// </summary>
        /// <returns>
        ///   A domain name in the canonical form.
        /// </returns>
        /// <remarks>
        ///   All uppercase US-ASCII letters in the <see cref="Labels"/> are
        ///   replaced by the corresponding lowercase US-ASCII letters.
        /// </remarks>
        public DomainName ToCanonical()
        {
            var labels = Labels
                .Select(l => l.ToLowerInvariant())
                .ToArray();
            return new DomainName(labels);
        }

        /// <summary>
        ///   Determines if this domain name is a subdomain of another
        ///   domain name.
        /// </summary>
        /// <param name="domain">
        ///   Another domain.
        /// </param>
        /// <returns>
        ///   <b>true</b> if this domain name is a subdomain of <paramref name="domain"/>.
        /// </returns>
        public bool IsSubdomain(DomainName domain)
        {
            if (domain == null)
            {
                return false;
            }
            if (Labels.Count <= domain.Labels.Count)
            {
                return false;
            }
            var i = Labels.Count - 1;
            var j = domain.Labels.Count - 1;
            for (; 0 <= j; --i, --j)
            {
                if (!DnsObject.NamesEquals(Labels[i], domain.Labels[j]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///   Gets the parent's domain name.
        /// </summary>
        /// <returns>
        ///   The domain name of the parent or <b>null</b> if
        ///   there is no parent; e.g. this is the root.
        /// </returns>
        public DomainName Parent()
        {
            if (Labels.Count == 0)
            {
                return null;
            }

            return new DomainName(Labels.Skip(1).ToArray());
        }

        void Parse(string name)
        {
            Labels.Clear();
            var label = new StringBuilder();
            var n = name.Length;
            for (int i = 0; i < n; ++i)
            {
                var c = name[i];

                // An escaped character is either \C or \DDD.
                if (c == '\\')
                {
                    c = name[++i];
                    if (!char.IsDigit(c))
                    {
                        label.Append(c);
                    }
                    else
                    {
                        var number = c - '0';
                        number = (number * 10) + (name[++i] - '0');
                        number = (number * 10) + (name[++i] - '0');
                        label.Append((char)number);
                    }
                    continue;
                }

                // End of label?
                if (c == dotChar)
                {
                    Labels.Add(label.ToString());
                    label.Clear();
                    continue;
                }

                // Just part of the label.
                label.Append(c);
            }
            if (label.Length > 0)
            {
                Labels.Add(label.ToString());
            }

        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ToString().ToLowerInvariant().GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var that = obj as DomainName;
            return (that == null)
               ? false
               : this.Equals(that);
        }

        /// <inheritdoc />
        public bool Equals(DomainName that)
        {
            var n = this.Labels.Count;
            if (n != that.Labels.Count)
            {
                return false;
            }
            for (var i = 0; i < n; ++i)
            {
                if (!DnsObject.NamesEquals(this.Labels[i], that.Labels[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///   Value equality.
        /// </summary>
        public static bool operator ==(DomainName a, DomainName b)
        {
            if (object.ReferenceEquals(a, b)) return true;
            if (a is null) return false;
            if (b is null) return false;

            return a.Equals(b);
        }

        /// <summary>
        ///   Value inequality.
        /// </summary>
        public static bool operator !=(DomainName a, DomainName b)
        {
            return !(a == b);
        }

    }
}
