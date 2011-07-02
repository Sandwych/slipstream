@ECHO OFF
IF NOT "%~f0" == "~f0" GOTO :WinNT
@"ir.exe" "D:/dev/dotnet/object-server/lib/ironruby/bin/prettify_json.rb" %1 %2 %3 %4 %5 %6 %7 %8 %9
GOTO :EOF
:WinNT
@"ir.exe" "%~dpn0" %*
