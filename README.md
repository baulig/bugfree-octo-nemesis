bugfree-octo-nemesis
====================

Web tests for Mono.

Prerequisites
-------------

You need a server with the following software:

* Apache
* Samba
* Squid 2.7 (do not use Squid3 on SLED 11, it does not work, use the 'squid' package instead)
* A valid SSL certificate

The following installation instructions are for a SuSE-Linux based server - I'm running SLES 11
in an AWS-VPC micro instance.

Samba
-----

This is pretty much straightforward.  You need samba and samba-winbind.

Squid
-----

You need Squid 2.7 - on SuSE, a simple "zypper install squid" will do.

Use the /usr/bin/ntlm_auth that comes with samba-winbind, not the one that's included in squid!

This is what I'm using:

    auth_param ntlm program /usr/bin/ntlm_auth --helper-protocol=squid-2.5-ntlmssp
    auth_param ntlm children 1
    auth_param ntlm keep_alive off
    acl authenticated proxy_auth REQUIRED

Unfortunately, squid will eat all debugging output from the helper process, but you can do a little trick
here; create /etc/squid/ntlm-auth-wrapper.sh:

    #!/bin/sh
    /usr/bin/ntlm_auth -E -d 9 --helper-protocol=squid-2.5-ntlmssp 2>/var/log/squid/ntlm-auth.log

To debug auth problems:

    debug_options ALL,1 33,9 28,9

Require auth:

    http_access allow all authenticated

SSL:

    http_port 3128
    https_port 3129 cert=/etc/apache2/cert/server.crt key=/etc/apache2/cert/server.key

If you want to configure it as a proxy only:

    cache deny all
    
