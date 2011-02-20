from jsonrpc import ServiceProxy
s = ServiceProxy('http://localhost:9287/ObjectServer.ashx')

print "Methods:"
print s.system.listMethods()

ids = s.Execute('objectserver', 'core.model', 'Search', ['', 0, 100]);
print "IDs:", ids

fields = ['name', 'label', 'info']
models = s.Execute('objectserver', 'core.model', 'Read', [ids, fields]);

print 'Models:'
for m in models:
    print m
