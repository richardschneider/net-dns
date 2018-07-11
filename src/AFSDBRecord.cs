﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Andrew File System Database.
    /// </summary>
    /// <remarks>
    ///   Maps a domain name to the name of an AFS cell database server.
    /// </remarks>
    /// <seealso href="https://tools.ietf.org/html/rfc1183"/>
    public class AFSDBRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="AFSDBRecord"/> class.
        /// </summary>
        public AFSDBRecord() : base()
        {
            Type = DnsType.AFSDB;
        }

        /// <summary>
        ///  A 16 bit integer which specifies the type of AFS server.
        /// </summary>
        /// <value>
        ///   See <see href="https://tools.ietf.org/html/rfc1183#section-1"/>
        /// </value>
        public ushort Subtype { get; set; }

        /// <summary>
        ///  A domain-name which specifies a host running an AFS server.
        /// </summary>
        /// <value>
        ///   The name of an AFS server.
        /// </value>
        public string Target { get; set; }


        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            Subtype = reader.ReadUInt16();
            Target = reader.ReadDomainName();
        }

        /// <inheritdoc />
        internal override void ReadData(MasterReader reader)
        {
            Subtype = reader.ReadUInt16();
            Target = reader.ReadDomainName();
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteUInt16(Subtype);
            writer.WriteDomainName(Target);
        }

        /// <inheritdoc />
        protected override void WriteData(TextWriter writer)
        {
            writer.Write(Subtype);
            writer.Write(' ');
            writer.Write(Target);
        }

    }
}
