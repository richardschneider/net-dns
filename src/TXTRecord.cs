using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Text strings.
    /// </summary>
    /// <remarks>
    ///   TXT RRs are used to hold descriptive text.  The semantics of the text
    ///   depends on the domain where it is found.
    /// </remarks>
    public class TXTRecord : ResourceRecord
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="TXTRecord"/> class.
        /// </summary>
        public TXTRecord() : base()
        {
            Type = DnsType.TXT;
        }

        /// <summary>
        ///  The sequence of strings.
        /// </summary>
        public List<string> Strings { get; set; } = new List<string>();

        /// <inheritdoc />
        protected override void ReadData(DnsReader reader, int length)
        {
            while (length > 0)
            {
                var s = reader.ReadString();
                Strings.Add(s);
                length -= Encoding.UTF8.GetByteCount(s) + 1;
            }
        }

        /// <inheritdoc />
        internal override void ReadData(MasterReader reader)
        {
            while (!reader.IsEndOfLine())
            {
                Strings.Add(reader.ReadString());
            }
        }



        /// <inheritdoc />
        protected override void WriteData(DnsWriter writer)
        {
            foreach (var s in Strings)
            {
                writer.WriteString(s);
            }
        }

        /// <inheritdoc />
        protected override void WriteData(TextWriter writer)
        {
            bool next = false;
            foreach (var s in Strings)
            {
                if (next)
                {
                    writer.Write(' ');
                }
                writer.Write('"');
                writer.Write(s.Replace("\\", "\\\\").Replace("\"", "\\\""));
                writer.Write('"');
                next = true;
            }
        }

    }
}
