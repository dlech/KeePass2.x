DEL ..\KeePass.ncb
DEL /A:H ..\KeePass.suo
RMDIR /S /Q ..\.vs

RMDIR /S /Q KeePass
RMDIR /S /Q KeePass_Distrib
RMDIR /S /Q KeePassLib
RMDIR /S /Q KeePassMsi
RMDIR /S /Q ShInstUtil

DEL /A:H ..\Ext\KeePassMsi\KeePassMsi.suo
RMDIR /S /Q ..\Ext\KeePassMsi\.vs

DEL ..\KeePass\KeePass.csproj.user
RMDIR /S /Q ..\KeePass\obj

DEL ..\KeePassLib\KeePassLib.csproj.user
RMDIR /S /Q ..\KeePassLib\obj

DEL ..\ShInstUtil\ShInstUtil.aps
DEL ..\ShInstUtil\ShInstUtil.ncb
DEL /A:H ..\ShInstUtil\ShInstUtil.suo
DEL /Q ..\ShInstUtil\*.user

DEL ..\Translation\KeePass.config.xml
DEL ..\Translation\KeePass.exe
DEL ..\Translation\KeePass.exe.config
DEL ..\Translation\KeePass.pdb
DEL ..\Translation\KeePass.XmlSerializers.dll
DEL ..\Translation\TrlUtil.exe
DEL ..\Translation\TrlUtil.exe.config
DEL ..\Translation\TrlUtil.pdb
DEL ..\Translation\TrlUtil.vshost.exe
DEL ..\Translation\TrlUtil.vshost.exe.manifest

DEL ..\Translation\TrlUtil\TrlUtil.csproj.user
RMDIR /S /Q ..\Translation\TrlUtil\obj

CLS