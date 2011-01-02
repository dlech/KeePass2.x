MKDIR KeePass_Distrib
DEL /F /S /Q KeePass_Distrib\*.*

COPY /B KeePass\Debug\KeePass.exe /B KeePass_Distrib\KeePass.exe
COPY /B KeePass\Debug\KeePass.XmlSerializers.dll /B KeePass_Distrib\KeePass.XmlSerializers.dll

REM COPY /B KeePassNtv\Debug\KeePassNtv32.dll /B KeePass_Distrib\KeePassNtv32.dll
REM COPY /B "KeePassNtv\Debug x64\KeePassNtv64.dll" /B KeePass_Distrib\KeePassNtv64.dll

COPY /B ..\Ext\KeePass.config.xml /B KeePass_Distrib\KeePass.config.xml
COPY /B ..\Ext\KeePass.exe.config /B KeePass_Distrib\KeePass.exe.config

COPY /B ..\Docs\License.txt /B KeePass_Distrib\License.txt

COPY /B ..\..\KeePassClassic\Build\KeePassLibC\Debug\KeePassLibC32.dll /B KeePass_Distrib\KeePassLibC32.dll
COPY /B "..\..\KeePassClassic\Build\KeePassLibC\Debug x64\KeePassLibC64.dll" /B KeePass_Distrib\KeePassLibC64.dll

COPY /B ShInstUtil\Debug\ShInstUtil.exe /B KeePass_Distrib\ShInstUtil.exe

COPY /B ..\..\..\Homepage_KeePass\Build_Chm_v2\KeePass.chm /B KeePass_Distrib\KeePass.chm

CD KeePass_Distrib
MKDIR XSL
CD ..
XCOPY ..\Ext\XSL\*.* KeePass_Distrib\XSL\ /V /K /H

MKDIR KeePassLib_Distrib
DEL /F /S /Q KeePassLib_Distrib\*.*

COPY /B KeePassLib\Debug\KeePassLib.dll /B KeePassLib_Distrib\KeePassLib.dll
COPY /B KeePassLib\Debug\KeePassLib.xml /B KeePassLib_Distrib\KeePassLib.xml

MKDIR KeePassLibSD_Distrib
DEL /F /S /Q KeePassLibSD_Distrib\*.*

COPY /B KeePassLibSD\Debug\KeePassLibSD.dll /B KeePassLibSD_Distrib\KeePassLibSD.dll

CLS