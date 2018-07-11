﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Mail exchange.
    /// </summary>
    /// <remarks>
    ///   MX records cause type A additional section processing for the host
    ///   specified by EXCHANGE.The use of MX RRs is explained in detail in
    ///   [RFC-974].
    /// </remarks>
    public class MXRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="MXRecord"/> class.
        /// </summary>
        public MXRecord() : base()
        {
            Type = DnsType.MX;
        }

        /// <summary>
        ///  The preference given to this RR among others at the same owner. 
        /// </summary>
        /// <value>
        ///   Lower values are preferred.
        /// </value>
        public ushort Preference { get; set; }

        /// <summary>
        ///  A domain-name which specifies a host willing to act as
        ///  a mail exchange for the owner name.
        /// </summary>
        /// <value>
        ///   The name of an mail exchange.
        /// </value>
        public string Exchange { get; set; }


        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            Preference = reader.ReadUInt16();
            Exchange = reader.ReadDomainName();
        }

        /// <inheritdoc />
        internal override void ReadData(MasterReader reader)
        {
            Preference = reader.ReadUInt16();
            Exchange = reader.ReadDomainName();
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteUInt16(Preference);
            writer.WriteDomainName(Exchange);
        }

        /// <inheritdoc />
        protected override void WriteData(TextWriter writer)
        {
            writer.Write(Preference);
            writer.Write(' ');
            writer.Write(Exchange);
        }

    }
}
