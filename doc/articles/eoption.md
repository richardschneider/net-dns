# Extended Options

An [option](xref:Makaretu.Dns.EdnsOption) specifies some extra behaviour that is
required from the server.

The following EDNS options are implemented

- [Keepalive](xref:Makaretu.Dns.EdnsKeepaliveOption) - [RFC 7828](https://tools.ietf.org/html/rfc7828)
- [NSID](xref:Makaretu.Dns.EdnsNSIDOption) - [RFC 5001](https://tools.ietf.org/html/rfc5001)
- [Padding](xref:Makaretu.Dns.EdnsPaddingOption) - [RFC 7830](https://tools.ietf.org/html/rfc7830)

For all other options the [UnknownEdnsOption](xref:Makaretu.Dns.UnknownEdnsOption) is used.

