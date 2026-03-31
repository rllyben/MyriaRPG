; MyriaRPG Inno Setup Script
; Requires .NET 8 Desktop Runtime on the target machine.
; Build command: dotnet publish -c Release

#define MyAppName "MyriaRPG"
#define MyAppVersion "0.5"
#define MyAppPublisher "Rhyen"
#define MyAppExeName "MyriaRPG.exe"
#define PublishDir "C:\Users\rhyen\OEBB\source\repos\MyriaRPG\bin\Release\net8.0-windows\publish"

[Setup]
AppId={{371DF85A-2C8C-4A38-8C5F-EA1B0AD06A41}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=C:\Users\rhyen\OEBB\source\repos\MyriaRPG\Setup
OutputBaseFilename=MyriaRPG_Setup
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Single wildcard copies everything from publish — including the Data\ subfolder
; with its correct subdirectory structure (Data\common\, Data\locales\, etc.)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Checks if any .NET 8 Desktop Runtime version is installed.
// The runtime registers subkeys like "8.0.x" under this registry path.
function IsDotNet8DesktopInstalled: Boolean;
var
  Versions: TArrayOfString;
  I: Integer;
begin
  Result := False;

  // System-wide install (most common)
  if RegGetSubkeyNames(HKLM,
      'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App',
      Versions) then
  begin
    for I := 0 to GetArrayLength(Versions) - 1 do
      if Pos('8.', Versions[I]) = 1 then
      begin
        Result := True;
        Exit;
      end;
  end;

  // Per-user install fallback
  if RegGetSubkeyNames(HKCU,
      'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App',
      Versions) then
  begin
    for I := 0 to GetArrayLength(Versions) - 1 do
      if Pos('8.', Versions[I]) = 1 then
      begin
        Result := True;
        Exit;
      end;
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;

  if not IsDotNet8DesktopInstalled() then
  begin
    if MsgBox(
      '.NET 8 Desktop Runtime was not detected on this machine.' + #13#10 +
      '#13#10' +
      'MyriaRPG requires the .NET 8 Desktop Runtime to run.' + #13#10 +
      'Please download and install it from:' + #13#10 +
      'https://dotnet.microsoft.com/download/dotnet/8.0' + #13#10 +
      '#13#10' +
      'Continue the installation anyway?',
      mbConfirmation, MB_YESNO) = IDNO then
      Result := False;
  end;
end;
