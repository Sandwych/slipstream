#encoding: utf-8
# bowling_spec.rb 

class Echo

  def echo(msg)
    msg
  end

end

describe Echo do 
  before(:each) do 
    @echo = Echo.new 
  end

  it "echo('hello') 应该得到 'hello'" do 
    @echo.echo('hello') == 'hello'
  end 
end



