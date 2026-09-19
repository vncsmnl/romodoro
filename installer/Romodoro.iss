#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef RuntimeArchitecture
  #define RuntimeArchitecture "x64"
#endif
#ifndef RuntimeVersion
  #define RuntimeVersion "10.0.12"
#endif
#ifndef RuntimeUrl
  #define RuntimeUrl "https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-10.0.12-windows-x64-installer"
#endif
#ifndef RuntimeSha256
  #define RuntimeSha256 ""
#endif
#ifndef PublishDir
  #define PublishDir "publish"
#endif
#ifndef OutputDir
  #define OutputDir "artifacts"
#endif

[Setup]
AppId={{A28F1D55-3B05-4E3C-9F50-6D58C3C8A1D2}
AppName=Romodoro
AppVersion={#AppVersion}
AppPublisher=vncsmnl
DefaultDirName={autopf}\Romodoro
DefaultGroupName=Romodoro
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64compatible arm64
ArchitecturesInstallIn64BitMode=x64compatible arm64
OutputDir={#OutputDir}
OutputBaseFilename=Romodoro-win-{#RuntimeArchitecture}-setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName=Romodoro
VersionInfoCompany=vncsmnl
VersionInfoDescription=Romodoro desktop timer
VersionInfoProductName=Romodoro
VersionInfoVersion={#AppVersion}.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Romodoro"; Filename: "{app}\Romodoro.exe"
Name: "{autodesktop}\Romodoro"; Filename: "{app}\Romodoro.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Romodoro.exe"; Description: "Launch Romodoro"; Flags: nowait postinstall skipifsilent

[Code]
function IsRequiredRuntimeInstalled(): Boolean;
var
  RootKey: Integer;
  ArchitectureKey: String;
  KeyName: String;
  VersionNames: TArrayOfString;
  I: Integer;
begin
  Result := False;
#if RuntimeArchitecture == "x86"
  RootKey := HKLM32;
  ArchitectureKey := 'x86';
#elseif RuntimeArchitecture == "arm64"
  RootKey := HKLM64;
  ArchitectureKey := 'arm64';
#else
  RootKey := HKLM64;
  ArchitectureKey := 'x64';
#endif
  KeyName := 'SOFTWARE\dotnet\Setup\InstalledVersions\' + ArchitectureKey + '\sharedfx\Microsoft.NETCore.App';
  if RegGetSubkeyNames(RootKey, KeyName, VersionNames) then
  begin
    for I := 0 to GetArrayLength(VersionNames) - 1 do
    begin
      if (Length(VersionNames[I]) >= 3) and (Copy(VersionNames[I], 1, 3) = '10.') then
      begin
        Result := True;
        Exit;
      end;
    end;
  end;
end;

function DownloadAndInstallRuntime(): String;
var
  RuntimePath: String;
  ExitCode: Integer;
begin
  Result := '';
  RuntimePath := ExpandConstant('{tmp}\dotnet-runtime-{#RuntimeArchitecture}.exe');
  try
    DownloadTemporaryFile('{#RuntimeUrl}', 'dotnet-runtime-{#RuntimeArchitecture}.exe', '{#RuntimeSha256}', nil);
  except
    Result := 'The .NET 10 runtime could not be downloaded or its SHA-256 verification failed.';
    Exit;
  end;

  if not Exec(RuntimePath, '/install /quiet /norestart', '', SW_HIDE, ewWaitUntilTerminated, ExitCode) then
  begin
    Result := 'The .NET 10 runtime installer could not be started.';
    Exit;
  end;
  if ExitCode <> 0 then
    Result := Format('The .NET 10 runtime installer failed with exit code %d.', [ExitCode]);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if IsRequiredRuntimeInstalled() then
    Exit;
  Result := DownloadAndInstallRuntime();
end;
