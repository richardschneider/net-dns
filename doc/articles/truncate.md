# Message Truncation

A [DNS message](xref:Makaretu.Dns.Message) is typically sent over UDP which has a maximum packet size. 
[Truncate](xref:Makaretu.Dns.Message.Truncate*) is used to adjust the message response to fit into the packet size, 
by removing [additional records](xref:Makaretu.Dns.Message.AdditionalRecords) and then 
[authority records](xref:Makaretu.Dns.Message.AuthorityRecords).

If it is still too big, then the [TC bit](xref:Makaretu.Dns.Message.TC) is set and the 
client is expected to send the query again over TCP.
