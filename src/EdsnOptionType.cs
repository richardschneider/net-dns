using System;
using System.Linq;

namespace Makaretu.Dns
{

    /// <summary>
    ///   EDSN option codes.
    /// </summary>
    /// <remarks>
    ///   Codes are specified in <see href="https://www.iana.org/assignments/dns-parameters/dns-parameters.xhtml#dns-parameters-11">IANA - DNS EDNS0 Option Codes</see>.
    /// </remarks>
    /// <seealso cref="EdnsOption.Type"/>
    /// <seealso cref="OPTRecord"/>
    /// <seealso cref="EdnsOptionRegistry"/>
    public enum EdnsOptionType : ushort
    {
        /// <summary>
        ///   Minimum value for local or experiment use.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc6891"/>
        ExperimentalMin = 65001,

        /// <summary>
        ///   Maximum value for local or experiment use.s
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc6891"/>
        ExperimentalMax = 65534,

        /// <summary>
        ///   Reserved for future expansion.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc6891"/>
        FutureExpansion = 65535 
    }
}