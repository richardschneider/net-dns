﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Allows administrators to use several servers for a single domain.
    /// </summary>
    public class SRVRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="SRVRecord"/> class.
        /// </summary>
        public SRVRecord() : base()
        {
            Type = DnsType.SRV;
        }

        /// <summary>
        ///  The priority of this target host.
        /// </summary>
        /// <remarks>
        ///  A client MUST attempt to contact the target host with the 
        ///  lowest-numbered priority it can
        ///  reach; target hosts with the same priority SHOULD be tried in an
        ///  order defined by the weight field.The range is 0-65535. 
        /// </remarks>
        public ushort Priority { get; set; }

        /// <summary>
        ///   A server selection mechanism.
        /// </summary>
        /// <remarks>
        ///   The weight field specifies a
        ///   relative weight for entries with the same priority.Larger
        ///   weights SHOULD be given a proportionately higher probability of
        ///   being selected.
        /// </remarks>
        public ushort Weight { get; set; }

        /// <summary>
        ///   The port on this target host of this service.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        ///   The domain name of the target host.
        /// </summary>
        /// <remarks>
        ///   There MUST be one or more
        ///   address records for this name, the name MUST NOT be an alias (in
        ///   the sense of RFC 1034 or RFC 2181).
        /// </remarks>
        public string Target { get; set; }

        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            Priority = reader.ReadUInt16();
            Weight = reader.ReadUInt16();
            Port = reader.ReadUInt16();
            Target = reader.ReadDomainName();
        }

        /// <inheritdoc />
        internal override void ReadData(MasterReader reader)
        {
            Priority = reader.ReadUInt16();
            Weight = reader.ReadUInt16();
            Port = reader.ReadUInt16();
            Target = reader.ReadDomainName();
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteUInt16(Priority);
            writer.WriteUInt16(Weight);
            writer.WriteUInt16(Port);
            writer.WriteDomainName(Target);
        }

        /// <inheritdoc />
        protected override void WriteData(TextWriter writer)
        {
            writer.Write(Priority);
            writer.Write(' ');
            writer.Write(Weight);
            writer.Write(' ');
            writer.Write(Port);
            writer.Write(' ');
            writer.Write(Target);
        }

    }
}
