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
        ///   Defined hashing algorithms.
        /// </summary>
        /// <remarks>
        ///   The key is the <see cref="DigestType"/>.
        ///   The value is a function that returns a new <see cref="ResourceRecord"/>.
        /// </remarks>
        public static Dictionary<DigestType, Func<HashAlgorithm>> Digests;

        /// <summary>
        ///   Defined hashing algorithm for the <see cref="SecurityAlgorithm"/>.
        /// </summary>
        /// <remarks>
        ///   The key is the <see cref="SecurityAlgorithm"/>.
        ///   The value is the <see cref="DigestType"/>.
        /// </remarks>
        public static Dictionary<SecurityAlgorithm, DigestType> Algorithms;

        static DigestRegistry()
        {
            Digests = new Dictionary<DigestType, Func<HashAlgorithm>>();
            Digests.Add(DigestType.Sha1, () => SHA1.Create());
            Digests.Add(DigestType.Sha256, () => SHA256.Create());
            Digests.Add(DigestType.Sha384, () => SHA384.Create());
            Digests.Add(DigestType.Sha512, () => SHA512.Create());

            Algorithms = new Dictionary<SecurityAlgorithm, DigestType>();
            Algorithms.Add(SecurityAlgorithm.RSASHA1, DigestType.Sha1);
            Algorithms.Add(SecurityAlgorithm.RSASHA256, DigestType.Sha256);
            Algorithms.Add(SecurityAlgorithm.RSASHA512, DigestType.Sha512);
            Algorithms.Add(SecurityAlgorithm.DSA, DigestType.Sha1);
            Algorithms.Add(SecurityAlgorithm.ECDSAP256SHA256, DigestType.Sha256);
            Algorithms.Add(SecurityAlgorithm.ECDSAP384SHA384, DigestType.Sha384);
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

        /// <summary>
        ///   Gets the hash algorithm for the <see cref="SecurityAlgorithm"/>.
        /// </summary>
        /// <param name="algorithm">
        ///   One of the <see cref="SecurityAlgorithm"/> values.
        /// </param>
        /// <returns>
        ///   A new instance of the <see cref="HashAlgorithm"/> that is used
        ///   for the <paramref name="algorithm"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        ///   When the <see cref="HashAlgorithm"/> for <paramref name="algorithm"/> 
        ///   is not defined.
        /// </exception>
        public static HashAlgorithm Create(SecurityAlgorithm algorithm)
        {
            if (Algorithms.TryGetValue(algorithm, out DigestType digestType))
            {
                return Create(digestType);
            }
            throw new NotImplementedException($"The digest type for the algorithm '{algorithm}' is not defined.");
        }
    }
}
