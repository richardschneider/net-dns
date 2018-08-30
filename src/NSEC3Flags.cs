using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Makaretu.Dns
{
    /// <summary>
    ///   The processing options of a <see cref="NSEC3Record"/>.
    /// </summary>
    /// <remarks>
    ///   Defined by <see href="https://tools.ietf.org/html/rfc5155#section-3.1.2">RFC 5155 - DNS Security (DNSSEC) Hashed Authenticated Denial of Existence</see>.
    /// </remarks>
    [Flags]
    public enum NSEC3Flags : byte
    {

        /// <summary>
        ///   Indicates uncovered unsigned delegations.
        /// </summary>
        OptOut = 0x01,
    }

}
