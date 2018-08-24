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

#if (!NETSTANDARD14 && !NET45)
        /// <summary>
        ///   Creates a new instance of the <see cref="DNSKEYRecord"/> class
        ///   from the specified ECDSA key.
        /// </summary>
        /// <param name="key">
        ///   A public or private ECDSA key.
        /// </param>
        /// <exception cref="ArgumentException">
        ///   <paramref name="key"/> is not named nistP256 nor nist384.
        /// </exception>
        /// <exception cref="CryptographicException">
        ///   <paramref name="key"/> is not valid.
        /// </exception>
        public DNSKEYRecord(ECDsa key)
            : this()
        {
            var p = key.ExportParameters(includePrivateParameters: false);
            p.Validate();

            Flags = 256; // TODO: define an enum

            if (!p.Curve.IsNamed)
                throw new ArgumentException("Only named ECDSA curves are allowed.");
            // TODO: Need a security algorithm registry
            switch (p.Curve.Oid.FriendlyName)
            {
                case "nistP256":
                case "ECDSA_P256":
                    Algorithm = SecurityAlgorithm.ECDSAP256SHA256;
                    break;
                case "nistP384":
                case "ECDSA_P384":
                    Algorithm = SecurityAlgorithm.ECDSAP384SHA384;
                    break;
                default:
                    throw new ArgumentException($"ECDSA curve '{p.Curve.Oid.FriendlyName} is not known'.");
            }

            // ECDSA public keys consist of a single value, called "Q" in FIPS 186-3.
            // In DNSSEC keys, Q is a simple bit string that represents the
            // uncompressed form of a curve point, "x | y".
            using (var ms = new MemoryStream())
            {
                ms.Write(p.Q.X, 0, p.Q.X.Length);
                ms.Write(p.Q.Y, 0, p.Q.Y.Length);
                PublicKey = ms.ToArray();
            }
        }
#endif

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
