require "yaml"
require "optparse"

module Settings
  extend self
  
  @parameters = [ ]
  
  def load!(filename,config)
    sets = YAML::load_file(filename)
    set = sets[config]
    set.each do |k,v|
      define_parameter!(k,v)
    end
  end
  
  def set_parameter(name,value)
    self.send("#{name}=", value)
  end
  
  def define_parameter!(name,value=nil)
    return if @parameters.include?(name)
    @parameters.push(name)
    attr_accessor name
    define_method name do |*values|
      value = values.first
      value ? set_parameter(name,value) : instance_variable_get("@#{name}")
    end
    set_parameter(name,value)
  end
  
  def config(&block)
    instance_eval(&block)
  end
  
  def parse_arguments!(config_file,config_name)
    thisdir = File.dirname(__FILE__)
    topdir = File.expand_path(File.dirname(thisdir))

    define_parameter!("web_root", topdir)

    OptionParser.new do |opts|
      opts.banner = "Usage: __FILE__ [options]"
      
      opts.on('-f', '--config-file NAME', 'Configuration file') { |v| config_file = v }
      opts.on('-c', '--config-name NAME', 'Configuration name') { |v| config_name = v }
      opts.on('-r', '--web-root PATH', 'Root directory of your web server') { |v| web_root(v) }
    end.parse!
    
    load!(config_file, config_name)
  end
  
  def init!
    env_var_name = "BUGFREE_OCTO_NEMESIS"
    env_config = ENV.has_key?(env_var_name) ? ENV[env_var_name] : "default:default.yml"
    parts = env_config.split(':')
    if parts.length > 1
      config_name = parts[0]
      config_file = parts[1]
    else
      config_name = "default"
      config_file = "default.yml"
    end
    puts "Using configuration #{config_name} from #{config_file}."
    parse_arguments!(config_file,config_name)
  end
  
  def create_app_config(path)
    File.open(path, "w") do |f|
      f.write("<configuration>\n  <appSettings>\n")
      @parameters.each do |name|
        value = instance_variable_get("@#{name}")
        f.write("    <add key=\"#{name}\" value=\"#{value}\"/>\n")
      end
      f.write("  </appSettings>\n</configuration>\n")
    end
  end
end
