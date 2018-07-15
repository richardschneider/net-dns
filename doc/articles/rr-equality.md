# Equality

Two [resource records](xref:Makaretu.Dns.ResourceRecord) are considered equal if their [Name](xref:Makaretu.Dns.ResourceRecord.Name), 
[Class](xref:Makaretu.Dns.ResourceRecord.Class), [Type](xref:Makaretu.Dns.ResourceRecord.Type) 
and resource specific data fields are equal.

> [!NOTE]
> The [TTL](xref:Makaretu.Dns.ResourceRecord.TTL) and  [CreationTime](xref:Makaretu.Dns.DnsObject.CreationTime) fields are 
explicitly excluded from the comparison.
