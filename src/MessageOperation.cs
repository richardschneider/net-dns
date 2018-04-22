using System;
using System.Collections.Generic;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   The type of <see cref="Message"/>.
    /// </summary>
    /// <seealso cref="Message.Opcode"/>
    public enum MessageOperation : byte
    {
        /// <summary>
        ///   Standard query.
        /// </summary>
        Query = 0,

        /// <summary>
        ///   Inverse query.
        /// </summary>
        InverseQuery = 1,

        /// <summary>
        ///   A server status request.
        /// </summary>
        Status = 2,

        /// <summary>
        ///   Update message, see <see href="https://tools.ietf.org/html/rfc2136"/>.
        /// </summary>
        Update = 5
    }
}
