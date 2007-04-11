MKDIR KeePass_Distrib
DEL /F /S /Q KeePass_Distrib\*.*

COPY /B KeePass\Release\KeePass.exe /B KeePass_Distrib\KeePass.exe

COPY /B KeePassNtv\Release\KeePassNtv32.dll /B KeePass_Distrib\KeePassNtv32.dll
COPY /B "KeePassNtv\Release x64\KeePassNtv64.dll" /B KeePass_Distrib\KeePassNtv64.dll

COPY /B ..\Ext\KeePass.config.xml /B KeePass_Distrib\KeePass.config.xml

COPY /B ..\Docs\License.txt /B KeePass_Distrib\License.txt

COPY /B ..\..\KeePassClassic\Build\KeePassLibC\Release\KeePassLibC32.dll /B KeePass_Distrib\KeePassLibC32.dll
COPY /B "..\..\KeePassClassic\Build\KeePassLibC\Release x64\KeePassLibC64.dll" /B KeePass_Distrib\KeePassLibC64.dll

COPY /B ShInstUtil\Release\ShInstUtil.exe /B KeePass_Distrib\ShInstUtil.exe

COPY /B ..\..\..\Homepage_KeePass\Build_Chm_v2\KeePass.chm /B KeePass_Distrib\KeePass.chm

CD KeePass_Distrib
MKDIR XSL
CD ..
XCOPY ..\Ext\XSL\*.* KeePass_Distrib\XSL\*.* /S /E /V /O /K /H

MKDIR KeePassLib_Distrib
DEL /F /S /Q KeePassLib_Distrib\*.*

COPY /B KeePassLib\Release\KeePassLib.dll /B KeePassLib_Distrib\KeePassLib.dll
COPY /B KeePassLib\Release\KeePassLib.xml /B KeePassLib_Distrib\KeePassLib.xml

MKDIR KeePassLibSD_Distrib
DEL /F /S /Q KeePassLibSD_Distrib\*.*

COPY /B KeePassLibSD\Release\KeePassLibSD.dll /B KeePassLibSD_Distrib\KeePassLibSD.dll

PAUSE
CLS