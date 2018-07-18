using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Makaretu.Dns.Resolving
{
    /// <summary>
    ///   A dictionary of <see cref="Node">DNS nodes</see>.
    /// </summary>
    /// <remarks>
    ///   This is a portion of the DNS distribute database.
    ///   <para>
    ///   The key is the case insensitive <see cref="Node.Name"/> and the value is a <see cref="Node"/>.
    ///   </para>    /// </remarks>
    public class Catalog : ConcurrentDictionary<string, Node>
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="Catalog"/> class.
        /// </summary>
        public Catalog() :
#if NETSTANDARD14
            base(StringComparer.OrdinalIgnoreCase)
#else
            base(StringComparer.InvariantCultureIgnoreCase)
#endif
        {
        }

        // TODO: Parents(Node)
        // TODO: Children(Node)

        /// <summary>
        ///   Include the zone information.
        /// </summary>
        /// <param name="reader">
        ///   The source of the zone information.
        /// </param>
        /// <returns>
        ///   The <see cref="Node"/> that represents the zone.
        /// </returns>
        /// <remarks>
        ///   All included nodes are marked as <see cref="Node.Authoritative"/>.
        /// </remarks>
        public Node IncludeZone(MasterReader reader)
        {
            // Read the resources.
            var resources = new List<ResourceRecord>();
            while (true)
            {
                var r = reader.ReadResourceRecord();
                if (r == null)
                {
                    break;
                }
                resources.Add(r);
            }

            // Validation
            if (resources.Count == 0)
                throw new InvalidDataException("No resources.");
            if (resources[0].Type != DnsType.SOA)
                throw new InvalidDataException("First resource record must be a SOA.");
            var soa = (SOARecord)resources[0];
            if (resources.Any(r => !r.Name.EndsWith(soa.Name)))
                throw new InvalidDataException("All resource records must belong to the zone.");

            // Insert the nodes of the zone.
            var nodes = resources.GroupBy(
                r => r.Name,
                (key, results) => new Node
                {
                    Name = key,
                    Authoritative = true,
                    Resources = new ConcurrentBag<ResourceRecord>(results)
                }
            );
            foreach (var node in nodes)
            {
                if (!TryAdd(node.Name, node))
                    throw new InvalidDataException($"'{node.Name}' already exists.");
            }

            return this[soa.Name];
        }

        /// <summary>
        ///   Remove all nodes that belong to the zone.
        /// </summary>
        /// <param name="name">
        ///   The name of the zone.
        /// </param>
        public void RemoveZone(string name)
        {
            var keys = Keys.Where(k => k.EndsWith(name));
            foreach (var key in keys)
            {
                TryRemove(key, out Node _);
            }
        }
    }
}
