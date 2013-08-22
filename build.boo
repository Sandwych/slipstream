
configuration = "release"

target default, (init, compile, test, deploy, package):
    pass

target ci, (init, compile, deploy, package):
    pass

target init:
    rmdir("build")

target clean:
    solution_file = "src/Slipstream.sln"
    msbuild(file: solution_file, configuration: "debug", targets: ("clean",))
    msbuild(file: solution_file, configuration: "release", targets: ("clean",))
    rmdir("build")

desc "Compiles the solution"
target compile:
    solution_file = "src/Slipstream.sln"
    msbuild(file: solution_file, configuration: configuration)

desc "Executes tests"
target test:
    nunit(assembly: "src/Malt.Common.Test/bin/${configuration}/Malt.Common.Test.dll")
    nunit(assembly: "src/SlipStream.Test/bin/${configuration}/SlipStream.Test.dll")
    nunit(assembly: "src/SlipStream.Client.Test/bin/${configuration}/SlipStream.Client.Test.dll")

desc "Copies the binaries to the 'build' directory"
target deploy, (compile):
#print "Removing 'build' directory"
#    rmdir('build')

    print "Copying to build dir"
    print "Copying Slipstream Server..."
    with FileList("src/SlipStream.DevServer/bin"):
        .Include("*.{dll,exe}")
        .ForEach def(file):
            file.CopyToDirectory("build/s2/bin")

    print "Copying ZeroMQ's DLL..."
    with FileList("lib/zmq"):
        .Include("libzmq.dll")
        .ForEach def(file):
            file.CopyToDirectory("build/s2/bin")

    print "Copying internal modules..."
    with FileList("src/SlipStream.DevServer/Modules/"):
        .Include("**")
        .ForEach def(file):
            file.CopyToDirectory("build/s2/Modules")
      
desc "Creates zip package"
target package, (deploy):
  zip("build/s2", 'build/s2-platform.zip')

