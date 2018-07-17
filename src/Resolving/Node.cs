using System;
using System.Collections.Concurrent;
using System.Text;

namespace Makaretu.Dns.Resolving
{
    /// <summary>
    ///   Locally held information on a domain name. 
    /// </summary>
    /// <remarks>
    ///   The domain name system is distributed, only a portion of the database
    ///   is available on each local host.
    /// </remarks>
    public class Node
    {
        /// <summary>
        ///   The name of the node.
        /// </summary>
        /// <value>
        ///   An absolute (fully qualified) name.  For example, "emanon.org".
        /// </value>
        /// <remarks>
        ///   All <see cref="Resources"/> must have a <see cref="ResourceRecord.Name"/> that
        ///   matches this value.
        /// </remarks>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        ///   The resource records associated with this node.
        /// </summary>
        /// <value>
        ///   Commonly called the RRSET (resource record set).
        /// </value>
        public ConcurrentBag<ResourceRecord> Resources { get; set;  } = new ConcurrentBag<ResourceRecord>();


        /// <summary>
        ///   Indicates that the node's resources contains the complete information for
        ///   the node.
        /// </summary>
        /// <value>
        ///   <b>true</b> if the <see cref="Resources"/> are authoritative; otherwise, <b>false</b>.
        /// </value>
        /// <remarks>
        ///   An Authoritative node is typically defined in a <see cref="Catalog.IncludeZone">zone</see>.
        /// </remarks>
        public bool Authoritative { get; set; }

    }
}