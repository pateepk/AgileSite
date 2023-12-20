REM Create bin folder junction"
REM * This Junction must exists for build to succeed *
REM git/vs does not recognize Junction as a Junction 
REM This needs to be run in administrator mode.

REM delete bad bin folder if exists
rmdir bin 
rmdir bin2
REM make the link
mklink /j bin ..\Output\Release\CMS
