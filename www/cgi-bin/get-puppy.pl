#!/usr/bin/perl -wT
use strict;
use CGI qw/:cgi/;

my $q = CGI->new();

my $method = request_method();
my $path_info = path_info();
my $auth = auth_type();
my $remote = remote_addr();
my $port = $ENV{"REMOTE_PORT"};

my $output = "";
open OUTPUT, '>', \$output;

print OUTPUT "METHOD: $method\n";
print OUTPUT "PATH: $path_info\n" if defined $path_info;
print OUTPUT "AUTH: $auth\n" if defined $auth;
print OUTPUT "REMOTE: $remote\n";
print OUTPUT "PORT: $port\n";

close OUTPUT;

print "Content-Type: text/plain\n";

my $mode = $q->param("mode");
$mode = "default" unless defined $mode;

if ($mode eq "chunked") {
    print "Transfer-Encoding: chunked\n\n";
    my $length = length($output);
    printf "%x\r\n", $length;
    print "$output\r\n";
    print "0\r\n\r\n";
} elsif ($mode eq "length") {
    my $length = length($output);
    print "Content-Length: $length\n\n";
    print $output;
} else {
    print "\n";
    print $output;
}

