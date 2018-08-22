using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Makaretu.Dns
{
    /// <summary>
    ///   Registry of implemented <see cref="DigestType"/>.
    /// </summary>
    /// <see cref="DigestType"/>
    /// <see cref="HashAlgorithm"/>
    public static class DigestRegistry
    {
        /// <summary>
        ///   All the hashing algorithms..
        /// </summary>
        /// <remarks>
        ///   The key is the <see cref="DigestType"/>.
        ///   The value is a function that returns a new <see cref="ResourceRecord"/>.
        /// </remarks>
        public static Dictionary<DigestType, Func<HashAlgorithm>> Digests;

        static DigestRegistry()
        {
            Digests = new Dictionary<DigestType, Func<HashAlgorithm>>();
            Digests.Add(DigestType.Sha1, () => SHA1.Create());
            Digests.Add(DigestType.Sha256, () => SHA256.Create());
            Digests.Add(DigestType.Sha384, () => SHA384.Create());
        }

        /// <summary>
        ///   Gets the hash algorithm for the <see cref="DigestType"/>.
        /// </summary>
        /// <param name="digestType">
        ///   One of the <see cref="DigestType"/> values.
        /// </param>
        /// <returns>
        ///   A new instance of the <see cref="HashAlgorithm"/> that implements
        ///   the <paramref name="digestType"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        ///   When <paramref name="digestType"/> is not implemented.
        /// </exception>
        public static HashAlgorithm Create(DigestType digestType)
        {
            if (Digests.TryGetValue(digestType, out Func<HashAlgorithm> maker)) 
            {
                return maker();
            }
            throw new NotImplementedException($"Digest type '{digestType}' is not implemented.");
        }

    }
}
