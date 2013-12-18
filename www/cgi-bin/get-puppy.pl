#!/usr/bin/perl -wT
use strict;
use CGI qw/:cgi/;

# my $q = CGI->new();
# print $q->header('text/plain');
# my %params = $q->Vars;

print "Content-type: text/plain\n\n";

my $method = request_method();
my $path_info = path_info();
my $auth = auth_type();
my $remote = remote_addr();
my $port = $ENV{"REMOTE_PORT"};

print "METHOD: $method\n";
print "PATH: $path_info\n" if defined $path_info;
print "AUTH: $auth\n" if defined $auth;
print "REMOTE: $remote\n";
print "PORT: $port\n";
