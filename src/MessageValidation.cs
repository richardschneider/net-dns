using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Makaretu.Dns
{
    public partial class Message
    {
        /// <summary>
        ///   Throws if the response is invalid.
        /// </summary>
        /// <param name="response">
        ///   The response to this query.
        /// </param>
        /// <param name="resolver">
        ///   Used to obtain the <see cref="DNSKEYRecord"/> and <see cref="DSRecord"/>
        ///   resource records when doing DNSEC validations.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <exception cref="Exception">
        ///   When the <paramref name="response"/> is not valid.
        /// </exception>
        /// <seealso cref="ValidateResponseAsync"/>
        public async Task EnsureValidResponseAsync(
            Message response,
            IResolver resolver,
            CancellationToken cancel = default(CancellationToken)
            )
        {
            var reason = await ValidateResponseAsync(response, resolver, cancel);
            if (reason != null)
                throw new Exception(reason);
        }

        /// <summary>
        ///   Validate the response to a query.
        /// </summary>
        /// <param name="response">
        ///   The response to this query.
        /// </param>
        /// <param name="resolver">
        ///   Used to obtain the <see cref="DNSKEYRecord"/> and <see cref="DSRecord"/>
        ///   resource records when doing DNSEC validations.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation. The task's value is
        ///  <b>null</b> if the <paramref name="response"/> is valid; otherwise, a
        ///  <see cref="string"/> containing the reason why the response is invalid.
        /// </returns>
        /// <seealso cref="EnsureValidResponseAsync"/>
        public async Task<string> ValidateResponseAsync(
            Message response,
            IResolver resolver,
            CancellationToken cancel = default(CancellationToken)
            )
        {
            if (!response.QR)
                return "Not a response, QR is not set.";
            if (Id != response.Id)
                return "Response and query IDs are not equal.";
            if (response.Status == MessageStatus.NoError 
                && (response.Answers.Count + response.AuthorityRecords.Count == 0))
                return "No answers.";
            // TODO: if MXDOMAIN needs an SOA authority record

            // TODO: DNSSEC

            // Everything is good.
            return null;
        }

    }
}
