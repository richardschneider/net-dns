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
- [HINFO](xref:Makaretu.Dns.HINFORecord) Host Info
- [MX](xref:Makaretu.Dns.MXRecord) Mail Exchange
- [NS](xref:Makaretu.Dns.NSRecord) Name Server
- [NULL](xref:Makaretu.Dns.NULLRecord) EXPERIMENTAL
- [OPT](xref:Makaretu.Dns.OPTRecord) Extension mechanism for DNS
- [PTR](xref:Makaretu.Dns.PTRRecord) Domain Name Pointer
- [RP](xref:Makaretu.Dns.RPRecord) Responsible Person
- [SOA](xref:Makaretu.Dns.SOARecord) Start Of Authority
- [SRV](xref:Makaretu.Dns.SRVRecord) Servers
- [TXT](xref:Makaretu.Dns.TXTRecord) Freeform text

For all other resource records an [UnknownRecord](xref:Makaretu.Dns.UnknownRecord) is used.

## Equality

Two [resource records](xref:Makaretu.Dns.ResourceRecord) are considered equal if their [Name](xref:Makaretu.Dns.ResourceRecord.Name), 
[Class](xref:Makaretu.Dns.ResourceRecord.Class), [Type](xref:Makaretu.Dns.ResourceRecord.Type) 
and resource specific data fields are equal.

> [!NOTE]
> The [TTL](xref:Makaretu.Dns.ResourceRecord.TTL) field is explicitly excluded from the comparison.
