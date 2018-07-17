using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Makaretu.Dns.Resolving
{
    /// <summary>
    ///   An abstract base class for a Resolver.
    /// </summary>
    public abstract class Resolver : IResolver
    {
        /// <inheritdoc />
        public Catalog Catalog { get; set; }

        /// <summary>
        ///   The question to answer.
        /// </summary>
        public Question Question { get; private set; }

        /// <summary>
        ///   The result of answering the <see cref="Question"/>.
        /// </summary>
        public Message Response { get; private set; }

        /// <inheritdoc />
        public async Task<Message> ResolveAsync(Question question, Message response = null, CancellationToken cancel = default(CancellationToken))
        {
            Question = question;
            Response = response ?? new Message { QR = true };
            bool found = await FindAnswerAsync(cancel);
            if (!found && Response.Status == MessageStatus.NoError)
                Response.Status = MessageStatus.NameError;

            // If a name error, then add the domain authority.
            if (Response.Status == MessageStatus.NameError)
            {
                SOARecord soa = FindAuthority(Question.Name);
                if (soa != null)
                {
                    Response.AuthorityRecords.Add(soa);
                }
            }
            return Response;
        }

        /// <summary>
        ///   Find an answer to the <see cref="Question"/>.
        /// </summary>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation.  The task's value
        ///   is <b>true</b> if the resolver added an answer.
        /// </returns>
        /// <remarks>
        ///   Derived classes must implement this method.
        /// </remarks>
        protected abstract Task<bool> FindAnswerAsync(CancellationToken cancel);

        SOARecord FindAuthority(string domainName)
        {
            var name = domainName;
            while (true)
            {
                if (Catalog.TryGetValue(name, out Node node))
                {
                    var soa = node.Resources.OfType<SOARecord>().FirstOrDefault();
                    if (soa != null) return soa;
                }
                var x = name.IndexOf('.');
                if (x < 0) break;
                name = name.Substring(x + 1);
            }

            return null;
        }
    }
}
