# Resolving

The [IResolver](xref:Makaretu.Dns.IResolver) interface defines the
**ResolveAsync** method that is used to answer a [query message](message.md).

## Resolvers

The following resolvers are available

| Name | Description |
| ---- | ----------- |
| [NameServer](name-server.md) | Finds an answer in a [Catalog](catalog.md) |

## Example

Getting the IPv4 addresses of `ns.example.com`

```csharp
IResolver resolver = ...

var query = new Message();
query.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.A });
var answer = await resolver.ResolveAsync(request);
```

The answer is
```
;; Header: QR AA RCODE=NoError

;; Answer
ns.example.com 3600 IN A 192.0.2.2

```
