using SimpleBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Delegation Signer.
    /// </summary>
    public class DSRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="DSRecord"/> class.
        /// </summary>
        public DSRecord() : base()
        {
            Type = DnsType.DS;
        }

        /// <summary>
        ///   The ID of the referenced <see cref="DNSKEYRecord"/>.
        /// </summary>
        public ushort KeyTag { get; set; }

        /// <summary>
        ///   The <see cref="SecurityAlgorithm"/> of the referenced <see cref="DNSKEYRecord"/>.
        /// </summary>
        public SecurityAlgorithm Algorithm {get; set; }

        /// <summary>
        ///   The cryptographic hash algorithm used to create the 
        ///   <see cref="Digest"/>.
        /// </summary>
        /// <value>
        ///   One of the <see cref="DigestType"/> value.
        /// </value>
        public DigestType HashAlgorithm { get; set; }

        /// <summary>
        ///   The digest of the referenced <see cref="DNSKEYRecord"/>.
        /// </summary>
        /// <remarks>
        ///   <c>digest = HashAlgorithm(DNSKEY owner name | DNSKEY RDATA)</c>
        /// </remarks>
        public byte[] Digest { get; set; }

        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            var end = reader.Position + length;

            KeyTag = reader.ReadUInt16();
            Algorithm = (SecurityAlgorithm)reader.ReadByte();
            HashAlgorithm = (DigestType)reader.ReadByte();
            Digest = reader.ReadBytes(end - reader.Position);
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteUInt16(KeyTag);
            writer.WriteByte((byte)Algorithm);
            writer.WriteByte((byte)HashAlgorithm);
            writer.WriteBytes(Digest);
        }

        internal override void ReadData(MasterReader reader)
        {
            KeyTag = reader.ReadUInt16();
            Algorithm = (SecurityAlgorithm)reader.ReadByte();
            HashAlgorithm = (DigestType)reader.ReadByte();
            // TODO: Whitespace is allowed within the hexadecimal text.
            Digest = Base16.Decode(reader.ReadString());
        }

        /// <inheritdoc />
        protected override void WriteData(TextWriter writer)
        {
            writer.Write(KeyTag);
            writer.Write(' ');
            writer.Write((byte)Algorithm);
            writer.Write(' ');
            writer.Write((byte)HashAlgorithm);
            writer.Write(' ');
            writer.Write(Base16.EncodeLower(Digest));
        }
    }
}
