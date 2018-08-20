using System;
using System.Linq;

namespace Makaretu.Dns
{

    /// <summary>
    ///  Identities the cryptographic digest algorithm used by the resource record.
    /// </summary>
    /// <seealso cref="ResourceRecord"/>
    /// <seealso href="https://www.ietf.org/rfc/rfc4034.txt">RFC 4035</seealso>
    public enum DigestType : byte
    {
        /// <summary>
        /// SHA-1.
        /// </summary>
        Sha1 = 1,

        /// <summary>
        /// SHA-256
        /// </summary>
        Sha256 = 2,
    }
}