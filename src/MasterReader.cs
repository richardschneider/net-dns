using System;
using System.Collections.Generic;
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
        TextReader text;
        TimeSpan? defaultTTL = null;
        string defaultDomainName = null;

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
            return ReadToken();
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
        ///   Read a time span (interval)
        /// </summary>
        /// <returns>
        ///   A <see cref="TimeSpan"/> with second resolution.
        /// </returns>
        public TimeSpan ReadTimeSpan()
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
        ///   Defaults the <see cref="ResourceRecord.Class"/> to <see cref="Class.IN"/>.
        ///   Defaults the <see cref="ResourceRecord.TTL"/>  to either the "$TTL" or
        ///   the <see cref="ResourceRecord.DefaultTTL"/>.
        ///   </para>
        /// </remarks>
        public ResourceRecord ReadResourceRecord()
        {
            string domainName = defaultDomainName;
            Class klass = Class.IN;
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
                    defaultTTL = ttl = ReadTimeSpan();
                    continue;
                }
                if (Char.IsDigit(token[0]))
                {
                    ttl = TimeSpan.FromSeconds(uint.Parse(token));
                    continue;
                }

                // Is TYPE?
                if (Enum.TryParse<DnsType>(token, out DnsType t))
                {
                    type = t;
                    continue;
                }

                // Is CLASS?
                if (Enum.TryParse<Class>(token, out Class k))
                {
                    klass = k;
                    continue;
                }

                // Must be domain name
                domainName = token;
            }

            // TODO: Check attributes

            defaultDomainName = domainName;

            // Create the specific resource record based on the type.
            ResourceRecord resource;
            if (ResourceRegistry.Records.TryGetValue(type.Value, out Func<ResourceRecord> maker))
            {
                resource = maker();
            }
            else
            {
                resource = new UnknownRecord();
            }
            resource.Name = domainName;
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
            // TODO: Handle parens
            int c;
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
                if (c == '(' || c == ')')
                {
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

                // Handle escaped character.
                // TODO: \DDD
                if (c == '\\')
                {
                    c = text.Read();
                }

                sb.Append((char)c);
            }

            return sb.ToString();
        }

    }
}
