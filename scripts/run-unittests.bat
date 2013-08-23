@ECHO OFF
@"%~dp0..\lib\nunit\nunit-console.exe" "%~dp0..\src\SlipStream.nunit" "/xml:%~dp0..\build\TestResult.xml" "/nologo" "/noshadow"
