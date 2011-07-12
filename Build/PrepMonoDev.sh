cp -f ../Ext/Icons/Finals2/plockb.ico ../KeePass/KeePass.ico
cp -f ../Ext/Icons/Finals2/plockb.ico ../KeePass/Resources/Images/KeePass.ico

sed 's!<SignAssembly>true</SignAssembly>!<SignAssembly>false</SignAssembly>!g' ../KeePass/KeePass.csproj > ../KeePass/KeePass.csproj.new
mv -f ../KeePass/KeePass.csproj.new ../KeePass/KeePass.csproj

sed 's!<SignAssembly>true</SignAssembly>!<SignAssembly>false</SignAssembly>!g' ../KeePassLib/KeePassLib.csproj > ../KeePassLib/KeePassLib.csproj.new
mv -f ../KeePassLib/KeePassLib.csproj.new ../KeePassLib/KeePassLib.csproj

cat ../KeePass.sln | grep -v 'DC15F71A-2117-4DEF-8C10-AA355B5E5979' | uniq > ../KeePass.sln.new
mv -f ../KeePass.sln.new ../KeePass.sln
