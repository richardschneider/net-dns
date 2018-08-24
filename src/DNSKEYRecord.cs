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
        ///   Creates a new instance of the <see cref="DNSKEYRecord"/> class
        ///   from the specified RSA key.
        /// </summary>
        /// <param name="key">
        ///   A public or private RSA key.
        /// </param>
        /// <param name="algorithm">
        ///   The security algorithm to use.  Only RSA types are allowed.
        /// </param>
        public DNSKEYRecord(RSA key, SecurityAlgorithm algorithm)
            : this()
        {
            Flags = 256; // TODO: define an enum
            Algorithm = algorithm; // TODO check for RSA algorithm

            using (var ms = new MemoryStream())
            {
                var p = key.ExportParameters(includePrivateParameters: false);
                // TODO: length > 255
                ms.WriteByte((byte)p.Exponent.Length);
                ms.Write(p.Exponent, 0, p.Exponent.Length);
                ms.Write(p.Modulus, 0, p.Modulus.Length);
                PublicKey = ms.ToArray();
            }
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
        ///   Identifies the public key's cryptographic algorithm.
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
        /// <value>
        ///   A non-unique identifier for the public key.
        /// </value>
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

        /// <inheritdoc />
        public override void ReadData(DnsReader reader, int length)
        {
            var end = reader.Position + length;

            Flags = reader.ReadUInt16();
            Protocol = reader.ReadByte();
            Algorithm = (SecurityAlgorithm)reader.ReadByte();
            PublicKey = reader.ReadBytes(end - reader.Position);
        }

        /// <inheritdoc />
        public override void WriteData(DnsWriter writer)
        {
            writer.WriteUInt16(Flags);
            writer.WriteByte(Protocol);
            writer.WriteByte((byte)Algorithm);
            writer.WriteBytes(PublicKey);
        }

        /// <inheritdoc />
        public override void ReadData(MasterReader reader)
        {
            Flags = reader.ReadUInt16();
            Protocol = reader.ReadByte();
            Algorithm = (SecurityAlgorithm)reader.ReadByte();
            PublicKey = reader.ReadBase64String();
        }

        /// <inheritdoc />
        public override void WriteData(TextWriter writer)
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
