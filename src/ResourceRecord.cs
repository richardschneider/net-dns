using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Contains some information on the named resource.
    /// </summary>
    /// <remarks>
    ///   The <see cref="ResourceRegistry"/> contains the metadata on known
    ///   resource records. When reading, if the registry does not contain
    ///   the record, then an <see cref="UnknownRecord"/> is used.
    /// </remarks>
    public class ResourceRecord : DnsObject, IMasterSerialiser
    {
        /// <summary>
        ///   The default time interval that a resource record maybe cached.
        /// </summary>
        /// <value>
        ///   Defaults to 1 day.
        /// </value>
        public static TimeSpan DefaultTTL = TimeSpan.FromDays(1);

        /// <summary>
        ///   The default time interval that a resource record containing
        ///   a host name maybe cached.
        /// </summary>
        /// <value>
        ///   Defaults to 1 day.
        /// </value>
        /// <remarks>
        ///   Host names are in A, AAAA, and HINFO records.
        /// </remarks>
        public static TimeSpan DefaultHostTTL = TimeSpan.FromDays(1);

        /// <summary>
        ///   An owner name, i.e., the name of the node to which this
        ///   resource record pertains.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///    One of the RR TYPE codes.
        /// </summary>
        public DnsType Type { get; set; }

        /// <summary>
        ///    One of the RR CLASS codes.
        /// </summary>
        /// <value>
        ///   Defaults to <see cref="Class.IN"/>.
        /// </value>
        public Class Class { get; set; } = Class.IN;

        /// <summary>
        ///    Specifies the time interval
        ///    that the resource record may be cached before the source
        ///    of the information should again be consulted. 
        /// </summary>
        /// <value>
        ///    The resolution is 1 second. Defaults to 1 day.
        /// </value>
        /// <remarks>
        ///    Zero values are interpreted to mean that the RR can only be
        ///    used for the transaction in progress, and should not be
        ///    cached.
        ///    
        ///    For example, SOA records are always distributed
        ///    with a zero TTL to prohibit caching. Zero values can
        ///    also be used for extremely volatile data.
        /// </remarks>
        /// <seealso cref="DefaultTTL"/>
        public TimeSpan TTL { get; set; } = DefaultTTL;

        /// <summary>
        ///   The length of the resource specific data.
        /// </summary>
        /// <returns>
        ///   Number of bytes to represent the resource specific data.
        /// </returns>
        /// <remarks>
        ///   This is referred to as the <c>RDLENGTH</c> in the DNS spec.
        /// </remarks>
        public int GetDataLength()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new DnsWriter(ms);
                this.WriteData(writer);
                return (int) ms.Length;
            }
        }

        /// <summary>
        ///   The resource specific data.
        /// </summary>
        /// <returns>
        ///   A byte array, never <b>null</b>.
        /// </returns>
        /// <remarks>
        ///   This is referred to as the <c>RDATA</c> in the DNS spec.
        /// </remarks>
        public byte[] GetData()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new DnsWriter(ms);
                this.WriteData(writer);
                return ms.ToArray();
            }
        }

        /// <inheritdoc />
        public override IDnsSerialiser Read(DnsReader reader)
        {
            // Read standard properties of a resource record.
            Name = reader.ReadDomainName();
            Type = (DnsType)reader.ReadUInt16();
            Class = (Class)reader.ReadUInt16();
            TTL = reader.ReadTimeSpan();
            int length = reader.ReadUInt16();

            // Find a specific class for the TYPE or default
            // to UnknownRecord.
            ResourceRecord specific;
            if (ResourceRegistry.Records.TryGetValue(Type, out Func<ResourceRecord> maker))
            {
                specific = maker();
            }
            else
            {
                specific = new UnknownRecord();
            }
            specific.Name = Name;
            specific.Type = Type;
            specific.Class = Class;
            specific.TTL = TTL;

            // Read the specific properties of the resource record.
            specific.ReadData(reader, length);

            return specific;
        }

        /// <summary>
        ///   Read the data that is specific to the resource record <see cref="System.Type"/>.
        /// </summary>
        /// <param name="reader">
        ///   The source of the DNS object's data.
        /// </param>
        /// <param name="length">
        ///   The length, in bytes, of the data.
        /// </param>
        /// <remarks>
        ///   Derived classes must implement this method.
        /// </remarks>
        protected virtual void ReadData(DnsReader reader, int length)
        {
        }

        /// <inheritdoc />
        public override void Write(DnsWriter writer)
        {
            writer.WriteDomainName(Name);
            writer.WriteUInt16((ushort)Type);
            writer.WriteUInt16((ushort)Class);
            writer.WriteTimeSpan(TTL);

            writer.PushLengthPrefixedScope();
            WriteData(writer);
            writer.PopLengthPrefixedScope();
        }

        /// <summary>
        ///   Write the data that is specific to the resource record <see cref="System.Type"/>.
        /// </summary>
        /// <param name="writer">
        ///   The destination for the DNS object's data.
        /// </param>
        /// <remarks>
        ///   Derived classes must implement this method.
        /// </remarks>
        protected virtual void WriteData(DnsWriter writer)
        {
        }

        /// <summary>
        ///   Determines if the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">
        ///   The object to compare.
        /// </param>
        /// <returns>
        ///   <b>true</b> if the specified object is equal to the current object; otherwise, <b>false</b>.
        /// </returns>
        /// <remarks>
        ///   Two Resource Records are considered equal if their <see cref="Name"/>, 
        ///   <see cref="Class"/>, <see cref="Type"/> and <see cref="GetData">data fields</see>
        ///   are equal. Note that the <see cref="TTL"/> field is explicitly 
        ///   excluded from the comparison.
        /// </remarks>
        public override bool Equals(object obj)
        {
            var that = obj as ResourceRecord;
            if (that == null) return false;

            if (!DnsObject.NamesEquals(this.Name, that.Name)) return false;
            if (this.Class != that.Class) return false;
            if (this.Type != that.Type) return false;

            return this.GetData().SequenceEqual(that.GetData());
        }

        /// <summary>
        ///   Value equality.
        /// </summary>
        /// <remarks>
        ///   Two Resource Records are considered equal if their <see cref="Name"/>, 
        ///   <see cref="Class"/>, <see cref="Type"/> and data fields
        ///   are equal. Note that the <see cref="TTL"/> field is explicitly 
        ///   excluded from the comparison.
        /// </remarks>
        public static bool operator ==(ResourceRecord a, ResourceRecord b)
        {
            if (object.ReferenceEquals(a, b)) return true;
            if (object.ReferenceEquals(a, null)) return false;
            if (object.ReferenceEquals(b, null)) return false;

            return a.Equals(b);
        }

        /// <summary>
        ///   Value inequality.
        /// </summary>
        /// <remarks>
        ///   Two Resource Records are considered equal if their <see cref="Name"/>, 
        ///   <see cref="Class"/>, <see cref="Type"/> and data fields
        ///   are equal. Note that the <see cref="TTL"/> field is explicitly 
        ///   excluded from the comparison.
        /// </remarks>
        public static bool operator !=(ResourceRecord a, ResourceRecord b)
        {
            if (object.ReferenceEquals(a, b)) return false;
            if (object.ReferenceEquals(a, null)) return true;
            if (object.ReferenceEquals(b, null)) return true;

            return !a.Equals(b);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return 
                Name?.ToLowerInvariant().GetHashCode() ?? 0
                ^ Class.GetHashCode()
                ^ Type.GetHashCode()
                ^ GetData().Aggregate(0, (r, b) => r ^ b.GetHashCode());

        }


        /// <summary>
        ///   Returns the textual representation.
        /// </summary>
        /// <returns>
        ///   The "master file format" of this resource record. 
        /// </returns>
        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                Write(writer);

                // Trim trailing whitespaces (tab, space, cr, lf, ...)
                var sb = writer.GetStringBuilder();
                while (sb.Length > 0 && Char.IsWhiteSpace(sb[sb.Length-1]))
                {
                    --sb.Length;
                }

                return sb.ToString();
            }
        }

        /// <inheritdoc />
        public void Write(TextWriter writer)
        {
            writer.Write(Name);
            writer.Write(' ');
            if (TTL != DefaultTTL)
            {
                writer.Write((int)TTL.TotalSeconds);
                writer.Write(' ');
            }
            writer.Write(Class);
            writer.Write(' ');
            writer.Write(Type);
            writer.Write(' ');
            WriteData(writer);
            writer.Write("\r\n");
        }

        /// <summary>
        ///   Write the textual representation of the data that is specific to 
        ///   the resource record.
        /// </summary>
        /// <param name="writer">
        ///   The destination for the resource record's data.
        /// </param>
        /// <remarks>
        ///   Derived classes should implement this method.
        ///   <para>
        ///   By default, this will write the base64 encoding of
        ///   the <see cref="GetData">RDATA</see>.
        ///   </para>
        /// </remarks>
        protected virtual void WriteData(TextWriter writer)
        {
            writer.Write(Convert.ToBase64String(GetData()));
        }

    }
}
