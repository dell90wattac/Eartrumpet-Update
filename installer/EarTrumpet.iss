; Inno Setup script for the EarTrumpet Win11-refresh build.
; CI compiles this after msbuild has produced the loose build; see .github/workflows/build.yml.
; SourceDir is passed on the command line (/DSourceDir=...) and points at the msbuild output folder.

#define MyAppName "EarTrumpet"
#define MyAppExe "EarTrumpet.exe"
#define MyPublisher "File-New-Project (Win11 refresh build)"

#ifndef SourceDir
  #define SourceDir "..\artifacts"
#endif

#define MyAppVersion GetVersionNumbersString(SourceDir + "\" + MyAppExe)

[Setup]
AppId={{5C0A7651-414F-4931-A5B3-5EAFE8CA4E8E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyPublisher}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExe}
UninstallDisplayName={#MyAppName}
SetupIconFile=..\EarTrumpet\Assets\Icon-Light.ico
OutputDir=Output
OutputBaseFilename=EarTrumpet-Win11-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
MinVersion=10.0.14393
; Per-user install by default (no admin prompt); the dialog lets people choose all-users instead.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
; EarTrumpet holds this mutex while running (see SingleInstanceAppMutex.cs).
; Setup and Uninstall ask the person to quit it first, which also catches the Store version.
AppMutex=EarTrumpet-0e510f7b-aed2-40b0-ad72-d2d3fdc89a02
CloseApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.%n%nIf the Microsoft Store version of EarTrumpet is installed, uninstall it first (Settings > Apps). Both versions use the same instance lock, so only one of them can run at a time.%n%nClick Next to continue.

[Tasks]
Name: "startup"; Description: "Start EarTrumpet when I sign in to Windows"; GroupDescription: "Startup:"
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\LICENSE"; DestDir: "{app}"; DestName: "LICENSE.txt"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"; WorkingDir: "{app}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExe}"""; Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExe}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
