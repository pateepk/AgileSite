:: Creates junctions in Output subfolder using Update-OutputJunctions.ps1
:: Params
::  Project directory
::  Build configuration (Debug/Release)
::  Optional explicite junction point target used to differentiate tests solutions.
 
SET _SolutionDirectory=%~1
SET _Configuration=%~2
SET _JunctionName=%~3

powershell -NoLogo -NonInteractive -ExecutionPolicy Unrestricted -Command "& \"%_SolutionDirectory%Scripts\Update-OutputJunctions.ps1\" \"%_SolutionDirectory%\\" %_Configuration% %_JunctionName%"