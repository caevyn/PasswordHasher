@echo off

cls

set VERSION=%1
"src\.nuget\nuget.exe" "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
"src\.nuget\nuget.exe" "install" "NUnit.Runners" "-OutputDirectory" "tools" "-ExcludeVersion"
"tools\FAKE\tools\Fake.exe" build.fsx VERSION=%VERSION%

pause