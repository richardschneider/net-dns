using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Base class for all DNS objects.
    /// </summary>
    public abstract class DnsObject : IDnsSerialiser
#if !NETSTANDARD14
        , ICloneable
#endif
    {
        /// <summary>
        ///   When the object was created.
        /// </summary>
        /// <value>
        ///   Local time.
        /// </value>
        /// <remarks>
        ///   Cloning does not alter the value.
        /// </remarks>
        public DateTime CreationTime { get; private set; } = DateTime.Now;

        /// <summary>
        ///   Length in bytes of the object when serialised.
        /// </summary>
        /// <returns>
        ///   Numbers of bytes when serialised.
        /// </returns>
        public int Length()
        {
            var writer = new DnsWriter(Stream.Null);
            Write(writer);

            return writer.Position;
        }

        /// <summary>
        ///   Makes a deep copy of the object.
        /// </summary>
        /// <returns>
        ///   A deep copy of the dns object.
        /// </returns>
        /// <remarks>
        ///   Uses serialisation to make a copy.
        /// </remarks>
        public virtual object Clone()
        {
            using (var ms = new MemoryStream())
            {
                Write(ms);
                ms.Position = 0;
                var clone = (DnsObject)Read(ms);
                clone.CreationTime = CreationTime;
                return clone;
            }
        }

        /// <summary>
        ///   Makes a deep copy of the object.
        /// </summary>
        /// <typeparam name="T">
        ///   Some type derived from <see cref="DnsObject"/>.
        /// </typeparam>
        /// <returns>
        ///   A deep copy of the dns object.
        /// </returns>
        /// <remarks>
        ///   Use serialisation to make a copy.
        /// </remarks>
        public T Clone<T>() where T : DnsObject
        {
            return (T)Clone();
        }

        /// <summary>
        ///   Reads the DNS object from a byte array.
        /// </summary>
        /// <param name="buffer">
        ///   The source for the DNS object.
        /// </param>
        public IDnsSerialiser Read(byte[] buffer)
        {
            return Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        ///   Reads the DNS object from a byte array.
        /// </summary>
        /// <param name="buffer">
        ///   The source for the DNS object.
        /// </param>
        /// <param name="offset">
        ///   The offset into the <paramref name="buffer"/>.
        /// </param>
        /// <param name="count">
        ///   The number of bytes in the <paramref name="buffer"/>.
        /// </param>
        public IDnsSerialiser Read(byte[] buffer, int offset, int count)
        {
            using (var ms = new MemoryStream(buffer, offset, count, false))
            {
                return Read(new DnsReader(ms));
            }
        }

        /// <summary>
        ///   Reads the DNS object from a stream.
        /// </summary>
        /// <param name="stream">
        ///   The source for the DNS object.
        /// </param>
        public IDnsSerialiser Read(Stream stream)
        {
            return Read(new DnsReader(stream));
        }

        /// <inheritdoc />
        public abstract IDnsSerialiser Read(DnsReader reader);

        /// <summary>
        ///   Writes the DNS object to a byte array.
        /// </summary>
        /// <returns>
        ///   A byte array containing the binary representaton of the DNS object.
        /// </returns>
        public byte[] ToByteArray()
        {
            using (var ms = new MemoryStream())
            {
                Write(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///   Writes the DNS object to a stream.
        /// </summary>
        /// <param name="stream">
        ///   The destination for the DNS object.
        /// </param>
        public void Write(Stream stream)
        {
            Write(new DnsWriter(stream));
        }

        /// <inheritdoc />
        public abstract void Write(DnsWriter writer);

        /// <summary>
        ///   Determines if the two domain names are equal.
        /// </summary>
        /// <param name="a">A domain name</param>
        /// <param name="b">A domain name</param>
        /// <returns>
        ///   <b>true</b> if <paramref name="a"/> and <paramref name="b"/> are
        ///   considered equal.
        /// </returns>
        /// <remarks>
        ///   Uses a case-insenstive algorithm, where 'A-Z' are equivalent to 'a-z'.
        /// </remarks>
        public static bool NamesEquals(string a, string b)
        {
#if NETSTANDARD14
            return a?.ToLowerInvariant() == b?.ToLowerInvariant();
#else
            return 0 == StringComparer.InvariantCultureIgnoreCase.Compare(a, b);
#endif
        }
    }
}
