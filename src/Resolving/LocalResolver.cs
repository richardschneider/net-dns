using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Makaretu.Dns.Resolving
{
    /// <summary>
    ///   Only anwsers questions from the local <see cref="Catalog"/>.
    /// </summary>
    public class LocalResolver : Resolver
    {
        /// <inheritdoc />
        protected override Task<bool> FindAnswerAsync(Question question, Message response, CancellationToken cancel)
        {
            // Find a node for the question name.
            var node = Catalog
                .Where(c => c.Key == question.Name)
                .Select(c => c.Value)
                .FirstOrDefault();
            if (node == null)
            {
                return Task.FromResult(false);
            }

            // https://tools.ietf.org/html/rfc1034#section-3.7.1
            response.AA |= node.Authoritative && question.Class != Class.ANY;

            //  Find the resources that match the question.
            var resources = node.Resources
                .Where(r => question.Class == Class.ANY || r.Class == question.Class)
                .Where(r => question.Type == DnsType.ANY || r.Type == question.Type)
                .Where(r => node.Authoritative || !r.IsExpired(question.CreationTime))
                .ToArray();
            if (resources.Length > 0)
            {
                response.Answers.AddRange(resources);
                return Task.FromResult(true);
            }

            // If node is alias (CNAME), then find answers for the alias' target.
            // The CNAME is added to the answers.
            var cname = node.Resources.OfType<CNAMERecord>().FirstOrDefault();
            if (cname != null)
            {
                response.Answers.Add(cname);
                question = question.Clone<Question>();
                question.Name = cname.Target;
                return FindAnswerAsync(question, response, cancel);
            }

            // TODO: https://tools.ietf.org/html/rfc1034#section-4.3.3 Wildcards

            // Nothing more can be done.
            return Task.FromResult(false);
        }
    }
}
