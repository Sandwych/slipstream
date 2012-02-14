
configuration = "release"

target default, (init, compile, test, deploy, package):
    pass

target ci, (init, compile, deploy, package):
    pass

target init:
    rmdir("build")

target clean:
    rmdir("build")

desc "Compiles the solution"
target compile:
    solution_file = "src/Slipstream.sln"
    msbuild(file: solution_file, configuration: configuration)

desc "Executes tests"
target test:
    nunit(assembly: "src/Malt.Common.Test/bin/${configuration}/Malt.Common.Test.dll")
    nunit(assembly: "src/ObjectServer.Test/bin/${configuration}/ObjectServer.Test.dll")
    nunit(assembly: "src/ObjectServer.Client.Test/bin/${configuration}/ObjectServer.Client.Test.dll")

desc "Copies the binaries to the 'build' directory"
target deploy:
    print "Copying to build dir"

    print "Copying Slipstream Server"
    with FileList("src/ObjectServer.DevServer/bin/${configuration}"):
        .Include("*.{dll,exe}")
        .ForEach def(file):
            file.CopyToDirectory("build/${configuration}/DevServer")

    print "Copying ZeroMQ's DLL"
    with FileList("lib/zmq"):
        .Include("libzmq.dll")
        .ForEach def(file):
            file.CopyToDirectory("build/${configuration}/DevServer")

      
desc "Creates zip package"
target package:
  zip("build/${configuration}", 'build/Slipstream-Server.zip')

