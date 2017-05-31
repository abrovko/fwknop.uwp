# fwknop.uwp
Universal Windows Platform (UWP) application - Single Packet Authorization (SPA) client (an alternative to port knocking).

See https:/www.cipherdyne.org/fwknop/docs/fwknop-tutorial.html for more details about Single Packet Authentication and fwknop . 

Client only supports AES (Rijndael) encryption and HMAC SHA-256 signing. Tested with fwknopd server 2.6.9 running on OpenWrt

The interesting part (SPA Packet generation encryption and signing) is <a href='fwknop.uwp/src/fwknop.uwp/Spa/SpaGenerator.cs'>here</a> it is direct port of the fwknop-2.6.8 library.
