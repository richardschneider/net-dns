﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Makaretu.Dns.Resolving
{
    /// <summary>
    ///   Anwsers questions from the local <see cref="Catalog"/>.
    /// </summary>
    public class NameServer : IResolver
    {
        /// <summary>
        ///   Information about some portion of the DNS database.
        /// </summary>
        /// <value>
        ///   A subset of the DNS database. Typically (1) one or more zones or (2) a cache of received
        ///   responses.
        /// </value>
        public Catalog Catalog { get; set; }

        /// <summary>
        ///   Determines how multiple questions are answered.
        /// </summary>
        /// <value>
        ///   <b>false</b> to answer <b>any</b> of the questions. 
        ///   <b>false</b> to answer <b>all</b> of the questions.
        ///   The default is <b>false</b>.
        /// </value>
        /// <remarks>
        ///   Standard DNS specifies that only one of the questions need to be answered.
        ///   Multicast DNS specifies that all the questions need to be answered.
        /// </remarks>
        public bool AnswerAllQuestions { get; set; }

        /// <inheritdoc />
        public async Task<Message> ResolveAsync(
            Message request,
            CancellationToken cancel = default(CancellationToken))
        {
            var response = request.CreateResponse();

            // TODO: Run all questions in parallel.
            foreach (var question in request.Questions)
            {
                await ResolveAsync(question, response, cancel);
                if (response.Answers.Count > 0 && !AnswerAllQuestions)
                    break;
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

            // Add additonal records.
            AddAdditionalRecords(response);

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
        protected Task<bool> FindAnswerAsync(Question question, Message response, CancellationToken cancel)
        {
            // Find a node for the question name.
            if (!Catalog.TryGetValue(question.Name, out Node node))
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

        void AddAdditionalRecords(Message response)
        {
            var extras = new Message();
            var resources = response.Answers.Concat(response.AuthorityRecords);
            var question = new Question();
            bool _;
            foreach (var resource in resources)
            {
                switch (resource.Type)
                {
                    case DnsType.NS:
                        FindAddresses(((NSRecord)resource).Authority, resource.Class, extras);
                        break;

                    case DnsType.PTR:
                        FindAddresses(((PTRRecord)resource).DomainName, resource.Class, extras);
                        break;

                    case DnsType.SOA:
                        FindAddresses(((SOARecord)resource).PrimaryName, resource.Class, extras);
                        break;

                    case DnsType.SRV:
                        question.Class = resource.Class;
                        question.Name = resource.Name;
                        question.Type = DnsType.TXT;
                        _ = FindAnswerAsync(question, extras, default(CancellationToken)).Result;

                        FindAddresses(((SRVRecord)resource).Target, resource.Class, extras);
                        break;

                    default:
                        break;
                }
            }

            response.AdditionalRecords.AddRange(extras.Answers);
        }

        void FindAddresses(string name, Class klass, Message response)
        {
            var question = new Question();

            question.Name = name;
            question.Class = klass;
            question.Type = DnsType.A;
            var _ = FindAnswerAsync(question, response, default(CancellationToken)).Result;

            question.Type = DnsType.AAAA;
            _ = FindAnswerAsync(question, response, default(CancellationToken)).Result;
        }
    }
}
