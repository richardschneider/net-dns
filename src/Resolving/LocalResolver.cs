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
        protected override Task<bool> FindAnswerAsync(CancellationToken cancel)
        {
            // Find a node for the question name.
            var node = Catalog
                .Where(c => c.Key == Question.Name)
                .Select(c => c.Value)
                .FirstOrDefault();
            if (node == null)
            {
                return Task.FromResult(false);
            }
            Response.AA = node.Authoritative;

            //  Find the resources that match question.
            var resources = node.Resources
                .Where(r => Question.Class == Class.ANY || r.Class == Question.Class)
                .Where(r => Question.Type == DnsType.ANY || r.Type == Question.Type)
                .Where(r => node.Authoritative || !r.IsExpired(Question.CreationTime))
                .ToArray();
            if (resources.Length == 0)
            {
                return Task.FromResult(false);
            }
            Response.Answers.AddRange(resources);
            return Task.FromResult(true);
        }
    }
}
