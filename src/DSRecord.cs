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
        ///   Creates a new instance of the <see cref="DSRecord"/> class
        ///   from the specified <see cref="DNSKEYRecord"/>.
        /// </summary>
        /// <param name="key">
        ///   The dns key to use.
        /// </param>
        /// <param name="digestType">
        ///   The digest algorithm to use.  Defaults to <see cref="DigestType.Sha1"/>.
        /// </param>
        public DSRecord(DNSKEYRecord key, DigestType digestType = DigestType.Sha1) 
            : this()
        {
            byte[] digest;
            using (var ms = new MemoryStream())
            using (var hasher = DigestRegistry.Create(digestType))
            {
                var writer = new DnsWriter(ms) { CanonicalForm = true };
                writer.WriteDomainName(key.Name);
                key.WriteData(writer);
                ms.Position = 0;
                digest = hasher.ComputeHash(ms);
            }
            Algorithm = key.Algorithm;
            Class = key.Class;
            KeyTag = key.KeyTag();
            Name = key.Name;
            TTL = key.TTL;
            Digest = digest;
            HashAlgorithm = DigestType.Sha1;
        }

        /// <summary>
        ///   The tag of the referenced <see cref="DNSKEYRecord"/>.
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
        public override void ReadData(DnsReader reader, int length)
        {
            var end = reader.Position + length;

            KeyTag = reader.ReadUInt16();
            Algorithm = (SecurityAlgorithm)reader.ReadByte();
            HashAlgorithm = (DigestType)reader.ReadByte();
            Digest = reader.ReadBytes(end - reader.Position);
        }

        /// <inheritdoc />
        public override void WriteData(DnsWriter writer)
        {
            writer.WriteUInt16(KeyTag);
            writer.WriteByte((byte)Algorithm);
            writer.WriteByte((byte)HashAlgorithm);
            writer.WriteBytes(Digest);
        }

        /// <inheritdoc />
        public override void ReadData(MasterReader reader)
        {
            KeyTag = reader.ReadUInt16();
            Algorithm = (SecurityAlgorithm)reader.ReadByte();
            HashAlgorithm = (DigestType)reader.ReadByte();

            // Whitespace is allowed within the hexadecimal text.
            var sb = new StringBuilder();
            while (!reader.IsEndOfLine())
            {
                sb.Append(reader.ReadString());
            }
            Digest = Base16.Decode(sb.ToString());
        }

        /// <inheritdoc />
        public override void WriteData(TextWriter writer)
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
