# net-dns

[![build status](https://ci.appveyor.com/api/projects/status/github/richardschneider/net-dns?branch=master&svg=true)](https://ci.appveyor.com/project/richardschneider/net-dns) 
[![travis build](https://travis-ci.org/richardschneider/net-dns.svg?branch=master)](https://travis-ci.org/richardschneider/net-dns)
[![Coverage Status](https://coveralls.io/repos/richardschneider/net-dns/badge.svg?branch=master&service=github)](https://coveralls.io/github/richardschneider/net-dns?branch=master)
[![Version](https://img.shields.io/nuget/v/Makaretu.Dns.svg)](https://www.nuget.org/packages/Makaretu.Dns)
[![docs](https://cdn.rawgit.com/richardschneider/net-dns/master/doc/images/docs-latest-green.svg)](https://richardschneider.github.io/net-dns/articles/intro.html)

DNS data model with serializer/deserializer for the wire format.

## Features

- Targets .NET Standard 1.4 and 2.0
- CI on Travis (Ubuntu Trusty) and AppVeyor (Windows Server 2016)
- Supports compressed domain names
- Supports multiple strings in TXT records

## Getting started

Published releases are available on [NuGet](https://www.nuget.org/packages/Makaretu.Dns/).  To install, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console).

    PM> Install-Package Makaretu.Dns
    
## Usage

```
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

# License
Copyright © 2018 Richard Schneider (makaretu@gmail.com)

The package is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refer to the [LICENSE](https://github.com/richardschneider/net-dns/blob/master/LICENSE) file for more information.
