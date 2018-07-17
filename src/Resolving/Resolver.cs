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
        /// <summary>
        ///   Information about some portion of the DNS database.
        /// </summary>
        /// <value>
        ///   A subset of the DNS database. Typically (1) one or more zones or (2) a cache received
        ///   responses.
        /// </value>
        public Catalog Catalog { get; set; }

        /// <inheritdoc />
        public async Task<Message> ResolveAsync(
            Message request,
            CancellationToken cancel = default(CancellationToken))
        {
            var response = request.CreateResponse();

            // TODO: Unicast DNS only requires one question to be answer.
            // TODO: Run all questions in parallel.
            foreach (var question in request.Questions)
            {
                await ResolveAsync(question, response, cancel);
            }

            return response;
        }

        /// <summary>
        ///   Get an answer to a question.
        /// </summary>
        /// <param name="question">
        ///   The question to answer.
        /// </param>
        /// <param name="response">
        ///   Where the answers are added.  If <b>null</b>, then a new <see cref="Message"/> is
        ///   created.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation. The task's value is
        ///   a <see cref="Message"/> response to the <paramref name="question"/>.
        /// </returns>
        /// <remarks>
        ///   If the question's domain does not exist, then the closest authority
        ///   (<see cref="SOARecord"/>) is added to the <see cref="Message.AuthorityRecords"/>.
        /// </remarks>
        public async Task<Message> ResolveAsync(Question question, Message response = null, CancellationToken cancel = default(CancellationToken))
        {
            response = response ?? new Message { QR = true };
            bool found = await FindAnswerAsync(question, response, cancel);
            if (!found && response.Status == MessageStatus.NoError)
                response.Status = MessageStatus.NameError;

            // If a name error, then add the domain authority.
            if (response.Status == MessageStatus.NameError)
            {
                SOARecord soa = FindAuthority(question.Name);
                if (soa != null)
                {
                    response.AuthorityRecords.Add(soa);
                }
            }

            // TODO: Add additonal records.

            return response;
        }

        /// <summary>
        ///   Find an answer to the <see cref="Question"/>.
        /// </summary>
        /// <param name="question">
        ///   The question to answer.
        /// </param>
        /// <param name="response">
        ///   Where the answers are added.
        /// </param>
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
        protected abstract Task<bool> FindAnswerAsync(Question question, Message response, CancellationToken cancel);

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
