using SimpleBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Public key cryptography to sign and authenticate resource records.
    /// </summary>
    public class DNSKEYRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="DNSKEYRecord"/> class.
        /// </summary>
        public DNSKEYRecord() : base()
        {
            Type = DnsType.DNSKEY;
        }

        /// <summary>
        ///  TODO
        /// </summary>
        public ushort Flags { get; set; }

        /// <summary>
        ///   Must be three.
        /// </summary>
        /// <value>
        ///   Defaults to 3.
        /// </value>
        public byte Protocol { get; set; } = 3;

        /// <summary>
        ///   Identifies the public key's cryptographic algorithm
        /// </summary>
        /// <remarks>
        ///    Determines the format of the<see cref="PublicKey"/>.
        /// </remarks>
        public SecurityAlgorithm Algorithm { get; set; }

        /// <summary>
        ///   The public key material.
        /// </summary>
        /// <remarks>
        ///   The format depends on the <see cref="Algorithm"/> of the key being stored.
        /// </remarks>
        public byte[] PublicKey { get; set; }

        /// <summary>
        ///   Calculates the key tag.
        /// </summary>
        /// <remarks>
        ///   <see href="https://tools.ietf.org/html/rfc4034#appendix-B"/> for the details.
        /// </remarks>
        public ushort KeyTag()
        {
            var key = this.GetData();
            var length = key.Length;
            int ac = 0;

            for (var i = 0; i < length; ++i)
            {
                ac += (i & 1) == 1 ? key[i] : key[i] << 8;
            }
            ac += (ac >> 16) & 0xFFFF;
            return (ushort) (ac & 0xFFFF);
        }

        /// <summary>
        ///   Create a delegation signer from the key.
        /// </summary>
        /// <returns>
        ///   A <see cref="DSRecord"/> that refers to this key.
        /// </returns>
        public DSRecord CreateDSRecord()
        {
            byte[] digest;
            using (var ms = new MemoryStream())
            using (var hasher = SHA1.Create())
            {
                var writer = new DnsWriter(ms);
                writer.WriteDomainName(CanonicalName, uncompressed: true);
                this.WriteData(writer);
                ms.Position = 0;
                digest = hasher.ComputeHash(ms);
            }
            return new DSRecord
            {
                Algorithm = Algorithm,
                Class = Class,
                Digest = digest,
                HashAlgorithm = DigestType.Sha1,
                KeyTag = KeyTag(),
                Name = Name,
                TTL = TTL
            };
        }

        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            var end = reader.Position + length;

            Flags = reader.ReadUInt16();
            Protocol = reader.ReadByte();
            Algorithm = (SecurityAlgorithm)reader.ReadByte();
            PublicKey = reader.ReadBytes(end - reader.Position);
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteUInt16(Flags);
            writer.WriteByte(Protocol);
            writer.WriteByte((byte)Algorithm);
            writer.WriteBytes(PublicKey);
        }

        internal override void ReadData(MasterReader reader)
        {
            Flags = reader.ReadUInt16();
            Protocol = reader.ReadByte();
            Algorithm = (SecurityAlgorithm)reader.ReadByte();
            PublicKey = reader.ReadBase64String();
        }

        /// <inheritdoc />
        protected override void WriteData(TextWriter writer)
        {
            writer.Write(Flags);
            writer.Write(' ');
            writer.Write(Protocol);
            writer.Write(' ');
            writer.Write((byte)Algorithm);
            writer.Write(' ');
            writer.Write(Convert.ToBase64String(PublicKey));
        }
    }
}
