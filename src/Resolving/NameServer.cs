using System.Collections.Generic;
using System.Linq;
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
        public Task<Message> ResolveAsync(Message request, CancellationToken cancel = default(CancellationToken))
        {
            var response = request.CreateResponse();

            if (AnswerAllQuestions)
            {
                Parallel.ForEach(request.Questions, question => Resolve(question, response));
            }
            else
            {
                foreach (var question in request.Questions)
                {
                    if (response.Answers.Count > 0)
                    {
                        break;
                    }
                    Resolve(question, response);
                }
            }

            if (response.Answers.Count > 0)
            {
                response.Status = MessageStatus.NoError;
            }

            // Remove duplicate records.
            if (response.Answers.Count > 1)
            {
                response.Answers = response.Answers.Distinct().ToList();
            }
            if (response.AuthorityRecords.Count > 1)
            {
                response.AuthorityRecords = response.AuthorityRecords.Distinct().ToList();
            }

            // Remove additional records that are also answers.
            if (response.AdditionalRecords.Count > 0)
            {
                response.AdditionalRecords = response.AdditionalRecords
                    .Where(a => !response.Answers.Contains(a))
                    .ToList();
            }

            return Task.FromResult(response);
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
        public Task<Message> ResolveAsync(Question question, Message response = null, CancellationToken cancel = default(CancellationToken))
        {
            return Task.FromResult(Resolve(question, response));
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
        /// <returns>
        ///   A task that represents the asynchronous operation. The task's value is
        ///   a <see cref="Message"/> response to the <paramref name="question"/>.
        /// </returns>
        /// <remarks>
        ///   If the question's domain does not exist, then the closest authority
        ///   (<see cref="SOARecord"/>) is added to the <see cref="Message.AuthorityRecords"/>.
        /// </remarks>
        protected Message Resolve(Question question, Message response = null)
        {
            response = response ?? new Message { QR = true };

            if (!FindAnswer(question, response))
            {
                response.Status = MessageStatus.NameError;
            }

            // If a name error, then add the domain authority.
            if (response.Status == MessageStatus.NameError)
            {
                SOARecord soa = FindAuthority(question.Name);
                if (soa != null)
                {
                    lock (response.AuthorityRecords)
                    {
                        response.AuthorityRecords.Add(soa);
                    }
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
        /// <returns>
        ///   A task that represents the asynchronous operation.  The task's value
        ///   is <b>true</b> if the resolver added an answer.
        /// </returns>
        /// <remarks>
        ///   Derived classes must implement this method.
        /// </remarks>
        protected bool FindAnswer(Question question, Message response)
        {
            // Find a node for the question name.
            if (!Catalog.TryGetValue(question.Name, out Node node))
            {
                return false;
            }

            // https://tools.ietf.org/html/rfc1034#section-3.7.1
            response.AA |= node.Authoritative && question.Class != DnsClass.ANY;

            //  Find the resources that match the question.
            var resources = node.Resources
                .Where(r => question.Class == DnsClass.ANY || r.Class == question.Class)
                .Where(r => question.Type == DnsType.ANY || r.Type == question.Type)
                .Where(r => node.Authoritative || !r.IsExpired(question.CreationTime))
                .ToArray();
            if (resources.Length > 0)
            {
                lock (response.Answers)
                {
                    response.Answers.AddRange(resources);
                }

                return true;
            }

            // If node is alias (CNAME), then find answers for the alias' target.
            // The CNAME is added to the answers.
            var cname = node.Resources.OfType<CNAMERecord>().FirstOrDefault();
            if (cname != null)
            {
                response.Answers.Add(cname);
                question = question.Clone<Question>();
                question.Name = cname.Target;

                return FindAnswer(question, response);
            }

            return false;
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
            var resources = response.Answers
                .Concat(response.AdditionalRecords)
                .Concat(response.AuthorityRecords)
                .ToList();

            Parallel.ForEach(resources, resource =>
                Parallel.ForEach(BuildQuestion(resource),
                    question => FindAnswer(question, extras)));

            // Add extras with no duplication.
            extras.Answers = extras.Answers
                .Where(a => !response.Answers.Contains(a))
                .Where(a => !response.AdditionalRecords.Contains(a))
                .Distinct()
                .ToList();

            lock (response.AdditionalRecords)
            {
                response.AdditionalRecords.AddRange(extras.Answers);
            }

            // Add additionals for any extras.
            if (extras.Answers.Count > 0)
            {
                AddAdditionalRecords(response);
            }
        }

        IEnumerable<Question> BuildQuestion(ResourceRecord resource)
        {
            switch (resource.Type)
            {
                case DnsType.A:
                    yield return new Question
                    {
                        Class = resource.Class,
                        Name = resource.Name,
                        Type = DnsType.AAAA
                    };
                    break;

                case DnsType.AAAA:
                    yield return new Question
                    {
                        Class = resource.Class,
                        Name = resource.Name,
                        Type = DnsType.A
                    };
                    break;

                case DnsType.NS when resource is NSRecord nc:
                    yield return new Question
                    {
                        Class = resource.Class,
                        Name = nc.Authority,
                        Type = DnsType.A
                    };
                    yield return new Question
                    {
                        Class = resource.Class,
                        Name = nc.Authority,
                        Type = DnsType.AAAA
                    };
                    break;

                case DnsType.PTR when resource is PTRRecord ptr:
                    yield return new Question
                    {
                        Class = resource.Class,
                        Name = ptr.DomainName,
                        Type = DnsType.ANY
                    };
                    break;

                case DnsType.SOA when resource is SOARecord soa:
                    yield return new Question
                    {
                        Class = resource.Class,
                        Name = soa.PrimaryName,
                        Type = DnsType.A
                    };
                    yield return new Question
                    {
                        Class = resource.Class,
                        Name = soa.PrimaryName,
                        Type = DnsType.AAAA
                    };
                    break;

                case DnsType.SRV when resource is SRVRecord srv:
                    yield return new Question
                    {
                        Class = resource.Class,
                        Name = resource.Name,
                        Type = DnsType.TXT
                    };
                    yield return new Question
                    {
                        Class = resource.Class,
                        Name = srv.Target,
                        Type = DnsType.A
                    };
                    yield return new Question
                    {
                        Class = resource.Class,
                        Name = srv.Target,
                        Type = DnsType.AAAA
                    };
                    break;

                default:
                    break;
            }
        }
    }
}
