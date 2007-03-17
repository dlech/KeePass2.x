RMDIR /S /Q KeePass
RMDIR /S /Q KeePass_Distrib
RMDIR /S /Q KeePassLib
RMDIR /S /Q KeePassLibDoc
RMDIR /S /Q KeePassLibSD
RMDIR /S /Q KeePassNtv
RMDIR /S /Q ShInstUtil

RMDIR /S /Q ..\Ext\Output

RMDIR /S /Q ..\KeePass\obj
DEL ..\KeePass\KeePass.csproj.user

RMDIR /S /Q ..\KeePassLib\obj
DEL ..\KeePassLib\KeePassLib.csproj.user

RMDIR /S /Q ..\KeePassLibSD\obj
DEL ..\KeePassLibSD\KeePassLibSD.csproj.user

RMDIR /S /Q ..\ShInstUtil\obj
DEL ..\ShInstUtil\ShInstUtil.csproj.user

DEL /A:H ..\KeePass.suo
DEL ..\KeePass.ncb

DEL ..\KeePassNtv\KeePassNtv.vcproj.REDDYX.Dominik.user

RMDIR /S /Q ..\Plugins\ArcFourCipher\bin
RMDIR /S /Q ..\Plugins\ArcFourCipher\obj
DEL ..\Plugins\ArcFourCipher\ArcFourCipher.csproj.user

RMDIR /S /Q ..\Plugins\KPScript\bin
RMDIR /S /Q ..\Plugins\KPScript\obj
DEL ..\Plugins\KPScript\KPScript.csproj.user

RMDIR /S /Q ..\Plugins\SamplePlugin\bin
RMDIR /S /Q ..\Plugins\SamplePlugin\obj
DEL ..\Plugins\SamplePlugin\SamplePlugin.csproj.user

CLS