# Message Trucation

A [DNS message](xref:Makaretu.Dns.Message) is typically sent over UDP which has a maximum packet size. 
[Truncate(length)](xref:Makaretu.Dns.Message.Truncate) is used to adjust the message to fit into the packet size, 
by removing [additional records](xref:Makaretu.Dns.Message.AdditionalRecords) and then 
[authority records](xref:Makaretu.Dns.Message.AuthorityRecords).

If it is still too big, then the [TC](xref:Makaretu.Dns.Message.TC) bit is set.
