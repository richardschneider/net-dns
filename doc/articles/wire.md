# Wire Format

The wire format is the binary representation of a [DNS object](xref:Makaretu.Dns.DnsObject); 
typically a [Message](message.md) or a [Resource Record](rr.md).  The Write and Read methods, 
defined in [IWireSerialiser](xref:Makaretu.Dns.IWireSerialiser), are used to serialize and 
deserialise a DNS object.  

The [WireWriter](xref:Makaretu.Dns.WireWriter) and 
[WireReader](xref:Makaretu.Dns.WireReader) classes are used to encode and decode data types. 
Convenience methods to support a byte array or stream are 
also defined.
