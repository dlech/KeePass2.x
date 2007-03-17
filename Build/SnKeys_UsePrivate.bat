CD ..

COPY /Y /B Private_NoDistrib\KeePass.pfx /B KeePass\KeePass.pfx
COPY /Y /B Private_NoDistrib\KeePassLib.pfx /B KeePassLib\KeePassLib.pfx
COPY /Y /B Private_NoDistrib\KeePassLibSD.pfx /B KeePassLibSD\KeePassLibSD.pfx

COPY /Y /B Private_NoDistrib\ShInstUtil.pfx /B ShInstUtil\ShInstUtil.pfx

COPY /Y /B Private_NoDistrib\ArcFourCipher.pfx /B Plugins\ArcFourCipher\ArcFourCipher.pfx
COPY /Y /B Private_NoDistrib\KPScript.pfx /B Plugins\KPScript\KPScript.pfx
COPY /Y /B Private_NoDistrib\SamplePlugin.pfx /B Plugins\SamplePlugin\SamplePlugin.pfx

CD Build

PAUSE
CLS