:: Download and unzip BepInEx 6.*
if exist "BepInEx\core\BepInEx.Core.dll" (
	rem BepInEx already installed
) else (
powershell -Command "Invoke-WebRequest https://github.com/BepInEx/BepInEx/releases/download/v6.0.0-pre.1/BepInEx_UnityIL2CPP_x64_6.0.0-pre.1.zip -OutFile temp_download.zip"
powershell Expand-Archive temp_download.zip -DestinationPath ./
del temp_download.zip
)
:: Get basic After The Fall modding tool
powershell -Command "Invoke-WebRequest https://github.com/floh-bhaptics/AfterTheFall_modding/releases/latest/download/AfterTheFall_modding.exe -OutFile AfterTheFall_modding.exe"

:: If plugins folder doesn't exist, create it
if not exist "BepInEx\plugins" mkdir BepInEx\plugins

:: Get bhaptics mod and dependencies
powershell -Command "Invoke-WebRequest https://github.com/floh-bhaptics/AfterTheFall_bhaptics/releases/latest/download/AfterTheFall_bhaptics.dll -OutFile BepInEx\plugins\AfterTheFall_bhaptics.dll"
powershell -Command "Invoke-WebRequest https://github.com/floh-bhaptics/AfterTheFall_bhaptics/releases/latest/download/Bhaptics.Tact.dll -OutFile BepInEx\plugins\Bhaptics.Tact.dll"
powershell -Command "Invoke-WebRequest https://github.com/floh-bhaptics/AfterTheFall_bhaptics/releases/latest/download/bhaptics_library.dll -OutFile BepInEx\plugins\bhaptics_library.dll"
