from jsonrpc import ServiceProxy
s = ServiceProxy('http://localhost:9287/ObjectServer.ashx')

print s.system.echo("echo")
print "Methods:"
print s.system.listMethods()

print "Version: " + s.GetVersion()

session_id = s.LogOn('objectserver', 'root', 'root')

domain = [("name","like","core%"),]
ids = s.Execute(session_id, 'core.model', 'Search', [domain, 0, 100]);
print "IDs:", ids

fields = ['name', 'label', 'info']

models = s.Execute(session_id, 'core.model', 'Read', [ids, fields]);
print 'Models:'
for m in models:
    print m

s.LogOff(session_id)

