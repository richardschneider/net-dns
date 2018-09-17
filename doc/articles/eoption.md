# Extended Options

An [option](xref:Makaretu.Dns.EdnsOption) specifies some extra behaviour that is
required from the server.

The following EDNS options are implemented

- [DAU](xref:Makaretu.Dns.EdnsDAUOption) - [RFC 6975](https://tools.ietf.org/html/rfc6975)
- [DHU](xref:Makaretu.Dns.EdnsDHUOption) - [RFC 6975](https://tools.ietf.org/html/rfc6975)
- [Keepalive](xref:Makaretu.Dns.EdnsKeepaliveOption) - [RFC 7828](https://tools.ietf.org/html/rfc7828)
- [N3U](xref:Makaretu.Dns.EdnsN3UOption) - [RFC 6975](https://tools.ietf.org/html/rfc6975)
- [NSID](xref:Makaretu.Dns.EdnsNSIDOption) - [RFC 5001](https://tools.ietf.org/html/rfc5001)
- [Padding](xref:Makaretu.Dns.EdnsPaddingOption) - [RFC 7830](https://tools.ietf.org/html/rfc7830)

For all other options the [UnknownEdnsOption](xref:Makaretu.Dns.UnknownEdnsOption) is used.

