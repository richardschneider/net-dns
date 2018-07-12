using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   The canonical name for an alias.
    /// </summary>
    /// <remarks>
    ///  CNAME RRs cause no additional section processing, but name servers may
    ///  choose to restart the query at the canonical name in certain cases. See
    ///  the description of name server logic in [RFC - 1034] for details.
    /// </remarks>
    public class CNAMERecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="CNAMERecord"/> class.
        /// </summary>
        public CNAMERecord() : base()
        {
            Type = DnsType.CNAME;
        }

        /// <summary>
        ///  A domain-name which specifies the canonical or primary
        ///  name for the owner. The owner name is an alias.
        /// </summary>
        public string Target { get; set; }


        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            Target = reader.ReadDomainName();
        }

        /// <inheritdoc />
        internal override void ReadData(MasterReader reader)
        {
            Target = reader.ReadDomainName();
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteDomainName(Target);
        }

        /// <inheritdoc />
        protected override void WriteData(TextWriter writer)
        {
            writer.Write(Target);
        }

    }
}
