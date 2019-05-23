# Pretty Printing

[Message.ToString()](xref:Makaretu.Dns.Message.ToString*) returns a human readable representation of the message.

Here's a response from Quad9 showing the TXT records for the `ipfs.io` domain.

```
;; Header: QR RD RCODE=NoError

;; Question
ipfs.io IN TXT

;; Answer
ipfs.io 3600 IN TXT "v=spf1 a include:_spf.google.com ~all"
ipfs.io 60 IN TXT dnslink=/ipns/website.ipfs.io

;; Authority
;;  (empty)

;; Additional
; EDNS: version: 0, udp 512
```