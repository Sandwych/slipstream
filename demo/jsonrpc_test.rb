#!env ruby
#encoding: utf-8
#
require 'ffi-rzmq'
require 'json'

if RUBY_PLATFORM.downcase.include?("mswin") or RUBY_PLATFORM.downcase.include?("mingw") then
  STDOUT.set_encoding Encoding.locale_charmap
end

class ServiceProxy

  def initialize(url)
    @url = url
    @id_count = 0;
    @zcontext = ZMQ::Context.new()
    @zsocket = @zcontext.socket(ZMQ::REQ)
    @zsocket.connect(url)
  end

  def call(method, *args)
    jobj = { 'method' => method, 'params' => args, 'id' => @id_count.to_s() }
    jstr = JSON::generate(jobj)
    @zsocket.send_string(jstr)
    jstr = @zsocket.recv_string()
    jobj = JSON::parse(jstr)
    return jobj['result']
  end

  def echo(arg)
    return self.call("system.echo", arg)
  end
  
  def listMethods()
    return self.call('system.listMethods')
  end

  def method_missing(method, *args, &block)
    self.call(method, *args)
  end

end

proxy = ServiceProxy.new('tcp://localhost:5555')

puts proxy.echo("Hello!")

puts "Methods:"
puts proxy.listMethods()

#尝试登录
sid = proxy.logOn("objectserver", "root", "root")
puts "sid=#{sid}"

#尝试查询模块表里的核心模块
domain = [["name","like","core%"]]
ids = proxy.execute(sid, 'core.model', 'Search', [domain, nil, 0, 100]);
puts "\nIDs:", ids

#读取查询到的核心模块信息
fields = ['name', 'label', 'info']

models = proxy.execute(sid, 'core.model', 'Read', [ids, fields]);
puts "\nModels:"
for m in models
    puts m
end

proxy.logOff(sid)

