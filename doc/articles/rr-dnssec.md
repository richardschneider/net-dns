# Resource Records for DNSSEC

The following resource records are implemented for [DNSSEC](dnssec.md)

- [DNSKEY](xref:Makaretu.Dns.DNSKEYRecord) Public Key
- [DS](xref:Makaretu.Dns.DSRecord) Delegation Signer
- [NSEC](xref:Makaretu.Dns.NSECRecord) Next Secure
- [NSEC3](xref:Makaretu.Dns.NSEC3Record) Authenticated Next Secure
- [NSEC3PARAM](xref:Makaretu.Dns.NSEC3PARAMRecord) Authenticated Next Secure Parameters
- [OPT](xref:Makaretu.Dns.OPTRecord) Extension mechanism (EDNS)
- [RRSIG](xref:Makaretu.Dns.RRSIGRecord) Signature for a set of resource records

## Message Headers Flags

The following [message](xref:Makaretu.Dns.Message) header flags are defined

- [AD](xref:Makaretu.Dns.Message.AD) Authenticated Data
- [CD](xref:Makaretu.Dns.Message.CD) Checking Disabled 
- [DO](xref:Makaretu.Dns.OPTRecord.DO) DNSSEC OK EDNS header bit

