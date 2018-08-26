using SimpleBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Transaction Signature.
    /// </summary>
    /// <remarks>
    ///   Defined in <see href="https://tools.ietf.org/html/rfc2845">RFC 2845</see>.
    /// </remarks>
    public class TSIGRecord : ResourceRecord
    {
        /// <summary>
        ///  The <see cref="Algorithm"/> name for HMACMD5.
        /// </summary>
        public const string HMACMD5 = "HMAC-MD5.SIG-ALG.REG.INT";

        /// <summary>
        ///   Creates a new instance of the <see cref="TSIGRecord"/> class.
        /// </summary>
        public TSIGRecord() : base()
        {
            Type = DnsType.TSIG;
            Class = Class.ANY;
            TTL = TimeSpan.Zero;
            var now = DateTime.UtcNow;
            TimeSigned = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind);
        }

        /// <summary>
        ///   Identifies the cryptographic algorithm to create the <see cref="MAC"/>.
        /// </summary>
        /// <value>
        ///   Identifies the HMAC alogirthm.
        /// </value>
        public string Algorithm { get; set; }

        /// <summary>
        ///   When the record was signed.
        /// </summary>
        /// <value>
        ///   Must be in <see cref="DateTimeKind.Utc"/>.
        ///   Resolution in seconds.
        ///   Defaults to <see cref="DateTime.UtcNow"/> less the milliseconds/
        /// </value>
        public DateTime TimeSigned { get; set; }

        /// <inheritdoc />
        public override void ReadData(DnsReader reader, int length)
        {
            Algorithm = reader.ReadDomainName();
            TimeSigned = reader.ReadDateTime48();
        }

        /// <inheritdoc />
        public override void WriteData(DnsWriter writer)
        {
            writer.WriteDomainName(Algorithm);
            writer.WriteDateTime48(TimeSigned);
        }

        /// <inheritdoc />
        public override void ReadData(MasterReader reader)
        {
            Algorithm = reader.ReadDomainName();
            TimeSigned = DateTime.ParseExact
            (
                reader.ReadString(),
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
            );
        }

        /// <inheritdoc />
        public override void WriteData(TextWriter writer)
        {
            writer.Write(Algorithm);
            writer.Write(' ');
            writer.Write(TimeSigned.ToUniversalTime().ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            writer.Write(' ');
        }
    }

}
