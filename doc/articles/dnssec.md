# DNSSEC

DNSSEC [RFC4035](https://tools.ietf.org/html/rfc4035) provides data integrity and 
authentication to security aware resolvers and applications through
the use of cryptographic digital signatures.

[DNSSEC – What Is It and Why Is It Important?](https://www.icann.org/resources/pages/dnssec-qaa-2014-01-29-en)

### Requesting DNSSEC

The client sends a [DNS query](message.md) with the [DO bit](xref:Makaretu.Dns.OPTRecord.DO) set. 
The server will then responsd with the answers and also the 
[RRSIGs](xref:Makaretu.Dns.RRSIGRecord)
that are used to validate the answers.

```csharp
// Request the IPv6 addresses for "example.com"
// RD is "recursion desired"
// DO enables signed answers.
var query = new Message()
{
    RD = true,
    DO = true,
    Questions =
    {
        new Question { Name = "example.com", Type = DnsType.AAAA }
    }
};

// Get an answer.
var response = await resolver.ResolveAsync(query);
```