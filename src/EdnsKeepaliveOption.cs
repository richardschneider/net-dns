using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   TCP idle time.
    /// </summary>
    /// <remarks>
    ///   Signals a variable idle timeout.  This
    ///   signalling encourages the use of long-lived TCP connections by
    ///   allowing the state associated with TCP transport to be managed
    ///   effectively with minimal impact on the DNS transaction time.
    /// </remarks>
    /// <seealso href="https://tools.ietf.org/html/rfc7828"/>
    public class EdnsKeepaliveOption : EdnsOption
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="EdnsKeepaliveOption"/> class.
        /// </summary>
        public EdnsKeepaliveOption()
        {
            Type = EdnsOptionType.Keepalive;
        }

        /// <summary>
        ///   The idle timeout value for the TCP connection.
        /// </summary>
        /// <value>
        ///   The resolution is 100 milliseconds.
        /// </value>
        public TimeSpan? Timeout { get; set; }

        /// <inheritdoc />
        public override void ReadData(DnsReader reader, int length)
        {
            if (length == 0) {
                Timeout = null;
                return;
            }
            Timeout = TimeSpan.FromTicks(reader.ReadUInt16() * TimeSpan.TicksPerMillisecond * 100);
        }

        /// <inheritdoc />
        public override void WriteData(DnsWriter writer)
        {
            if (Timeout.HasValue)
            {
                writer.WriteUInt16((ushort)(Timeout.Value.Ticks / TimeSpan.TicksPerMillisecond / 100));
            }
        }

    }
}
