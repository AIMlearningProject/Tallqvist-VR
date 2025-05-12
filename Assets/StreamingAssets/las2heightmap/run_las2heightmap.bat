@echo off
REM Set PROJ_LIB to the share/proj directory next to the EXE
set "PROJ_LIB=%~dp0share\proj"

REM Launch the actual executable with all passed arguments
"%~dp0las2heightmap.exe" %*