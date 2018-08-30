# Cryptography

DNSSEC uses secure hashes and asymmetric keys to sign and authenticate 
[resource records](xref:Makaretu.Dns.ResourceRecord).  The 
[RRSIG](xref:Makaretu.Dns.RRSIGRecord) is used to sign a group of resources of
the same [type](xref:Makaretu.Dns.DnsType).  The 
[DNSKEY](xref:Makaretu.Dns.DNSKEYRecord) is used to verify the signature.

### Digest Registry

The [digest registry](xref:Makaretu.Dns.DigestRegistry) contains the implemented hash 
algorithms (Sha1, Sha256, Sha384 and Sha512).

To support a new digest type create its implementation and add it to registry.

```csharp
public class ShaX : HashAlgorithm
{
	...
}

DigestRegisty.Digests.Add(DigestType.ShaX, () => ShaX.Create());

```

### Security Registry

The [security registry](xref:Makaretu.Dns.SecurityAlgorithmRegistry) contains the implemented signing 
algorithms.
