using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Methods to read DNS data items.
    /// </summary>
    public class DnsReader
    {
        Stream stream;
        readonly Dictionary<int, string> names = new Dictionary<int, string>();

        /// <summary>
        ///   The reader relative position within the stream.
        /// </summary>
        public int Position;

        /// <summary>
        ///   Creates a new instance of the <see cref="DnsReader"/> on the
        ///   specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The source for data items.
        /// </param>
        public DnsReader(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        ///   Read a byte.
        /// </summary>
        /// <returns>
        ///   The next byte in the stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        ///   When no more data is available.
        /// </exception>
        public byte ReadByte()
        {
            var value = stream.ReadByte();
            if (value < 0)
                throw new EndOfStreamException();
            ++Position;
            return (byte)value;
        }

        /// <summary>
        ///   Read the specified number of bytes.
        /// </summary>
        /// <param name="length">
        ///   The number of bytes to read.
        /// </param>
        /// <returns>
        ///   The next <paramref name="length"/> bytes in the stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        ///   When no more data is available.
        /// </exception>
        public byte[] ReadBytes(int length)
        {
            var buffer = new byte[length];
            for (var offset = 0; length > 0; )
            {
                var n = stream.Read(buffer, offset, length);
                if (n == 0)
                    throw new EndOfStreamException();
                offset += n;
                length -= n;
                Position += n;
            }
            
            return buffer;
        }

        /// <summary>
        ///   Read an unsigned short.
        /// </summary>
        /// <returns>
        ///   The two byte little-endian value as an unsigned short.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        ///   When no more data is available.
        /// </exception>
        public ushort ReadUInt16()
        {
            int value = ReadByte();
            value = value << 8 | ReadByte();
            return (ushort)value;
        }

        /// <summary>
        ///   Read an unsigned int.
        /// </summary>
        /// <returns>
        ///   The four byte little-endian value as an unsigned int.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        ///   When no more data is available.
        /// </exception>
        public uint ReadUInt32()
        {
            int value = ReadByte();
            value = value << 8 | ReadByte();
            value = value << 8 | ReadByte();
            value = value << 8 | ReadByte();
            return (uint)value;
        }

        /// <summary>
        ///   Read a domain name.
        /// </summary>
        /// <returns>
        ///   The domain name as a string.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        ///   When no more data is available.
        /// </exception>
        /// <remarks>
        ///   A domain name is represented as a sequence of labels, where
        ///   each label consists of a length octet followed by that
        ///   number of octets. The domain name terminates with the
        ///   zero length octet for the null label of the root.
        ///   <note>
        ///   Compressed domain names are also supported.
        ///   </note>
        /// </remarks>
        public string ReadDomainName()
        {
            var pointer = Position;
            var length = ReadByte();

            // Do we have a compressed pointer?
            if ((length & 0xC0) == 0xC0)
            {
                var cpointer = (length ^ 0xC0) << 8 | ReadByte();
                var cname = names[cpointer];
                names[pointer] = cname;
                return cname;
            }

            // End of labels?
            if (length == 0)
            {
                return string.Empty;
            }

            // Read current label and remaining labels.
            var buffer = ReadBytes(length);
            var name = Encoding.UTF8.GetString(buffer, 0, length);
            var remainingLabels = ReadDomainName();
            if (remainingLabels != string.Empty)
            {
                name = name + "." + remainingLabels;
            }

            // Add to compressed names
            names[pointer] = name;

            return name;
        }

        /// <summary>
        ///   Read a string.
        /// </summary>
        /// <remarks>
        ///   Strings are encoded with a length prefixed byte.  All strings are treated
        ///   as UTF-8.
        /// </remarks>
        /// <returns>
        ///   The string.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        ///   When no more data is available.
        /// </exception>
        public string ReadString()
        {
            var length = ReadByte();
            var buffer = ReadBytes(length);
            return Encoding.UTF8.GetString(buffer, 0, length);
        }

        /// <summary>
        ///   Read a time span (interval)
        /// </summary>
        /// <returns>
        ///   A <see cref="TimeSpan"/> with second resolution.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        ///   When no more data is available.
        /// </exception>
        /// <remarks>
        ///   The interval is represented as the number of seconds in two bytes.
        /// </remarks>
        public TimeSpan ReadTimeSpan()
        {
            return TimeSpan.FromSeconds(ReadUInt32());
        }

        /// <summary>
        ///   Read an Internet address.
        /// </summary>
        /// <returns>
        ///   An <see cref="IPAddress"/>.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        ///   When no more data is available.
        /// </exception>
        /// <remarks>
        ///   Use a <paramref name="length"/> of 4 to read an IPv4 address and
        ///   16 to read an IPv6 address.
        /// </remarks>
        public IPAddress ReadIPAddress(int length = 4)
        {
            var address = ReadBytes(length);
            return new IPAddress(address);
        }
    }
}
