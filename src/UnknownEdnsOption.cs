using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   An unknown EDNS option.
    /// </summary>
    /// <remarks>
    ///   When an <see cref="EdnsOption"/> is read with a <see cref="EdnsOption.Type"/> that
    ///   is not <see cref="EdnsOptionRegistry">registered</see>, then this is used
    ///   to deserialise the information.
    /// </remarks>
    public class UnknownEdnsOption : EdnsOption
    {
        /// <summary>
        ///   Specfic data for the option.
        /// </summary>
        public byte[] Data { get; set; }

        /// <inheritdoc />
        public override void ReadData(DnsReader reader, int length)
        {
            Data = reader.ReadBytes(length);
        }

        /// <inheritdoc />
        public override void WriteData(DnsWriter writer)
        {
            writer.WriteBytes(Data);
        }

    }
}
