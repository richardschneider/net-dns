using SimpleBase;
using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Methods to read DNS data items encoded in the master file format.
    /// </summary>
    public class MasterReader
    {
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        TextReader text;
        TimeSpan? defaultTTL = null;
        string defaultDomainName = null;
        int parenLevel = 0;

        /// <summary>
        ///   The reader relative position within the stream.
        /// </summary>
        public int Position;

        /// <summary>
        ///   Creates a new instance of the <see cref="MasterReader"/> using the
        ///   specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="text">
        ///   The source for data items.
        /// </param>
        public MasterReader(TextReader text)
        {
            this.text = text;
        }

        /// <summary>
        ///   The origin domain name, sometimes called the zone name.
        /// </summary>
        /// <value>
        ///   Defaults to "".
        /// </value>
        /// <remarks>
        ///   <b>Origin</b> is used when the domain name "@" is used
        ///   for a domain name.
        /// </remarks>
        public string Origin { get; set; } = String.Empty;

        /// <summary>
        ///   Read a byte.
        /// </summary>
        /// <returns>
        ///   The number as a byte.
        /// </returns>
        public byte ReadByte()
        {
            return byte.Parse(ReadToken(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///   Read an unsigned short.
        /// </summary>
        /// <returns>
        ///   The number as an unsigned short.
        /// </returns>
        public ushort ReadUInt16()
        {
            return ushort.Parse(ReadToken(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///   Read an unsigned int.
        /// </summary>
        /// <returns>
        ///   The number as an unsignd int.
        /// </returns>
        public uint ReadUInt32()
        {
            return uint.Parse(ReadToken(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///   Read a domain name.
        /// </summary>
        /// <returns>
        ///   The domain name as a string.
        /// </returns>
        public string ReadDomainName()
        {
            return MakeAbsoluteDomainName(ReadToken());
        }

        string MakeAbsoluteDomainName(string name)
        {
            // If an absolute name.
            if (name.EndsWith("."))
                return name.Substring(0, name.Length - 1);

            // Then its a relative name.
            return (name + "." + Origin).TrimEnd('.');
        }

        /// <summary>
        ///   Read a string.
        /// </summary>
        /// <returns>
        ///   The string.
        /// </returns>
        public string ReadString()
        {
            return ReadToken();
        }

        /// <summary>
        ///   Read bytes encoded in base-64.
        /// </summary>
        /// <returns>
        ///   The bytes.
        /// </returns>
        /// <remarks>
        ///   This must be the last field in the RDATA because the string
        ///   can contain embedded spaces.
        /// </remarks>
        public byte[] ReadBase64String()
        {
            // Handle embedded space and CRLFs inside of parens.
            var sb = new StringBuilder();
            while (!IsEndOfLine())
            {
                sb.Append(ReadToken());
            }
            return Convert.FromBase64String(sb.ToString());
        }

        /// <summary>
        ///   Read a time span (interval) in 16-bit seconds.
        /// </summary>
        /// <returns>
        ///   A <see cref="TimeSpan"/> with second resolution.
        /// </returns>
        public TimeSpan ReadTimeSpan16()
        {
            return TimeSpan.FromSeconds(ReadUInt16());
        }

        /// <summary>
        ///   Read a time span (interval) in 32-bit seconds.
        /// </summary>
        /// <returns>
        ///   A <see cref="TimeSpan"/> with second resolution.
        /// </returns>
        public TimeSpan ReadTimeSpan32()
        {
            return TimeSpan.FromSeconds(ReadUInt32());
        }

        /// <summary>
        ///   Read an Internet address.
        /// </summary>
        /// <param name="length">
        ///   Ignored.
        /// </param>
        /// <returns>
        ///   An <see cref="IPAddress"/>.
        /// </returns>
        public IPAddress ReadIPAddress(int length = 4)
        {
            return IPAddress.Parse(ReadToken());
        }

        /// <summary>
        ///   Read a DNS Type.
        /// </summary>
        /// <remarks>
        ///   Either the name of a <see cref="DnsType"/> or
        ///   the string "TYPEx".
        /// </remarks>
        public DnsType ReadDnsType()
        {
            var token = ReadToken();
            if (token.StartsWith("TYPE"))
            {
                return (DnsType)ushort.Parse(token.Substring(4), CultureInfo.InvariantCulture);
            }
            return (DnsType)Enum.Parse(typeof(DnsType), token);
        }

        /// <summary>
        ///   Read the number of seonds since the unix epoch.
        /// </summary>
        /// <returns>
        ///   A 32 bit integer.
        /// </returns>
        /// <remarks>
        ///   The unix epoch starts at 00:00:00 on 1 January 1970 UTC.
        ///   With 32 bits, time will end at 3:14:08 on 19 January 2038 UTC.
        ///   <para>
        ///   The seconds can also be formatted at "yyyyMMddHHmmss".
        ///   </para>
        /// </remarks>
        public uint  ReadUnixSeconds32()
        {
            var token = ReadToken();
            if (token.Length == 14)
            {
                var date = DateTime.ParseExact(
                    token, 
                    "yyyyMMddHHmmss",
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                var seconds = (date - UnixEpoch).TotalSeconds;
                return Convert.ToUInt32(seconds);
            }

            return uint.Parse(token, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///   Read hex encoded RDATA.
        /// </summary>
        /// <returns>
        ///   A byte array containing the RDATA.
        /// </returns>
        /// <remarks>
        ///   See <see href="https://tools.ietf.org/html/rfc3597#section-5"/> for all
        ///   the details.
        /// </remarks>
        public byte[] ReadResourceData()
        {
            var leadin = ReadToken();
            if (leadin != "#")
                throw new FormatException($"Expected RDATA leadin '\\#', not '{leadin}'.");
            var length = ReadUInt32();
            if (length == 0)
                return new byte[0];

            // Get the hex string.
            var sb = new StringBuilder();
            while (sb.Length < length * 2)
            {
                var word = ReadToken();
                if (word.Length == 0)
                    break;
                if (word.Length % 2 != 0)
                    throw new FormatException($"The hex word ('{word}') must have an even number of digits.");
                sb.Append(word);
            }
            if (sb.Length != length * 2)
                throw new FormatException("Wrong number of RDATA hex digits.");

            // Convert hex string into byte array.
            try
            {
                return Base16.Decode(sb.ToString());
            }
            catch (InvalidOperationException e)
            {
                throw new FormatException(e.Message);
            }
        }

        /// <summary>
        ///   Read a resource record.
        /// </summary>
        /// <returns>
        ///   A <see cref="ResourceRecord"/> or <b>null</b> if no more
        ///   resource records are available.
        /// </returns>
        /// <remarks>
        ///   Processes the "$ORIGIN" and "$TTL" specials that define the
        ///   <see cref="Origin"/> and a default time-to-live respectively.
        ///   <para>
        ///   A domain name can be "@" to refer to the <see cref="Origin"/>. 
        ///   A missing domain name will use the previous record's domain name.
        ///   </para>
        ///   <para>
        ///   Defaults the <see cref="ResourceRecord.Class"/> to <see cref="DnsClass.IN"/>.
        ///   Defaults the <see cref="ResourceRecord.TTL"/>  to either the "$TTL" or
        ///   the <see cref="ResourceRecord.DefaultTTL"/>.
        ///   </para>
        /// </remarks>
        public ResourceRecord ReadResourceRecord()
        {
            string domainName = defaultDomainName;
            DnsClass klass = DnsClass.IN;
            TimeSpan? ttl = defaultTTL;
            DnsType? type = null;

            while (!type.HasValue)
            {
                var token = ReadToken();
                if (token == "")
                {
                    return null;
                }

                // Is origin?
                if (token == "$ORIGIN")
                {
                    Origin = ReadToken();
                    continue;
                }
                if (token == "@")
                {
                    domainName = Origin;
                    continue;
                }

                // Is TTL?
                if (token == "$TTL")
                {
                    defaultTTL = ttl = ReadTimeSpan32();
                    continue;
                }
                if (token.All(c => Char.IsDigit(c)))
                {
                    ttl = TimeSpan.FromSeconds(uint.Parse(token));
                    continue;
                }

                // Is TYPE?
                if (token.StartsWith("TYPE"))
                {
                    type = (DnsType)ushort.Parse(token.Substring(4), CultureInfo.InvariantCulture);
                    continue;
                }
                if (token.ToLowerInvariant() != "any" && Enum.TryParse<DnsType>(token, out DnsType t))
                {
                    type = t;
                    continue;
                }

                // Is CLASS?
                if (token.StartsWith("CLASS"))
                {
                    klass = (DnsClass)ushort.Parse(token.Substring(5), CultureInfo.InvariantCulture);
                    continue;
                }
                if (Enum.TryParse<DnsClass>(token, out DnsClass k))
                {
                    klass = k;
                    continue;
                }

                // Must be domain name.
                domainName = token;
            }

            // TODO: Check attributes

            defaultDomainName = domainName;

            // Create the specific resource record based on the type.
            var resource = ResourceRegistry.Create(type.Value);
            resource.Name = MakeAbsoluteDomainName(domainName);
            resource.Type = type.Value;
            resource.Class = klass;
            if (ttl.HasValue)
            {
                resource.TTL = ttl.Value;
            }

            // Read the specific properties of the resource record.
            resource.ReadData(this);

            return resource;

        }

        /// <summary>
        ///   Determines if the reader is at the end of a line.
        /// </summary>
        public bool IsEndOfLine()
        {
            int c;
            while (parenLevel > 0)
            {
                while ((c = text.Peek()) >= 0)
                {
                    if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                    {
                        text.Read();
                        continue;
                    }
                    if (c == ')')
                    {
                        --parenLevel;
                        text.Read();
                        break;
                    }
                    return false;
                }

            }

            while ((c = text.Peek()) >= 0)
            {
                if (c == ' ' || c == '\t')
                {
                    text.Read();
                    continue;
                }

                if (c == '\r' || c == '\n' || c == ';')
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        string ReadToken()
        {
            var sb = new StringBuilder();
            int c;
            bool skipWhitespace = true;
            bool inquote = false;
            bool incomment = false;
            while ((c = text.Read()) >= 0)
            {
                // Comments are terminated by a newline.
                if (incomment)
                {
                    if (c == '\r' || c == '\n')
                    {
                        incomment = false;
                        skipWhitespace = true;
                    }
                    continue;
                }

                // Handle escaped character.
                if (c == '\\')
                {
                    c = text.Read();
                    // TODO: \DDD
                    sb.Append((char)c);
                    skipWhitespace = false;
                    continue;
                }

                // Handle quoted strings.
                if (inquote)
                {
                    if (c == '"')
                    {
                        inquote = false;
                        break;
                    }
                    else
                    {
                        sb.Append((char)c);
                    }
                    continue;
                }
                if (c == '"')
                {
                    inquote = true;
                    continue;
                }

                // Ignore parens.
                if (c == '(')
                {
                    ++parenLevel;
                    c = ' ';
                }
                if (c == ')')
                {
                    --parenLevel;
                    c = ' ';
                }

                // Skip whitespace.
                if (skipWhitespace)
                {
                    if (Char.IsWhiteSpace((char)c))
                    {
                        continue;
                    }
                    skipWhitespace = false;
                }
                if (Char.IsWhiteSpace((char)c))
                {
                    break;
                }

                // Handle start of comment.
                if (c == ';')
                {
                    incomment = true;
                    continue;

                }

                sb.Append((char)c);
            }

            return sb.ToString();
        }

    }
}
