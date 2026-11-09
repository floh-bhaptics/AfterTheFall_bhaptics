:: Download and unzip BepInEx 6.*
if exist "BepInEx\core\BepInEx.Core.dll" (
	rem BepInEx already installed
) else (
powershell -Command "Invoke-WebRequest https://github.com/BepInEx/BepInEx/releases/download/v6.0.0-pre.2/BepInEx-Unity.IL2CPP-win-x64-6.0.0-pre.2.zip -OutFile temp_download.zip"
powershell Expand-Archive temp_download.zip -DestinationPath ./
del temp_download.zip
)

:: If plugins folder doesn't exist, create it
if not exist "BepInEx\plugins" mkdir BepInEx\plugins

:: Get bhaptics mod and dependencies
powershell -Command "Invoke-WebRequest https://github.com/floh-bhaptics/AfterTheFall_bhaptics/releases/latest/download/AfterTheFall_bhaptics.dll -OutFile BepInEx\plugins\AfterTheFall_bhaptics.dll"
powershell -Command "Invoke-WebRequest https://github.com/floh-bhaptics/AfterTheFall_bhaptics/releases/latest/download/bhaptics_library.dll -OutFile BepInEx\plugins\bhaptics_library.dll"
