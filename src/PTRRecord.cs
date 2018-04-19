using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   A domain name pointer.
    /// </summary>
    /// <remarks>
    ///  PTR records cause no additional section processing.  These RRs are used
    ///  in special domains to point to some other location in the domain space.
    ///  These records are simple data, and don't imply any special processing
    ///  similar to that performed by CNAME, which identifies aliases.See the
    ///  description of the IN-ADDR.ARPA domain for an example.
    /// </remarks>
    public class PTRRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="PTRRecord"/> class.
        /// </summary>
        public PTRRecord() : base()
        {
            Type = 12;
        }

        /// <summary>
        ///  A domain-name which points to some location in the
        ///  domain name space.
        /// </summary>
        public string DomainName { get; set; }


        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            DomainName = reader.ReadDomainName();
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteDomainName(DomainName);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var that = obj as PTRRecord;
            if (that == null) return false;

            return base.Equals(obj) 
                && this.DomainName == that.DomainName;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode()
                ^ DomainName?.GetHashCode() ?? 0;
        }


    }
}
