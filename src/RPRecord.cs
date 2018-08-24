using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   The person responsible for a name.
    /// </summary>
    /// <remarks>
    ///  The responsible person identification to any name in the DNS.
    /// </remarks>
    /// <seealso href="https://tools.ietf.org/html/rfc1183"/>
    public class RPRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="RPRecord"/> class.
        /// </summary>
        public RPRecord() : base()
        {
            Type = DnsType.RP;
        }

        /// <summary>
        ///   The mailbox for the responsible person.
        /// </summary>
        /// <value>
        ///   Defaults to "".
        /// </value>
        public string Mailbox { get; set; } = "";

        /// <summary>
        ///   The name of TXT records for the responsible person.
        /// </summary>
        /// <value>
        ///   Defaults to "".
        /// </value>
        public string TextName { get; set; } = "";

        /// <inheritdoc />
        public override void ReadData(DnsReader reader, int length)
        {
            Mailbox = reader.ReadDomainName();
            TextName = reader.ReadDomainName();
        }

        /// <inheritdoc />
        public override void ReadData(MasterReader reader)
        {
            Mailbox = reader.ReadDomainName();
            TextName = reader.ReadDomainName();
        }

        /// <inheritdoc />
        public override void WriteData(DnsWriter writer)
        {
            writer.WriteDomainName(Mailbox);
            writer.WriteDomainName(TextName);
        }

        /// <inheritdoc />
        public override void WriteData(TextWriter writer)
        {
            writer.Write(Mailbox);
            writer.Write(' ');
            writer.Write(TextName);
        }

    }
}
