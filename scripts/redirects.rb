thisdir = File.dirname(__FILE__)
topdir = File.expand_path(File.dirname(thisdir))

require File.join(thisdir, "settings.rb")
require 'fileutils'

module Redirects
  extend self
  
  def create_redirects()
    root_dir = File.join(Settings.web_root, "redirects")
    create_root_htaccess(root_dir)
    create_redirect_same_server("redirects/same-server/301", 301, "www")
    create_redirect_same_server("redirects/same-server/302", 302, "www")
    create_redirect_same_server("redirects/same-server/303", 303, "www")
    create_redirect_same_server("redirects/same-server/307", 307, "www")
    create_redirect_switch_proto("redirects/to-http/301", 301, "http", "www")
    create_redirect_switch_proto("redirects/to-http/302", 302, "http", "www")
    create_redirect_switch_proto("redirects/to-http/303", 303, "http", "www")
    create_redirect_switch_proto("redirects/to-http/307", 307, "http", "www")
    create_redirect_switch_proto("redirects/to-https/301", 301, "https", "www")
    create_redirect_switch_proto("redirects/to-https/302", 302, "https", "www")
    create_redirect_switch_proto("redirects/to-https/303", 303, "https", "www")
    create_redirect_switch_proto("redirects/to-https/307", 307, "https", "www")
  end

  def create_redirect_same_server(dir, code, target)
      target_dir = File.join(Settings.web_root, dir)
      source_url = File.join(Settings.web_prefix, dir)
      target_url = File.join(Settings.web_prefix, target)
      create_redirect_full(target_dir, code, source_url, target_url)
  end
  
  def create_redirect_switch_proto(dir, code, proto, target)
      target_dir = File.join(Settings.web_root, dir)
      source_url = File.join(Settings.web_prefix, dir)
      target_url = proto + "://" + Settings.web_host + File.join(Settings.web_prefix, target)
      create_redirect_full(target_dir, code, source_url, target_url)
  end
  
  def create_redirect_full(dir, code, source, target)
    FileUtils.mkdir_p(dir) unless File.directory?(dir)
    htaccess = File.join(dir, ".htaccess")
    File.open(htaccess, "w") do |f|
      f.write("Redirect #{code} #{source} #{target}\n")
    end
  end

  def create_root_htaccess(dir)
    Dir.mkdir(dir) unless File.directory?(dir)
    htaccess = File.join(dir, ".htaccess")
    File.open(htaccess, "w") do |f|
      f.write("Options +Indexes\n")
    end
  end

end
