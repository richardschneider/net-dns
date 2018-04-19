using System;
using System.Collections.Generic;
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
            Type = 47;
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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var that = obj as NSECRecord;
            if (that == null) return false;

            return base.Equals(obj)
                && this.NextOwnerName == that.NextOwnerName
                && this.TypeBitmaps.SequenceEqual(that.TypeBitmaps);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode()
                ^ NextOwnerName?.GetHashCode() ?? 0
                ^ TypeBitmaps?.Aggregate(0, (r, b) => r ^ b.GetHashCode()) ?? 0;
        }


    }
}
