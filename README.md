# net-dns

[![build status](https://ci.appveyor.com/api/projects/status/github/richardschneider/net-dns?branch=master&svg=true)](https://ci.appveyor.com/project/richardschneider/net-dns) 
[![travis build](https://travis-ci.org/richardschneider/net-dns.svg?branch=master)](https://travis-ci.org/richardschneider/net-dns)
[![Coverage Status](https://coveralls.io/repos/richardschneider/net-dns/badge.svg?branch=master&service=github)](https://coveralls.io/github/richardschneider/net-dns?branch=master)
[![Version](https://img.shields.io/nuget/v/Makaretu.Dns.svg)](https://www.nuget.org/packages/Makaretu.Dns)
[![docs](https://cdn.rawgit.com/richardschneider/net-dns/master/doc/images/docs-latest-green.svg)](https://richardschneider.github.io/net-dns/articles/intro.html)

DNS data model with serializer/deserializer for the wire and "master file" format.

## Features

- Serialization for the wire and master file formats
- Pretty printing of messages
- Supports compressed domain names
- Supports multiple strings in TXT records
- Supports the extended 12-bit RCODE
- Future proof: handles unknown resource records and EDNS options
- Graceful truncation of messages
- A name server that answeres DNS questions
- Data models for
  - [RFC 1035](https://tools.ietf.org/html/rfc1035) Domain Names (DNS)
  - [RFC 1183](https://tools.ietf.org/html/rfc1183) New DNS RR Definitions
  - [RFC 1996](https://tools.ietf.org/html/rfc1996) Zone Changes (DNS NOTIFY)
  - [RFC 2136](https://tools.ietf.org/html/rfc2136) Dynamic Updates (DNS UPDATE)
  - [RFC 2845](https://tools.ietf.org/html/rfc2845) Secret Key Transaction Authentication for DNS (TSIG)
  - [RFC 2930](https://tools.ietf.org/html/rfc2930) Secret Key Establishment for DNS (TKEY RR)
  - [RFC 3225](https://tools.ietf.org/html/rfc3225) Indicating Resolver Support of DNSSEC
  - [RFC 3599](https://tools.ietf.org/html/rfc3596) DNS Extensions to Support IPv6
  - [RFC 4034](https://tools.ietf.org/html/rfc4034) Resource Records for the DNS Security Extensions (DNSSEC)
  - [RFC 5001](https://tools.ietf.org/html/rfc5001) DNS Name Server Identifier (NSID) Option
  - [RFC 6672](https://tools.ietf.org/html/rfc6672) DNAME Redirection in the DNS
  - [RFC 6891](https://tools.ietf.org/html/rfc6891) Extension Mechanisms for DNS (EDNS(0))
  - [RFC 7828](https://tools.ietf.org/html/rfc7828) The edns-tcp-keepalive EDNS0 Option
  - [RFC 7830](https://tools.ietf.org/html/rfc7830) The EDNS(0) Padding Option
- Targets .Net Framework 4.5 and 4.7.2 and .NET Standard 1.4 and 2.0
- CI on Travis (Ubuntu Trusty and OSX) and AppVeyor (Windows Server 2016)

## Getting started

Published releases are available on [NuGet](https://www.nuget.org/packages/Makaretu.Dns/).  To install, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console).

    PM> Install-Package Makaretu.Dns
    
## Usage

### Name Server

Create a name server that can answer questions for a zone.

```csharp
using Makaretu.Dns.Resolving;

var catalog = new Catalog();
catalog.IncludeZone(...);
catalog.IncludeRootHints();
var resolver = new NameServer { Catalog = catalog };
```

Answer a question

```csharp
var request = new Message();
request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.AAAA });
var response = await resolver.ResolveAsync(request);
```

### Data Model

```csharp
using Makaretu.Dns

var msg = new Message
{
	AA = true,
	QR = true,
	Id = 1234
};
msg.Questions.Add(new Question 
{ 
	Name = "emanon.org" 
});
msg.Answers.Add(new ARecord 
{ 
	Name = "emanon.org",
	Address = IPAddress.Parse("127.0.0.1") 
});
msg.AuthorityRecords.Add(new SOARecord
{
	Name = "emanon.org",
	PrimaryName = "erehwon",
	Mailbox = "hostmaster.emanon.org"
});
msg.AdditionalRecords.Add(new ARecord 
{ 
	Name = "erehwon", 
	Address = IPAddress.Parse("127.0.0.1") 
});

```

# Related projects

- [net-mdns](https://github.com/richardschneider/net-mdns) - client and server for multicast DNS
- [net-udns](https://github.com/richardschneider/net-udns) - client for unicast DNS, DNS over HTTPS (DOH) and DNS over TLS (DOT)
- [DNSSEC](https://www.icann.org/resources/pages/dnssec-qaa-2014-01-29-en) -  What Is It and Why Is It Important?
 
# License
Copyright © 2018 Richard Schneider (makaretu@gmail.com)

The package is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refer to the [LICENSE](https://github.com/richardschneider/net-dns/blob/master/LICENSE) file for more information.

<a href="https://www.buymeacoffee.com/kmXOxKJ4E" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/yellow_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>
