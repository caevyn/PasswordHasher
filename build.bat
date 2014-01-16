@echo off

cls

"src\.nuget\nuget.exe" "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
"src\.nuget\nuget.exe" "install" "NUnit.Runners" "-OutputDirectory" "tools" "-ExcludeVersion"
"tools\FAKE\tools\Fake.exe" build.fsx

pause