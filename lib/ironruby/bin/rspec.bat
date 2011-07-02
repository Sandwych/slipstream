@ECHO OFF
IF NOT "%~f0" == "~f0" GOTO :WinNT
@"D:\dev\dotnet\object-server\lib\ironruby\bin\ir.exe" "D:/dev/dotnet/object-server/lib/ironruby/bin/rspec" %1 %2 %3 %4 %5 %6 %7 %8 %9
GOTO :EOF
:WinNT
@"D:\dev\dotnet\object-server\lib\ironruby\bin\ir.exe" "%~dpn0" %*
