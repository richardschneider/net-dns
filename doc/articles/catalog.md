# Catalog

A [Catalog](xref:Makaretu.Dns.Resolving.Catalog) contains a portion 
of the DNS database for a group of nodes.  Typically, a catalog is also a zone. 
Each [Node](xref:Makaretu.Dns.Resolving.Node) contains the 
[resource records](rr.md) for a given domain name. 

## Zone

DNS zones are represented by a Catalog and can be loaded from a [master file](master-file.md) 
with the [IncludeZone()](xref:Makaretu.Dns.Resolving.Catalog.IncludeZone*) method.