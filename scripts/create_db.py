#encoding: utf-8

import pyjsonrpc
import hashlib

ss_client = pyjsonrpc.HttpClient(url = "http://localhost:9287/jsonrpc")

dbs = 'Current databases:', ss_client.listDatabases()

root_password_hash = hashlib.sha1(u'root').hexdigest()

if 'slipstream_db1' in dbs:
    ss_client.deleteDatabase(root_password_hash, 'slipstream_db1')

ss_client.createDatabase(root_password_hash, "slipstream_db1", "admin")

