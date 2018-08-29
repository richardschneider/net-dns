# Resource Records

A [resource record](xref:Makaretu.Dns.ResourceRecord) (RR) contains some information on the named resource.  These records are found in the 
[Answers](xref:Makaretu.Dns.Message.Answers), 
[AuthorityRecords](xref:Makaretu.Dns.Message.AuthorityRecords) and
[AdditionalRecords](xref:Makaretu.Dns.Message.AdditionalRecords) properties of a message.

The following resource records are implemented

- [A](xref:Makaretu.Dns.ARecord) IPv4
- [AAAA](xref:Makaretu.Dns.AAAARecord) IPv6
- [AFSDB](xref:Makaretu.Dns.AFSDBRecord) AFS Database
- [CNAME](xref:Makaretu.Dns.CNAMERecord) Alias
- [DNAME](xref:Makaretu.Dns.DNAMERecord) Alias for a name and all its subnames
- [DNSKEY](xref:Makaretu.Dns.DNSKEYRecord) Public Key
- [DS](xref:Makaretu.Dns.DSRecord) Delegation Signer
- [HINFO](xref:Makaretu.Dns.HINFORecord) Host Info
- [MX](xref:Makaretu.Dns.MXRecord) Mail Exchange
- [NS](xref:Makaretu.Dns.NSRecord) Name Server
- [NSEC](xref:Makaretu.Dns.NSECRecord) Next Secure
- [NSEC3](xref:Makaretu.Dns.NSEC3Record) Authenticated Next Secure
- [NSEC3PARAM](xref:Makaretu.Dns.NSEC3PARAMRecord) Authenticated Next Secure Parameters
- [NULL](xref:Makaretu.Dns.NULLRecord) EXPERIMENTAL
- [OPT](xref:Makaretu.Dns.OPTRecord) Extension mechanism for DNS
- [PTR](xref:Makaretu.Dns.PTRRecord) Domain Name Pointer
- [RP](xref:Makaretu.Dns.RPRecord) Responsible Person
- [RRSIG](xref:Makaretu.Dns.RRSIGRecord) Signature for a RRSET
- [SOA](xref:Makaretu.Dns.SOARecord) Start Of Authority
- [SRV](xref:Makaretu.Dns.SRVRecord) Servers
- [TKEY](xref:Makaretu.Dns.TKEYRecord) Shared Secret Key
- [TSIG](xref:Makaretu.Dns.TSIGRecord) Transactional Signature
- [TXT](xref:Makaretu.Dns.TXTRecord) Freeform text

For all other resource records an [UnknownRecord](xref:Makaretu.Dns.UnknownRecord) is used.

