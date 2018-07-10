﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Contains the the next owner name and the set of RR
    ///   types present at the NSEC RR's owner name [RFC3845].  T
    /// </summary>
    public class NSECRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="NSECRecord"/> class.
        /// </summary>
        public NSECRecord() : base()
        {
            Type = DnsType.NSEC;
        }

        /// <summary>
        ///   The next owner name that has authoritative data or contains a
        ///   delegation point NS RRset
        /// </summary>
        public string NextOwnerName { get; set; }

        /// <summary>
        ///   Identifies the RRset types that exist at the NSEC RR's owner name.
        /// </summary>
        public byte[] TypeBitmaps { get; set; }

        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            NextOwnerName = reader.ReadDomainName();
            var tbLength = reader.ReadUInt16();
            TypeBitmaps = reader.ReadBytes(tbLength);
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteDomainName(NextOwnerName);
            writer.WriteUInt16((ushort)TypeBitmaps.Length);
            writer.WriteBytes(TypeBitmaps);
        }
    }
}
