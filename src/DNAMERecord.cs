using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Alias for a name and all its subnames.
    /// </summary>
    /// <remarks>
    ///  Alias for a name and all its subnames, unlike <see cref="CNAMERecord"/>, which is an 
    ///  alias for only the exact name. Like a CNAME record, the DNS lookup will continue by 
    ///  retrying the lookup with the new name.
    /// </remarks>
    public class DNAMERecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="DNAMERecord"/> class.
        /// </summary>
        public DNAMERecord() : base()
        {
            Type = DnsType.DNAME;
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
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteDomainName(Target, uncompressed: true);
        }

    }
}
