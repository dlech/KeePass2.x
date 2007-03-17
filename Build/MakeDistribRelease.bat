MKDIR KeePass_Distrib
DEL /F /S /Q KeePass_Distrib\*.*

COPY /B KeePass\Release\KeePass.exe /B KeePass_Distrib\KeePass.exe

COPY /B KeePassNtv\Release\KeePassNtv.dll /B KeePass_Distrib\KeePassNtv.dll

COPY /B ..\Ext\KeePass.config.xml /B KeePass_Distrib\KeePass.config.xml

COPY /B ..\Docs\License.txt /B KeePass_Distrib\License.txt

COPY /B ..\..\KeePassClassic\Build\KeePassLibC\Release\KeePassLibC.dll /B KeePass_Distrib\KeePassLibC.dll

COPY /B ShInstUtil\Release\ShInstUtil.exe /B KeePass_Distrib\ShInstUtil.exe

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