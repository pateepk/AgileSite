:: Creates junction between bin folder and the Output CMS folder
:: Params
::  Project directory
::  Build configuration (Debug/Release)

SET _ProjectDirectory=%~1
SET _Configuration=%~2

IF EXIST "%_ProjectDirectory%\bin" (rmdir /Q /S "%_ProjectDirectory%\bin")
mklink /J "%_ProjectDirectory%\bin" "%_ProjectDirectory%\..\Output\%_Configuration%\CMS"