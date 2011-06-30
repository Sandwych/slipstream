import json
import urllib2

class ServiceProxy(object):
    def __init__(self, serviceURL, serviceName=None):
        self.__serviceURL = serviceURL
        self.__serviceName = serviceName
        self.__id_count = 0

    def __getattr__(self, name):
        if self.__serviceName != None:
            name = '%s.%s' % (self.__serviceName, name)
        return ServiceProxy(self.__serviceURL, name)

    def __call__(self, *args):
        self.__id_count += 1
        jobj = { 'method': self.__serviceName, 'params': args, 'id':str(self.__id_count) }
        postdata = json.dumps(jobj)
        print postdata
        respdata = urllib2.urlopen(self.__serviceURL, postdata).read()
        print respdata
        resp = json.loads(respdata)
        if resp['error'] != None:
            raise Exception(resp['error'])
        #raise JSONRPCException(resp['error'])
        else:
            return resp['result']


s = ServiceProxy('http://localhost:9287/crossdomain.xml')

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

