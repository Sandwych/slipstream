#!env ruby
#encoding: utf-8
#
require 'ffi-rzmq'
require 'json'
require 'benchmark'
require 'rpcjson'


if RUBY_PLATFORM.downcase.include?("mswin") or RUBY_PLATFORM.downcase.include?("mingw") then
  STDOUT.set_encoding Encoding.locale_charmap
end

class HttpProxy

  def initialize(url)
    @client = RPC::JSON::Client.new(url, 1.1)
  end

  def close()
    #短连接，不需要关闭
  end

  def call(method, *args)
    @client.method_missing(method, *args)
  end

end

class ZmqProxy

  def initialize(url)
    @zcontext = ZMQ::Context.new()
    @zsocket = @zcontext.socket(ZMQ::REQ)
    @zsocket.connect(url)
    @id_count = 0
  end

  def close()
    if @zsocket != nil then
      @zsocket.close
      @zsocket = nil
    end
  end

  def call(method, *args)
    @id_count += 1
    jobj = { 'method' => method, 'params' => args, 'id' => @id_count.to_s() }
    jstr = JSON::generate(jobj)
    @zsocket.send_string(jstr)
    jstr = @zsocket.recv_string()
    jobj = JSON::parse(jstr)
    #TODO 错误检查
    return jobj['result']
  end

end

class ServiceProxy

  def initialize(proxy_class, url)
    @url = url
    @logged = false
    @session_id = nil
    @proxy = proxy_class.new(url)
  end

  def close()
    if @logged then
      self.logOff()
    end
    @proxy.close()
  end

  def call(method, *args)
    @proxy.call(method, *args)
  end

  def echo(arg)
    return self.call("system.echo", arg)
  end
  
  def listMethods()
    return self.call('system.listMethods')
  end

  def logOn(dbname, user, password)
    #错误检查
    @session_id = self.call("logOn", dbname, user, password)
    @logged = true
  end

  def logOff()
    self.call("logOff", @session_id)
    @logged = false
  end

  def execute(model_name, service, *args)
    self.call("execute", @session_id, model_name, service, [*args])
  end

  def method_missing(method, *args, &block)
    self.call(method, *args)
  end

end

#proxy = ServiceProxy.new(ZmqProxy, 'tcp://localhost:5555')
proxy = ServiceProxy.new(HttpProxy, 'http://localhost:9287/jsonrpc')

puts proxy.echo("Hello!")

puts "列出系统方法:"
puts proxy.listMethods()

#尝试登录
proxy.logOn("objectserver", "root", "root")

#尝试查询模块表里的核心模块
domain = [["name","like","core%"]]
ids = proxy.execute('core.model', 'Search', domain, nil, 0, 100);

#读取查询到的核心模块信息
fields = ['name', 'label', 'info']

models = proxy.execute('core.model', 'Read', ids, fields);
puts "查询出的模型："
for m in models
    puts m
end

TIMES = 200
puts "测试查询和读取上面的数据[#{TIMES}]次"
time = Benchmark.measure do
  for i in 1..TIMES
    ids = proxy.execute('core.model', 'Search', domain, nil, 0, 100);
    models = proxy.execute('core.model', 'Read', ids, fields);
    print '.'
  end
end
puts 
puts "耗时（秒）："
puts time

proxy.logOff()

proxy.close()

