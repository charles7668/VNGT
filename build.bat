dotnet publish GameManager -f net8.0-windows10.0.19041.0 -o "./bin/GameManager x86" -c Release -r win10-x86 -p:WindowsPackageType=None
dotnet publish SavePatcher -c Release -o "./bin/GameManager x86/tools/SavePatcher" -r win-x86
dotnet publish GameManager -f net8.0-windows10.0.19041.0 -o "./bin/GameManager" -c Release -r win10-x64 -p:WindowsPackageType=None
dotnet publish SavePatcher -c Release -o "./bin/GameManager/tools/SavePatcher" -r win-x64