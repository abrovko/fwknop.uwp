# fwknop.uwp
Universal Windows Platform (UWP) application - Single Packet Authorization (SPA) client (an alternative to port knocking).

See https://www.cipherdyne.org/fwknop/docs/fwknop-tutorial.html for more details about Single Packet Authentication and fwknop . 

Client only supports AES (Rijndael) encryption and HMAC SHA-256 signing. Tested with fwknopd server 2.6.9 running on OpenWrt


I did not publish this app to Windows Store. Just side-loaded to my device(s). I may consider publishing in the future if there is a demand. 

The interesting part (SPA Packet generation encryption and signing) is <a href='fwknop.uwp/src/fwknop.uwp/Spa/SpaGenerator.cs'>here</a> and it is direct port of the fwknop-2.6.8 library.


Couple of screenshots just to give an idea of what it is

<img src="https://github.com/abrovko/fwknop.uwp/blob/master/wp_ss_20170530_0001.png" width="350px"/> <img src="https://github.com/abrovko/fwknop.uwp/blob/master/wp_ss_20170530_0002.png" width="350px"/>
