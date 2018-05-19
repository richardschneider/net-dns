# Extended Opcode

Extended opcodes (values requiring more than 4 bits) are split between
the message header and the [OPT resource record](xref:Makaretu.Dns.OPTRecord).

When [setting](xref:Makaretu.Dns.Message.Opcode) an extended opcode, the OPT record is created if 
it does not already exist.