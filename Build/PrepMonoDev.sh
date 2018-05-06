#!/bin/sh

kpBuild="$(pwd)"
kpRoot="${kpBuild}/.."

# Mono's resource compiler/linker doesn't support ICO files
# containing high resolution images (in PNG format)
kpIco="${kpRoot}/Ext/Icons_15_VA/LowResIcons/KeePass_LR.ico"
kpIcoG="${kpRoot}/Ext/Icons_15_VA/LowResIcons/KeePass_LR_G.ico"
kpIcoR="${kpRoot}/Ext/Icons_15_VA/LowResIcons/KeePass_LR_R.ico"
kpIcoY="${kpRoot}/Ext/Icons_15_VA/LowResIcons/KeePass_LR_Y.ico"

fnPrepSolution()
{
	cd "${kpRoot}"
	local kpSln="KeePass.sln"

	# Update solution format to 11 (this targets Mono 4 rather than 3.5)
	sed -i 's!Format Version 10\.00!Format Version 11\.00!g' "${kpSln}"
}

fnPrepKeePass()
{
	cd "${kpRoot}/KeePass"
	local kpCsProj="KeePass.csproj"

	sed -i 's! ToolsVersion="3\.5"!!g' "${kpCsProj}"
	sed -i 's!<SignAssembly>true</SignAssembly>!<SignAssembly>false</SignAssembly>!g' "${kpCsProj}"
	sed -i '/sgen\.exe/d' "${kpCsProj}"

	cp -f "${kpIco}" KeePass.ico
	cp -f "${kpIco}" Resources/Icons/KeePass.ico
	cp -f "${kpIcoG}" Resources/Icons/KeePass_G.ico
	cp -f "${kpIcoR}" Resources/Icons/KeePass_R.ico
	cp -f "${kpIcoY}" Resources/Icons/KeePass_Y.ico
}

fnPrepKeePassLib()
{
	cd "${kpRoot}/KeePassLib"
	local kpCsProj="KeePassLib.csproj"
	local kpXmlUtilEx="Utility/XmlUtilEx.cs"

	sed -i 's! ToolsVersion="3\.5"!!g' "${kpCsProj}"
	sed -i 's!<SignAssembly>true</SignAssembly>!<SignAssembly>false</SignAssembly>!g' "${kpCsProj}"

	sed -i -E 's!(xrs\.ProhibitDtd = true;)!// \1!g' "${kpXmlUtilEx}"
	sed -i -E 's!// (xrs\.DtdProcessing = DtdProcessing\.Prohibit;)!\1!g' "${kpXmlUtilEx}"
}

fnPrepTrlUtil()
{
	cd "${kpRoot}/Translation/TrlUtil"
	local kpCsProj="TrlUtil.csproj"

	sed -i 's! ToolsVersion="3\.5"!!g' "${kpCsProj}"

	cp -f "${kpIco}" Resources/KeePass.ico
}

fnPrepSolution
fnPrepKeePass
fnPrepKeePassLib
fnPrepTrlUtil

cd "${kpBuild}"
