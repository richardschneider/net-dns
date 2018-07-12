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
