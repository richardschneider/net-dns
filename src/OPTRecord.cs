﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   An extension mechanism for DNS (EDNS(0)).
    /// </summary>
    /// <remarks>
    ///   An OPT record does not carry any DNS data. It is used only to
    ///   contain control information pertaining to the question-and-answer
    ///   sequence of a specific transaction. OPT RRs MUST NOT be cached,
    ///   forwarded, or stored in or loaded from master files.
    ///   <para>
    ///   The <b>OPTRecord</b> can be present in the <see cref="Message.AdditionalRecords"/>
    ///   section.
    ///   </para>
    ///   <note>
    ///   The <see cref="ResourceRecord.Class"/> property is repurposed to specify
    ///   the requestor's payload size.
    ///   </note>
    ///   <note>
    ///   The <see cref="ResourceRecord.TTL"/> property is repurposed to specify
    ///   the <see cref="Opcode8"/>, <see cref="Version"/> and <see cref="DO"/> properties.
    ///   </note>
    /// </remarks>
    /// <seealso href="https://tools.ietf.org/html/rfc6891"/>
    public class OPTRecord : ResourceRecord
    {

        /// <summary>
        ///   Creates a new instance of the <see cref="OPTRecord"/> class.
        /// </summary>
        public OPTRecord() : base()
        {
            Type = DnsType.OPT;
            Name = "";
            RequestorPayloadSize = 1280;
            TTL = TimeSpan.Zero;
        }

        /// <summary>
        ///   The maximimum packet size that can be received by the requestor.
        /// </summary>
        /// <value>
        ///   Specified in number of bytes. Defaults to 1280, which is reasonable over Ethernet.
        /// </value>
        /// <remarks>
        ///   The <see cref="ResourceRecord.Class"/> property is repurposed to specify
        ///   the requestor's payload size.
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc6891#section-6.2.3"/>
        public ushort RequestorPayloadSize
        {
            get { return (ushort)Class; }
            set { Class = (Class)value; }
        }

        /// <summary>
        ///   The most significant 8 bits of the opcode.
        /// </summary>
        /// <value>
        ///   Defaults to zero.
        /// </value>
        /// <remarks>
        ///   The <see cref="ResourceRecord.TTL"/> property is repurposed to specify
        ///   the opcode's most significant bits.
        /// </remarks>
        /// <seealso cref="Message.Opcode"/>
        public byte Opcode8
        {
            get { return (byte)(((TTL.Ticks / TimeSpan.TicksPerSecond) >> 24) & 0xff); }
            set
            {
                TTL = TimeSpan.FromTicks(
                    (((TTL.Ticks / TimeSpan.TicksPerSecond) & ~0xff000000L)
                    | ((long)value << 24))
                    * TimeSpan.TicksPerSecond);
            }
        }

        /// <summary>
        ///   The EDNS version.
        /// </summary>
        /// <value>
        ///   Defaults to zero.
        /// </value>
        /// <remarks>
        ///   The <see cref="ResourceRecord.TTL"/> property is repurposed to specify
        ///   the version.
        /// </remarks>
        public byte Version
        {
            get { return (byte)(((TTL.Ticks / TimeSpan.TicksPerSecond) >> 16) & 0xff); }
            set
            {
                TTL = TimeSpan.FromTicks(
                    (((TTL.Ticks / TimeSpan.TicksPerSecond) & ~0xff0000L)
                    | ((long)value << 16))
                    * TimeSpan.TicksPerSecond);
            }
        }

        /// <summary>
        ///   The DNSSEC OK bit as defined by [RFC3225].
        /// </summary>
        /// <value>
        ///   Defaults to <b>false</b>.
        /// </value>
        /// <remarks>
        ///   The <see cref="ResourceRecord.TTL"/> property is repurposed to specify
        ///   the version.
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc3225"/>
        public bool DO
        {
            get { return (TTL.Ticks / TimeSpan.TicksPerSecond) == 0x8000L; }
            set
            {
                TTL = TimeSpan.FromTicks(
                    (((TTL.Ticks / TimeSpan.TicksPerSecond) & ~0x8000L)
                    | (Convert.ToInt64(value) << 15))
                    * TimeSpan.TicksPerSecond);
            }
        }

        /// <summary>
        ///   The extended DNS options.
        /// </summary>
        /// <value>
        ///   The EDNS option sequence.
        /// </value>
        public List<EdnsOption> Options { get; set; } = new List<EdnsOption>();

        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            var end = reader.Position + length;
            while (reader.Position < end)
            {
                var type = (EdnsOptionType)reader.ReadUInt16();
                int olength = reader.ReadUInt16();

                EdnsOption option;
                if (EdnsOptionRegistry.Options.TryGetValue(type, out Func<EdnsOption> maker))
                {
                    option = maker();
                }
                else
                {
                    option = new UnknownEdnsOption { Type = type };
                }
                Options.Add(option);
                option.ReadData(reader, olength);
            }
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            foreach (var option in Options)
            {
                writer.WriteUInt16((ushort)option.Type);

                writer.PushLengthPrefixedScope();
                option.WriteData(writer);
                writer.PopLengthPrefixedScope();
            }
        }

    }
}
