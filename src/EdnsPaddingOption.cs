using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Padding for a <see cref="Message"/>.
    /// </summary>
    /// <remarks>
    ///  Padding is used to frustrate size-based correlation of the encrypted message.
    /// </remarks>
    /// <seealso href="https://tools.ietf.org/html/rfc7830"/>
    public class EdnsPaddingOption : EdnsOption
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="EdnsPaddingOption"/> class.
        /// </summary>
        public EdnsPaddingOption()
        {
            Type = EdnsOptionType.Padding;
        }

        /// <summary>
        ///   The padding bytes.
        /// </summary>
        /// <value>
        ///   The bytes used for padding.  Normally all bytes are zero.
        /// </value>
        public byte[] Padding { get; set; }

        /// <inheritdoc />
        public override void ReadData(DnsReader reader, int length)
        {
            Padding = reader.ReadBytes(length);
        }

        /// <inheritdoc />
        public override void WriteData(DnsWriter writer)
        {
            writer.WriteBytes(Padding);
        }

    }
}
