#!/usr/bin/ruby

thisdir = File.dirname(__FILE__)
topdir = File.expand_path(File.dirname(thisdir))

require "optparse"
require File.join(thisdir, "settings.rb")
require File.join(thisdir, "redirects.rb")

Settings.init!

Redirects.create_redirects
