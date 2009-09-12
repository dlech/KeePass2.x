CD ..

COPY /Y /B Ext\KPDummyKey.pfx /B KeePass\KeePass.pfx
COPY /Y /B Ext\KPDummyKey.pfx /B KeePassLib\KeePassLib.pfx
COPY /Y /B Ext\KPDummyKey.pfx /B KeePassLibSD\KeePassLibSD.pfx

REM COPY /Y /B Ext\KPDummyKey.pfx /B ShInstUtil\ShInstUtil.pfx

COPY /Y /B Ext\KPDummyKey.pfx /B Plugins\ArcFourCipher\ArcFourCipher.pfx
COPY /Y /B Ext\KPDummyKey.pfx /B Plugins\KPScript\KPScript.pfx
COPY /Y /B Ext\KPDummyKey.pfx /B Plugins\SamplePlugin\SamplePlugin.pfx

CD Build

PAUSE
CLS