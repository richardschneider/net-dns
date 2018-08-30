using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public Node IncludeZone(PresentationReader reader)
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
                    Resources = new ConcurrentSet<ResourceRecord>(results)
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

        /// <summary>
        ///   Adds the resource record to the catalog. 
        /// </summary>
        /// <param name="resource">
        ///   The <see cref="ResourceRecord.Name"/> is also the name of the node.
        /// </param>
        /// <param name="authoritative">
        ///   Indicates if the <paramref name="resource"/> is authoritative or cached.
        /// </param>
        /// <returns>
        ///   The <see cref="Node"/> that was created or update.
        /// </returns>
        public Node Add(ResourceRecord resource, bool authoritative = false)
        {
            var node = this.AddOrUpdate(
                resource.Name,
                (k) => new Node { Name = k, Authoritative = authoritative },
                (k, n) => n
            );

            // If the resource already exist, then update the the non-equality
            // properties TTL and CreationTime.
            if (!node.Resources.Add(resource))
            {
                // TODO: not very efficient.
                node.Resources.Remove(resource);
                node.Resources.Add(resource);
            }

            return node;
        }

        /// <summary>
        ///   Include the root name servers.
        /// </summary>
        /// <returns>
        ///   The <see cref="Node"/> that represents the "root".
        /// </returns>
        /// <remarks>
        ///   A DNS recursive resolver typically needs a "root hints file". This file 
        ///   contains the names and IP addresses of the authoritative name servers for the root zone, 
        ///   so the software can bootstrap the DNS resolution process.
        /// </remarks>
        public Node IncludeRootHints()
        {
            var assembly = typeof(Catalog).GetTypeInfo().Assembly;
            using (var hints = assembly.GetManifestResourceStream("Makaretu.Dns.Resolving.RootHints"))
            {
                var reader = new PresentationReader(new StreamReader(hints));
                ResourceRecord r;
                while (null != (r = reader.ReadResourceRecord()))
                {
                    Add(r);
                }
            }

            var root = this[""];
            root.Authoritative = true;
            return root;
        }


        /// <summary>
        ///   Get a sequence of nodes in canonical order.
        /// </summary>
        /// <returns>
        ///   A sequence of nodes in canonical order.
        /// </returns>
        /// <remarks>
        ///   Node names are converted to US-ASCII lowercase and
        ///   then sorted by their reversed labels.
        /// </remarks>
        public IEnumerable<Node> NodesInCanonicalOrder()
        {
            return this.Values
                .OrderBy(node => String.Join(".", node.Name.ToLowerInvariant().Split('.').Reverse()));
        }
    }
}
