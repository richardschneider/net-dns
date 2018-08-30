# Registry

The [resource registry](xref:Makaretu.Dns.ResourceRegistry) contains the known resource records. 

## Creating

The [Create](xref:Makaretu.Dns.ResourceRegistry.Create*) method creates a 
new instance of a [resource record](xref:Makaretu.Dns.ResourceRecord) with the 
specified [DnsType](xref:Makaretu.Dns.DnsType).

```csharp
var rr = ResourceRegistry.Create(DnsType.NS);
```

## Registering

The [Register](xref:Makaretu.Dns.ResourceRegistry.Register*) method adds a new resource record type.


```csharp
public class XRecord : ResourceRecord
{
	public XRecord() : base()
    {
        Type = (DnsType)4242;
    }

    // TODO serialisation methods
}

ResourceRegistry.Register<XRecord>();
```
