# Extended DNS

[RFC 6891](https://tools.ietf.org/html/rfc6891) defines a set of extension mechanisms for DNS. 
The [OPT resource record](xref:Makaretu.Dns.OPTRecord), 
in the [additional records](xref:Makaretu.Dns.Message.AdditionalRecords) section, 
is used to supply the [options](xref:Makaretu.Dns.EdnsOption).

```csharp
var msg = new Message
{
    RD = true,
    AdditionalRecords = new List<ResourceRecord>
    {
        new OPTRecord
        {
            RequestorPayloadSize = 512,
            Options = new List<EdnsOption>
            {
                new EdnsKeepaliveOption { Timeout = TimeSpan.FromSeconds(30) }
            }
        }
    }
};
```
