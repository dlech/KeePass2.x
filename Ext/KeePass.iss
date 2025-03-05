; KeePass Password Safe Installation Script
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!
; Thanks to Hilbrand Edskes for installer improvements.

#define MyAppNameShort "KeePass"
#define MyAppNameShortEx "KeePass 2"
#define MyAppName "KeePass Password Safe"
#define MyAppNameEx "KeePass Password Safe 2"
#define MyAppPublisher "Dominik Reichl"
#define MyAppURL "https://keepass.info/"
#define MyAppExeName "KeePass.exe"
#define MyAppUrlName "KeePass.url"
#define MyAppHelpName "KeePass.chm"
#define MyAppId "KeePassPasswordSafe2"

#define KeeVersionStr "2.58"
#define KeeVersionStrWithMinor "2.58"
#define KeeVersionStrWithMinorPath "2.58"
#define KeeVersionWin "2.58.0.0"
#define KeeVersionWinShort "2.58"

#define KeeDevPeriod "2003-2025"

[Setup]
AppName={#MyAppName}
AppVersion={#KeeVersionWinShort}
AppVerName={#MyAppName} {#KeeVersionStrWithMinor}
AppId={#MyAppId}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppCopyright=Copyright © {#KeeDevPeriod} {#MyAppPublisher}
MinVersion=6.1sp1

; When specifying ArchitecturesInstallIn64BitMode=x64, Inno Setup performs
; a side-by-side installation instead of updating a previous installation
; (32-bit installation on a 64-bit system).
; In order to avoid this, we only change DefaultDirName.
; ArchitecturesInstallIn64BitMode=x64
DefaultDirName={code:MyGetProgramFiles}\{#MyAppNameEx}

DefaultGroupName={#MyAppNameEx}
AllowNoIcons=yes
LicenseFile=..\Docs\License.txt
OutputDir=..\Build\KeePass_Distrib
OutputBaseFilename={#MyAppNameShort}-{#KeeVersionStrWithMinorPath}-Setup
Compression=lzma2/ultra
SolidCompression=yes
InternalCompressLevel=ultra
UninstallDisplayIcon={app}\{#MyAppExeName}
AppMutex=KeePassAppMutex,Global\KeePassAppMutexEx
SetupMutex=KeePassSetupMutex2
ChangesAssociations=yes
VersionInfoVersion={#KeeVersionWin}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} {#KeeVersionStr} Setup
VersionInfoCopyright=Copyright © {#KeeDevPeriod} {#MyAppPublisher}
SetupIconFile=compiler:SetupClassicIcon.ico
; WizardImageFile=compiler:WizClassicImage-IS.bmp
; WizardSmallImageFile=compiler:WizClassicSmallImage-IS.bmp
WizardStyle=classic
DisableDirPage=auto
AlwaysShowDirOnReadyPage=yes
DisableProgramGroupPage=yes
AlwaysShowGroupOnReadyPage=no

[Languages]
Name: en; MessagesFile: "compiler:Default.isl"
Name: ca; MessagesFile: "compiler:Languages\Catalan.isl"
Name: cs; MessagesFile: "compiler:Languages\Czech.isl"
Name: da; MessagesFile: "compiler:Languages\Danish.isl"
Name: de; MessagesFile: "compiler:Languages\German.isl"
Name: es; MessagesFile: "compiler:Languages\Spanish.isl"
Name: fi; MessagesFile: "compiler:Languages\Finnish.isl"
Name: fr; MessagesFile: "compiler:Languages\French.isl"
; Name: hu; MessagesFile: "compiler:Languages\Hungarian.isl"
Name: it; MessagesFile: "compiler:Languages\Italian.isl"
Name: ja; MessagesFile: "compiler:Languages\Japanese.isl"
Name: nb; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: nl; MessagesFile: "compiler:Languages\Dutch.isl"
Name: pl; MessagesFile: "compiler:Languages\Polish.isl"
Name: ptBR; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: ptPT; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: ru; MessagesFile: "compiler:Languages\Russian.isl"
; Name: sk; MessagesFile: "compiler:Languages\Slovak.isl"
Name: sl; MessagesFile: "compiler:Languages\Slovenian.isl"

[CustomMessages]
MyCompCore=KeePass core files
MyCompHelp=User manual
MyCompNtvLib=Native support library
MyCompXSL=XSL stylesheets for KDBX XML files
; "KeePass performance optimization" could falsely be interpreted as
; operating system performance optimization carried out by KeePass
MyCompNGen=Optimize KeePass performance
MyCompPreLoad=Optimize KeePass start-up performance
MyStatusNGen=Optimizing KeePass performance...
MyStatusPreLoad=Optimizing KeePass start-up performance...
MyOptPlgPage=Open the plugins web page

ca.MyCompCore=Fitxers del nucli del KeePass
ca.MyCompHelp=Manual d'usuari
ca.MyCompNtvLib=Biblioteca de suport nativa
ca.MyCompXSL=Fulls d'estil XSL pels fitxers XML del KeePass
ca.MyCompNGen=Optimitza el rendiment del KeePass
ca.MyCompPreLoad=Optimitza l'inici del KeePass
ca.MyStatusNGen=Optimitzant el rendiment del KeePass...
ca.MyStatusPreLoad=Optimitzant l'inici del KeePass...
ca.MyOptPlgPage=Obre el web dels connectors

da.MyCompCore=KeePass kernefiler
da.MyCompHelp=Brugermanual
da.MyCompNtvLib=Oprindeligt understøttelsesbibliotek
da.MyCompXSL=XSL-stylesheets til KDBX XML-filer
da.MyCompNGen=Optimer KeePass ydeevne
da.MyCompPreLoad=Optimer KeePass opstarts ydeevne
da.MyStatusNGen=Optimerer KeePass ydeevne...
da.MyStatusPreLoad=Optimerer KeePass opstarts ydeevne...
da.MyOptPlgPage=Åbn websiden med plugins

de.MyCompCore=KeePass-Hauptdateien
de.MyCompHelp=Benutzerhandbuch
de.MyCompNtvLib=Native Unterstützungsbibliothek
de.MyCompXSL=XSL-Stylesheets für KDBX-XML-Dateien
de.MyCompNGen=KeePass-Leistung optimieren
de.MyCompPreLoad=KeePass-Start-Leistung optimieren
de.MyStatusNGen=Optimiere KeePass-Leistung...
de.MyStatusPreLoad=Optimiere KeePass-Start-Leistung...
de.MyOptPlgPage=Die Plugins-Webseite öffnen

es.MyCompCore=Archivos de instalación de KeePass
es.MyCompHelp=Manual del usuario
es.MyCompNtvLib=Biblioteca para soporte nativo
es.MyCompXSL=Hojas de estilo XSL para los archivos XML de KDBX
es.MyCompNGen=Optimizar el rendimiento de KeePass
es.MyCompPreLoad=Optimizar el rendimiento de inicio de KeePass
es.MyStatusNGen=Optimizando el rendimiento de KeePass...
es.MyStatusPreLoad=Optimizando el rendimiento de inicio de KeePass...
es.MyOptPlgPage=Abrir la página web de los complementos

fi.MyCompCore=KeePassin ydintiedostot
fi.MyCompHelp=Käyttäjän opas
fi.MyCompNtvLib=Natiivitukikirjasto
fi.MyCompXSL=XSL-tyylarkit KDBX XML-tiedostoja varten
fi.MyCompNGen=KeePassin suorituksen optimointi
fi.MyCompPreLoad=KeePassin käynnistyksen optimointi
fi.MyStatusNGen=Optimoidaan KeePassin suoritusta...
fi.MyStatusPreLoad=Optimoidaan KeePassin käynnistystä...
fi.MyOptPlgPage=Avaa liitännäiset ja laajennukset sisältävä sivusto

fr.MyCompCore=Fichiers de base KeePass
fr.MyCompHelp=Manuel de l'utilisateur
fr.MyCompNtvLib=Bibliothèque de support native
fr.MyCompXSL=Feuilles de style XSL pour les fichiers KDBX XML
fr.MyCompNGen=Optimiser les performances de KeePass
fr.MyCompPreLoad=Optimiser les performances de démarrage de KeePass
fr.MyStatusNGen=En cours d'optimisation des performances de KeePass...
fr.MyStatusPreLoad=En cours d'optimisation des performances de démarrage de KeePass...
fr.MyOptPlgPage=Ouvre la page des greffons (plug-ins) sur la toile

; hu.MyCompCore=KeePass nélkülözhetetlen fájlok
; hu.MyCompHelp=Használati utasítás
; hu.MyCompNtvLib=Natív támogatási könyvtár
; hu.MyCompXSL=XSL stíluslapok a KDBX XML fájlokhoz
; hu.MyCompNGen=Optimalizálja a KeePass teljesítményét
; hu.MyCompPreLoad=Optimalizálja a KeePass indítási teljesítményét
; hu.MyStatusNGen=A KeePass teljesítményének optimalizálása...
; hu.MyStatusPreLoad=A KeePass indítási teljesítményének optimalizálása...
; hu.MyOptPlgPage=Nyissa meg a bővítmények weboldalát

it.MyCompCore=File core di KeePass
it.MyCompHelp=Manuale utente
it.MyCompNtvLib=Libreria di supporto nativa
it.MyCompXSL=Fogli di stile XSL per i file KDBX XML
it.MyCompNGen=Ottimizza le prestazioni di KeePass
it.MyCompPreLoad=Ottimizza le prestazioni di avvio di KeePass
it.MyStatusNGen=Ottimizzazione delle prestazioni di KeePass...
it.MyStatusPreLoad=Ottimizzazione delle prestazioni di avvio di KeePass...
it.MyOptPlgPage=Apri la pagina web dei plug-in

ja.MyCompCore=KeePassのコアファイル
ja.MyCompHelp=ユーザーマニュアル
ja.MyCompNtvLib=ネイティブサポートライブラリ
ja.MyCompXSL=KDBXのXMLファイル用XSLスタイルシート
ja.MyCompNGen=KeePassのパフォーマンスを最適化
ja.MyCompPreLoad=KeePassの起動パフォーマンスを最適化
ja.MyStatusNGen=KeePassのパフォーマンスを最適化中...
ja.MyStatusPreLoad=KeePassの起動パフォーマンスを最適化中...
ja.MyOptPlgPage=プラグインのWebページを開きます。

nl.MyCompCore=KeePass basisbestanden
nl.MyCompHelp=Handleiding
nl.MyCompNtvLib=Standaard bestandsondersteuning
nl.MyCompXSL=XSL stylesheets voor KDBX XML bestanden
nl.MyCompNGen=Optimaliseer KeePass prestaties
nl.MyCompPreLoad=Optimaliseer KeePass opstartprestaties
nl.MyStatusNGen=KeePass prestaties optimaliseren...
nl.MyStatusPreLoad=KeePass opstartprestaties optimaliseren...
nl.MyOptPlgPage=Open de plugins webpagina

pl.MyCompCore=Główne pliki KeePassa
pl.MyCompHelp=Przewodnik użytkownika
pl.MyCompNtvLib=Wbudowana obsługa dodatkowych funkcji
pl.MyCompXSL=Arkusze stylów XSL dla plików KDBX XML
pl.MyCompNGen=Optymalizuj wydajność KeePassa
pl.MyCompPreLoad=Optymalizuj czas uruchamiania KeePassa
pl.MyStatusNGen=Optymalizowanie wydajności KeePassa...
pl.MyStatusPreLoad=Optymalizowanie czasu uruchamiania KeePassa...
pl.MyOptPlgPage=Otwórz stronę internetową z wtyczkami

ptBR.MyCompCore=Arquivos principais do KeePass
ptBR.MyCompHelp=Manual do usuário
ptBR.MyCompNtvLib=Biblioteca de suporte nativo
ptBR.MyCompXSL=Folhas de estilo XSL para arquivos XML KDBX
ptBR.MyCompNGen=Otimizar desempenho do KeePass
ptBR.MyCompPreLoad=Otimizar desempenho da inicialização do KeePass
ptBR.MyStatusNGen=Otimizando desempenho do KeePass...
ptBR.MyStatusPreLoad=Otimizando desempenho da inicialização do KeePass...
ptBR.MyOptPlgPage=Abrir página web dos plugins

ptPT.MyCompCore=Ficheiros principais do KeePass
ptPT.MyCompHelp=Manual do utilizador
ptPT.MyCompNtvLib=Biblioteca de suporte nativo
ptPT.MyCompXSL=Folhas de estilo XSL para ficheiros XML KDBX
ptPT.MyCompNGen=Otimize o desempenho do KeePass
ptPT.MyCompPreLoad=Otimize o desempenho do arranque do KeePass
ptPT.MyStatusNGen=Otimizando o desempenho do KeePass...
ptPT.MyStatusPreLoad=Otimizando o desempenho do arranque do KeePass...
ptPT.MyOptPlgPage=Abrir a página web dos miniaplicativos

ru.MyCompCore=Основные файлы KeePass
ru.MyCompHelp=Руководство пользователя
ru.MyCompNtvLib=Встроенная библиотека поддержки
ru.MyCompXSL=Таблицы стилей XSL для XML-файлов KDBX
ru.MyCompNGen=Оптимизировать производительность KeePass
ru.MyCompPreLoad=Оптимизировать скорость запуска KeePass
ru.MyStatusNGen=Оптимизация производительности KeePass...
ru.MyStatusPreLoad=Оптимизация скорости запуска KeePass...
ru.MyOptPlgPage=Открыть веб-страницу плагинов

sl.MyCompCore=Datoteke jedra KeePassa
sl.MyCompHelp=Navodila za uporabo
sl.MyCompNtvLib=Izvorna podporna knjižnica
sl.MyCompXSL=XSL slogovne datoteke za datoteke KDBX XML
sl.MyCompNGen=Optimizirajte delovanja KeePassa
sl.MyCompPreLoad=Optimizirajte delovanje zagona KeePassa
sl.MyStatusNGen=Optimizacija delovanja KeePassa...
sl.MyStatusPreLoad=Optimizacija zagonskega delovanja KeePassa...
sl.MyOptPlgPage=Odpri spletno stran vtičnikov

[Tasks]
Name: FileAssoc; Description: {cm:AssocFileExtension,{#MyAppNameShort},.kdbx}
Name: DesktopIcon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
; Name: QuickLaunchIcon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Components]
Name: Core; Description: "{cm:MyCompCore}"; Flags: fixed; Types: full compact custom
Name: UserDoc; Description: "{cm:MyCompHelp}"; Types: full custom
Name: KeePassLibC; Description: "{cm:MyCompNtvLib}"; Types: full custom
; Name: NativeLib; Description: Native Crypto Library (Fast Key Transformations); Types: full custom
Name: XSL; Description: "{cm:MyCompXSL}"; Types: full custom
Name: NGen; Description: "{cm:MyCompNGen}"; Types: full custom; ExtraDiskSpaceRequired: 8388608
Name: PreLoad; Description: "{cm:MyCompPreLoad}"; Types: full custom; ExtraDiskSpaceRequired: 2048
; Name: FileAssoc; Description: {cm:AssocFileExtension,{#MyAppNameShort},.kdbx}; Types: full custom

[Dirs]
Name: "{app}\Languages"; Flags: uninsalwaysuninstall
Name: "{app}\Plugins"; Flags: uninsalwaysuninstall

[Files]
Source: ..\Build\KeePass_Distrib\KeePass.exe; DestDir: {app}; Flags: ignoreversion; Components: Core
Source: ..\Build\KeePass_Distrib\KeePass.XmlSerializers.dll; DestDir: {app}; Flags: ignoreversion; Components: Core
Source: ..\Build\KeePass_Distrib\KeePass.exe.config; DestDir: {app}; Flags: ignoreversion; Components: Core
Source: ..\Build\KeePass_Distrib\KeePass.config.xml; DestDir: {app}; Flags: ignoreversion; Components: Core
Source: ..\Build\KeePass_Distrib\License.txt; DestDir: {app}; Flags: ignoreversion; Components: Core
Source: ..\Build\KeePass_Distrib\ShInstUtil.exe; DestDir: {app}; Flags: ignoreversion; Components: Core
Source: ..\Build\KeePass_Distrib\KeePass.chm; DestDir: {app}; Flags: ignoreversion; Components: UserDoc
Source: ..\Build\KeePass_Distrib\KeePassLibC32.dll; DestDir: {app}; Flags: ignoreversion; Components: KeePassLibC
Source: ..\Build\KeePass_Distrib\KeePassLibC64.dll; DestDir: {app}; Flags: ignoreversion; Components: KeePassLibC
; Source: ..\Build\KeePass_Distrib\KeePassNtv32.dll; DestDir: {app}; Flags: ignoreversion; Components: NativeLib
; Source: ..\Build\KeePass_Distrib\KeePassNtv64.dll; DestDir: {app}; Flags: ignoreversion; Components: NativeLib
Source: ..\Build\KeePass_Distrib\XSL\KDBX_Common.xsl; DestDir: {app}\XSL; Flags: ignoreversion; Components: XSL
Source: ..\Build\KeePass_Distrib\XSL\KDBX_DetailsFull_HTML.xsl; DestDir: {app}\XSL; Flags: ignoreversion; Components: XSL
Source: ..\Build\KeePass_Distrib\XSL\KDBX_DetailsLight_HTML.xsl; DestDir: {app}\XSL; Flags: ignoreversion; Components: XSL
Source: ..\Build\KeePass_Distrib\XSL\KDBX_PasswordsOnly_TXT.xsl; DestDir: {app}\XSL; Flags: ignoreversion; Components: XSL
Source: ..\Build\KeePass_Distrib\XSL\KDBX_Tabular_HTML.xsl; DestDir: {app}\XSL; Flags: ignoreversion; Components: XSL

[Registry]
; Always unregister .kdbx association at uninstall
Root: HKCR; Subkey: .kdbx; Flags: uninsdeletekey; Tasks: not FileAssoc
Root: HKCR; Subkey: kdbxfile; Flags: uninsdeletekey; Tasks: not FileAssoc
; Register .kdbx association at install, and unregister at uninstall
Root: HKCR; Subkey: .kdbx; ValueType: string; ValueData: kdbxfile; Flags: uninsdeletekey; Tasks: FileAssoc
Root: HKCR; Subkey: kdbxfile; ValueType: string; ValueData: KeePass Database; Flags: uninsdeletekey; Tasks: FileAssoc
Root: HKCR; Subkey: kdbxfile; ValueType: string; ValueName: AlwaysShowExt; Flags: uninsdeletekey; Tasks: FileAssoc
Root: HKCR; Subkey: kdbxfile\DefaultIcon; ValueType: string; ValueData: """{app}\{#MyAppExeName}"",0"; Flags: uninsdeletekey; Tasks: FileAssoc
Root: HKCR; Subkey: kdbxfile\shell\open; ValueType: string; ValueData: &Open with {#MyAppName}; Flags: uninsdeletekey; Tasks: FileAssoc
Root: HKCR; Subkey: kdbxfile\shell\open\command; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: FileAssoc

; [INI]
; Filename: {app}\{#MyAppUrlName}; Section: InternetShortcut; Key: URL; String: {#MyAppURL}

[Icons]
; Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}
; Name: {group}\{cm:ProgramOnTheWeb,{#MyAppName}}; Filename: {app}\{#MyAppUrlName}
; Name: {group}\Help; Filename: {app}\{#MyAppHelpName}; Components: UserDoc
; Name: {group}\{cm:UninstallProgram,{#MyAppName}}; Filename: {uninstallexe}
Name: {autoprograms}\{#MyAppNameShortEx}; Filename: {app}\{#MyAppExeName}
Name: {autodesktop}\{#MyAppNameShortEx}; Filename: {app}\{#MyAppExeName}; Tasks: DesktopIcon
; Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppNameShortEx}; Filename: {app}\{#MyAppExeName}; Tasks: QuickLaunchIcon; Check: MyAppDataCheck

[Run]
; Filename: {app}\KeePass.exe; Parameters: -RegisterFileExt; Components: FileAssoc
Filename: {app}\ShInstUtil.exe; Parameters: net_check; WorkingDir: {app}; Flags: skipifdoesntexist skipifsilent
Filename: {app}\ShInstUtil.exe; Parameters: preload_register; WorkingDir: {app}; StatusMsg: "{cm:MyStatusPreLoad}"; Flags: skipifdoesntexist; Components: PreLoad
Filename: {app}\ShInstUtil.exe; Parameters: ngen_install; WorkingDir: {app}; StatusMsg: "{cm:MyStatusNGen}"; Flags: skipifdoesntexist; Components: NGen
Filename: {app}\{#MyAppExeName}; Description: "{cm:LaunchProgram,{#MyAppNameShort}}"; Flags: postinstall nowait skipifsilent
Filename: "https://keepass.info/plugins.html"; Description: "{cm:MyOptPlgPage}"; Flags: postinstall shellexec skipifsilent unchecked

[UninstallRun]
; Filename: {app}\KeePass.exe; Parameters: -UnregisterFileExt
Filename: {app}\ShInstUtil.exe; Parameters: preload_unregister; WorkingDir: {app}; Flags: skipifdoesntexist; RunOnceId: "PreLoad"; Components: PreLoad
Filename: {app}\ShInstUtil.exe; Parameters: ngen_uninstall; WorkingDir: {app}; Flags: skipifdoesntexist; RunOnceId: "NGen"; Components: NGen

; Delete old files when upgrading
[InstallDelete]
Name: {app}\{#MyAppUrlName}; Type: files
Name: {app}\XSL\KDBX_DetailsFull.xsl; Type: files
Name: {app}\XSL\KDBX_DetailsLite.xsl; Type: files
Name: {app}\XSL\KDBX_PasswordsOnly.xsl; Type: files
Name: {app}\XSL\KDBX_Styles.css; Type: files
Name: {app}\XSL\KDBX_Tabular.xsl; Type: files
Name: {app}\XSL\TableHeader.gif; Type: files
Name: {group}\{#MyAppName}.lnk; Type: files
Name: {group}\{cm:ProgramOnTheWeb,{#MyAppName}}.lnk; Type: files
Name: {group}\Help.lnk; Type: files
Name: {group}\{cm:UninstallProgram,{#MyAppName}}.lnk; Type: files
Name: {group}; Type: dirifempty
Name: {userdesktop}\{#MyAppName}.lnk; Type: files; Check: MyDesktopCheck
Name: {userdesktop}\{#MyAppNameShortEx}.lnk; Type: files; Check: MyDesktopCheck
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}.lnk; Type: files; Check: MyAppDataCheck
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppNameShortEx}.lnk; Type: files; Check: MyAppDataCheck

; [UninstallDelete]
; Type: files; Name: {app}\{#MyAppUrlName}

[Code]
function MyDesktopCheck(): Boolean;
begin
  try
    ExpandConstant('{userdesktop}');
    Result := True;
  except
    Result := False;
  end;
end;

function MyAppDataCheck(): Boolean;
begin
  try
    ExpandConstant('{userappdata}');
    Result := True;
  except
    Result := False;
  end;
end;

function MyGetProgramFiles(Param: String): String;
begin
  if IsWin64() then
    Result := ExpandConstant('{autopf64}')
  else
    Result := ExpandConstant('{autopf}');
end;
