using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Contains the IPv6 address of the named resource.
    /// </summary>
    public class AAAARecord : AddressRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="AAAARecord"/> class.
        /// </summary>
        public AAAARecord() : base()
        {
            Type = DnsType.AAAA;
        }

    }
}
