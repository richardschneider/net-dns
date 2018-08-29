# Makaretu DNS

A .Net data model for the [Domain Name System](https://www.rfc-editor.org/info/rfc1035). 
The source code is on [GitHub](https://github.com/richardschneider/net-dns) and the 
package is published on [NuGet](https://www.nuget.org/packages/Makaretu.Dns).

The [Message](message.md) object is used to create a query or a response. It is
serialised/deserialised with the [Write and Read](xref:Makaretu.Dns.DnsObject) methods, respectively.

The [UpdateMessage](xref:Makaretu.Dns.UpdateMessage) is used to create a dynamic DNS update.

The [common resource records](rr.md) are implemented.  To future proof the component, the 
[UnknownRecord](xref:Makaretu.Dns.UnknownRecord) is used.
