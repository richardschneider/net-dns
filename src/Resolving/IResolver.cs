using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Makaretu.Dns.Resolving
{
    /// <summary>
    ///   Answers a question.
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        ///   Information about some portion of the DNS database.
        /// </summary>
        /// <value>
        ///   A subset of the DNS database.
        /// </value>
        Catalog Catalog { get; set; }

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
        Task<Message> ResolveAsync(
            Question question, 
            Message response = null,
            CancellationToken cancel = default(CancellationToken));
    }
}
