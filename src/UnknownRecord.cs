using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   An unknown resource record.
    /// </summary>
    public class UnknownRecord : ResourceRecord
    {
        /// <summary>
        ///    Specfic data for the resource.
        /// </summary>
        public byte[] Data { get; set; }


        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            Data = reader.ReadBytes(length);
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteBytes(Data);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var that = obj as UnknownRecord;
            if (that == null) return false;

            return base.Equals(obj)
                && this.Data.SequenceEqual(that.Data);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode()
                ^ Data?.Sum(b => b).GetHashCode() ?? 0;
        }

    }
}
