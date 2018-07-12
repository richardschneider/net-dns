using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    ///   All communications inside of the domain protocol are carried in a single
    ///   format called a message.
    /// </summary>
    public class Message : DnsObject
    {
        /// <summary>
        ///   The least significant 4 bits of the opcode.
        /// </summary>
        byte opcode4;

        /// <summary>
        ///   Maximum bytes of a message.
        /// </summary>
        /// <value>
        ///   9000 bytes.
        /// </value>
        /// <remarks>
        ///   In reality the max length is dictated by the network MTU.  For legacy IPv4 systems,
        ///   512 bytes should be used.  For DNSSEC, at least 4096 bytes are needed.
        ///   <para>
        ///   9000 bytes (less IP and UPD header lengths) is specified by Multicast DNS.
        ///   </para>
        /// </remarks>
        public const int MaxLength = 9000;

        /// <summary>
        ///   Minimum bytes of a messages
        /// </summary>
        /// <value>
        ///   12 bytes.
        /// </value>
        public const int MinLength = 12;

        /// <summary>
        /// A 16 bit identifier assigned by the program that
        /// generates any kind of query. This identifier is copied
        /// the corresponding reply and can be used by the requester
        /// to match up replies to outstanding queries.
        /// </summary>
        /// <value>
        ///   A unique identifier.
        /// </value>
        public ushort Id { get; set; }

        /// <summary>
        ///   A one bit field that specifies whether this message is a query(0), or a response(1).
        /// </summary>
        /// <value>
        ///   <b>false</b> for a query; otherwise, <b>true</b> for a response.
        /// </value>
        public bool QR { get; set; }

        /// <summary>
        ///   Determines if the message is query.
        /// </summary>
        /// <value>
        ///   <b>true</b> for a query; otherwise, <b>false</b> for a response.
        /// </value>
        public bool IsQuery { get { return !QR; } }

        /// <summary>
        ///   Determines if the message is a response to a query.
        /// </summary>
        /// <value>
        ///   <b>false</b> for a query; otherwise, <b>true</b> for a response.
        /// </value>
        public bool IsResponse { get { return QR; } }


        /// <summary>
        ///   The requested operation. 
        /// </summary>
        /// <value>
        ///   One of the <see cref="MessageOperation"/> values. Both standard
        ///   and extended values are supported.
        /// </value>
        /// <remarks>
        ///   This value is set by the originator of a query
        ///   and copied into the response.
        ///   <para>
        ///   Extended opcodes (values requiring more than 4 bits) are split between
        ///   the message header and the <see cref="OPTRecord"/> in the
        ///   <see cref="AdditionalRecords"/> section.  When setting an extended opcode,
        ///   the <see cref="OPTRecord"/> will be created if it does not already
        ///   exist.
        ///   </para>
        /// </remarks>
        /// <seealso cref="Message.CreateResponse"/>
        public MessageOperation Opcode
        {
            get
            {
                var opt = AdditionalRecords.OfType<OPTRecord>().FirstOrDefault();
                if (opt == null)
                    return (MessageOperation)opcode4;
                return (MessageOperation)(((ushort)opt.Opcode8 << 4) | opcode4);
            }
            set
            {
                var opt = AdditionalRecords.OfType<OPTRecord>().FirstOrDefault();

                // Is standard opcode?
                var extendedOpcode = (int)value;
                if ((extendedOpcode & 0xff0) == 0)
                {
                    opcode4 = (byte)extendedOpcode;
                    if (opt != null)
                        opt.Opcode8 = 0;
                    return;
                }

                // Extended opcode, needs an OPT resource record.
                if (opt == null)
                {
                    opt = new OPTRecord();
                    AdditionalRecords.Add(opt);
                }
                opcode4 = (byte)(extendedOpcode & 0xf);
                opt.Opcode8 = (byte)((extendedOpcode >> 4) & 0xff);
            }
        }

        /// <summary>
        ///    Authoritative Answer - this bit is valid in responses,
        ///    and specifies that the responding name server is an
        ///    authority for the domain name in question section.
        ///    
        ///    Note that the contents of the answer section may have
        ///    multiple owner names because of aliases.The AA bit
        ///    corresponds to the name which matches the query name, or
        ///    the first owner name in the answer section.
        /// </summary>
        /// <value>
        ///   <b>true</b> for an authoritative answer; otherwise, <b>false</b>.
        /// </value>
        public bool AA { get; set; }

        /// <summary>
        ///   TrunCation - specifies that this message was truncated
        ///   due to length greater than that permitted on the
        ///   transmission channel.
        /// </summary>
        /// <value>
        ///   <b>true</b> for a truncated message; otherwise, <b>false</b>.
        /// </value>
        public bool TC { get; set; }

        /// <summary>
        ///    Recursion Desired - this bit may be set in a query and
        ///    is copied into the response. If RD is set, it directs
        ///    the name server to pursue the query recursively.
        ///    
        ///    Recursive query support is optional.
        /// </summary>
        /// <value>
        ///   <b>true</b> if recursion is desired; otherwise, <b>false</b>.
        /// </value>
        public bool RD { get; set; }

        /// <summary>
        ///    Recursion Available - this be is set or cleared in a
        ///    response, and denotes whether recursive query support is
        ///    available in the name server.
        /// </summary>
        /// <value>
        ///   <b>true</b> if recursion is available; otherwise, <b>false</b>.
        /// </value>
        public bool RA { get; set; }

        /// <summary>
        ///    Reserved for future use. 
        /// </summary>
        /// <value>
        ///    Must be zero in all queries and responses.
        /// </value>
        public int Z { get; set; }

        /// <summary>
        ///     Response code - this 4 bit field is set as part of responses.
        /// </summary>
        /// <value>
        ///   One of the <see cref="MessageStatus"/> values.
        /// </value>
        public MessageStatus Status { get; set; }

        /// <summary>
        ///   The list of question.
        /// </summary>
        /// <value>
        ///   A list of questions.
        /// </value>
        public List<Question> Questions { get; } = new List<Question>();

        /// <summary>
        ///   The list of answers.
        /// </summary>
        /// <value>
        ///   A list of answers.
        /// </value>
        public List<ResourceRecord> Answers { get; } = new List<ResourceRecord>();

        /// <summary>
        ///   The list of authority records.
        /// </summary>
        /// <value>
        ///   A list of authority resource records.
        /// </value>
        public List<ResourceRecord> AuthorityRecords { get; } = new List<ResourceRecord>();

        /// <summary>
        ///   The list of additional records.
        /// </summary>
        /// <value>
        ///   A list of additional resource records.
        /// </value>
        public List<ResourceRecord> AdditionalRecords { get; } = new List<ResourceRecord>();

        /// <summary>
        ///   Create a response for the query message.
        /// </summary>
        /// <returns>
        ///   A new response for the query message.
        /// </returns>
        public Message CreateResponse()
        {
            return new Message
            {
                Id = Id,
                Opcode = Opcode,
                QR = true
            };
        }
        /// <inheritdoc />
        public override IDnsSerialiser Read(DnsReader reader)
        {
            Id = reader.ReadUInt16();
            var flags = reader.ReadUInt16();
            QR = (flags & 0x8000) == 0x8000;
            AA = (flags & 0x0400) == 0x0400;
            TC = (flags & 0x0200) == 0x0200;
            RD = (flags & 0x0100) == 0x0100;
            RA = (flags & 0x0080) == 0x0080;
            opcode4 = (byte)((flags & 0x7800) >> 11);
            Z = (flags & 0x0070) >> 4;
            Status = (MessageStatus)(flags & 0x000F);
            var qdcount = reader.ReadUInt16();
            var ancount = reader.ReadUInt16();
            var nscount = reader.ReadUInt16();
            var arcount = reader.ReadUInt16();
            for (var i = 0; i < qdcount; ++i)
            {
                var question = (Question) new Question().Read(reader);
                Questions.Add(question);
            }
            for (var i = 0; i < ancount; ++i)
            {
                var rr = (ResourceRecord) new ResourceRecord().Read(reader);
                Answers.Add(rr);
            }
            for (var i = 0; i < nscount; ++i)
            {
                var rr = (ResourceRecord)new ResourceRecord().Read(reader);
                AuthorityRecords.Add(rr);
            }
            for (var i = 0; i < arcount; ++i)
            {
                var rr = (ResourceRecord)new ResourceRecord().Read(reader);
                AdditionalRecords.Add(rr);
            }

            return this;
        }

        /// <inheritdoc />
        public override void Write(DnsWriter writer)
        {
            writer.WriteUInt16(Id);
            var flags =
                (Convert.ToInt32(QR) << 15) |
                (((ushort)opcode4 & 0xf)<< 11) |
                (Convert.ToInt32(AA) << 10) |
                (Convert.ToInt32(TC) << 9) |
                (Convert.ToInt32(RD) << 8) |
                (Convert.ToInt32(RA) << 7) |
                ((Z & 0x7) << 4) |
                ((ushort)Status & 0xf);
            writer.WriteUInt16((ushort)flags);
            writer.WriteUInt16((ushort)Questions.Count);
            writer.WriteUInt16((ushort)Answers.Count);
            writer.WriteUInt16((ushort)AuthorityRecords.Count);
            writer.WriteUInt16((ushort)AdditionalRecords.Count);
            foreach (var r in Questions) r.Write(writer);
            foreach (var r in Answers) r.Write(writer);
            foreach (var r in AuthorityRecords) r.Write(writer);
            foreach (var r in AdditionalRecords) r.Write(writer);
        }
    }
}
