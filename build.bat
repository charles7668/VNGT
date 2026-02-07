@echo off
setlocal

dotnet publish GameManager -f net10.0-windows10.0.19041.0 -o "./bin/GameManager x86" -c Release -r win-x86 -p:WindowsPackageType=None
dotnet publish SavePatcher -c Release -o "./bin/SavePatcher x86" -r win-x86
dotnet publish GameManager -f net10.0-windows10.0.19041.0 -o "./bin/GameManager" -c Release -r win-x64 -p:WindowsPackageType=None
dotnet publish SavePatcher -c Release -o "./bin/SavePatcher x64" -r win-x64

echo start build ProcessTracer...

:: Search vsinstalldir using vswhere
for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do (
    set "VSINSTALLDIR=%%i"
)

echo Visual Studio installation path: %VSINSTALLDIR%

set "VCVARS=%VSINSTALLDIR%\VC\Auxiliary\Build\vcvars64.bat"
echo Full vcvars64.bat path: %VCVARS%
call "%VCVARS%"

cd ProcessTracer
dotnet restore
call build.bat
cd ..

echo Build ProcessTracer finished.

echo Start copy files to output path...
xcopy "ProcessTracer\Release\*" "bin\GameManager x86\ProcessTracer\" /E /I /Y
xcopy "ProcessTracer\Release\*" "bin\GameManager\ProcessTracer\" /E /I /Y
echo Copy files finished.

endlocal