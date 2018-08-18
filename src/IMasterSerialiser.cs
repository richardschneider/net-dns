using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Master file serialisation of a resource record.
    /// </summary>
    /// <remarks>
    ///   The "mater file format" is the text representation of a <see cref="ResourceRecord"/>.
    ///   It is also referred to as the "presentation format".
    ///   See <see href="https://tools.ietf.org/html/rfc1035">RFC 1035 - 5 Master File</see>
    ///   and <see href="https://tools.ietf.org/html/rfc3597">RFC 3597 - Handling of Unknown DNS Resource Record (RR) Types</see>
    ///   for more details.
    /// </remarks>
    public interface IMasterSerialiser
    {

        /// <summary>
        ///   Reads the text representation of a resource record.
        /// </summary>
        /// <param name="reader">
        ///   The source of the <see cref="ResourceRecord"/>.
        /// </param>
        /// <returns>
        ///   The final resource record.
        /// </returns>
        /// <remarks>
        ///   Reading a <see cref="ResourceRecord"/> will return a new instance that
        ///   is type specific
        /// </remarks>
        ResourceRecord Read(MasterReader reader);

        /// <summary>
        ///  Writes the text representation of a resource record.
        /// </summary>
        /// <param name="writer">
        ///   The destination of the <see cref="ResourceRecord"/>.
        /// </param>
        void Write(TextWriter writer);
    }
}
