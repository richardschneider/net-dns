# Master File

A `master file` is a text file consisting or directives and resource records in 
the [presentation format](presentation.md). Comments start with 
a semi-colon (`;`) and blank lines are allowed.  The file is typically used to define 
a DNS Zone.

## Example

```
; Set zone name and default timeout
$ORIGIN example.org.
$TTL 3600

@    SOA   ns1 username.example.org. ( 2007120710 1 2 4 1 )
     NS    ns1
     NS    ns2
     MX    10 mail

; Host addresses
ns1  A     192.0.2.1
ns2  A     192.0.2.2
mail A     192.0.2.3
 ```

