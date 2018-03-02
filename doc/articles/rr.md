# Resource Records

A resource record (RR) contains some information on the named resource.  These records are found in the 
[Answers](xref:Makaretu.Dns.Message.Answers), 
[AuthorityRecords](xref:Makaretu.Dns.Message.AuthorityRecords) and
[AdditionalRecords](xref:Makaretu.Dns.Message.AdditionalRecords) properties of a message.

## Supported

- [A](xref:Makaretu.Dns.ARecord)
- [AAAA](xref:Makaretu.Dns.AAAARecord)
- [CNAME](xref:Makaretu.Dns.CNAMERecord)
- [DNAME](xref:Makaretu.Dns.DNAMERecord)
- [HINFO](xref:Makaretu.Dns.HINFORecord)
- [MX](xref:Makaretu.Dns.MXRecord)
- [NS](xref:Makaretu.Dns.NSRecord)
- [NULL](xref:Makaretu.Dns.NULLRecord)
- [PTR](xref:Makaretu.Dns.PTRRecord)
- [SOA](xref:Makaretu.Dns.SOARecord)
- [SRV](xref:Makaretu.Dns.SRVRecord)
- [TXT](xref:Makaretu.Dns.TXTRecord)

## Unknown

For all other resource records an [UnknownRecord](xref:Makaretu.Dns.UnknownRecord) can be used.
