rm Output\SpeedLaunch-1.0.0.0.exe

mkdir Build

copy SpeedLaunch\bin\Release\NCalc.dll Build
copy SpeedLaunch\bin\Release\SpeedLaunch.exe Build
copy SpeedLaunch.ico Build

call subscribe "Build\NCalc.dll"
call subscribe "Build\SpeedLaunch.exe"


iscc /q create-installation-package.iss

call subscribe "Output\SpeedLaunch-1.0.0.0.exe"

sha256sum "Output\SpeedLaunch-1.0.0.0.exe" > "Output\signature.txt"

pause
