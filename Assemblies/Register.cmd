@echo off
rem ******************************************************
rem *                                                    *
rem * This batch registers all .NET libraries in the     *
rem * current folder and all its subfolders.             *
rem *                                                    *
rem * It also executes any registry scripts found.       *
rem *                                                    *
rem ******************************************************

rem Register assemblies
for /r %%f in (*.dll) do @gacutil /nologo /f /i %%f

rem Patching registry
for /r %%f in (*.reg) do @regedit /s %%f
