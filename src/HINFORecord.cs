using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Host information. 
    /// </summary>
    /// <remarks>
    ///   Standard values for CPU and OS can be found in [RFC-1010].
    ///
    ///   HINFO records are used to acquire general information about a host. The
    ///   main use is for protocols such as FTP that can use special procedures
    ///   when talking between machines or operating systems of the same type.
    /// </remarks>
    public class HINFORecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="HINFORecord"/> class.
        /// </summary>
        public HINFORecord() : base()
        {
            Type = 13;
            TTL = ResourceRecord.DefaultHostTTL;
        }

        /// <summary>
        ///  CPU type.
        /// </summary>
        public string Cpu { get; set; }

        /// <summary>
        ///  Operating system type.
        /// </summary>
        public string OS { get; set; }


        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            Cpu = reader.ReadString();
            OS = reader.ReadString();
        }

        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            writer.WriteString(Cpu);
            writer.WriteString(OS);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var that = obj as HINFORecord;
            if (that == null) return false;

            return base.Equals(obj)
                && this.Cpu == that.Cpu
                && this.OS == that.OS;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode()
                ^ Cpu?.GetHashCode() ?? 0
                ^ OS?.GetHashCode() ?? 0;
        }

    }
}
