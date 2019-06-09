# Catalog

A [Catalog](xref:Makaretu.Dns.Resolving.Catalog) contains a portion 
of the DNS database for a group of nodes.  Typically, a catalog is also a zone. 
Each [Node](xref:Makaretu.Dns.Resolving.Node) contains the 
[resource records](rr.md) for a given domain name. 

## Zone

DNS zones are represented by a Catalog and can be loaded from a [master file](master-file.md) 
with the [IncludeZone()](xref:Makaretu.Dns.Resolving.Catalog.IncludeZone*) method.

## Root hints

Use [IncludeRootHints](xref:Makaretu.Dns.Resolving.Catalog.IncludeRootHints)
to include the addresses of the root name servers.

A DNS recursive resolver typically needs a "root hints file". This file 
contains the names and IP addresses of the authoritative name servers for the root zone, 
so the software can bootstrap the DNS resolution process.

## Reverse lookups

Use [IncludeReverseLookupRecords](xref:Makaretu.Dns.Resolving.Catalog.IncludeReverseLookupRecords)
to add PTR records for each authoritative A/AAAA record.

A forward-confirmed reverse DNS (FCrDNS) verification can create a 
form of authentication showing a valid relationship between the 
owner of a domain name and the owner of the server that has been 
given an IP address. While not very thorough, this validation is 
strong enough to often be used for whitelisting purposes, 
since spammers and phishers usually cannot achieve forward validation
when they use zombie computers to forge domain records.
