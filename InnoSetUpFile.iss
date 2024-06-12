#define MyAppName "AuthB2C"
#define MyAppVersion "1.0"
#define MyAppPublisher "Self Free"
#define MyAppURL ""
#define MyAppExeName "AuthB2C.exe"
#define MyAppAssocName MyAppName + " File"
#define MyAppAssocExt ".myp"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt
#define MyAppPublishDir "bin\Debug"
;#define MyServicePublishDir "ElevatedPrivilegeService\bin\Debug"
;#define MyServiceExeName "ElevatedPrivilegeService.exe"

[Setup]
ArchitecturesInstallIn64BitMode=x64
AppId={{E25CCB02-6DD6-45C7-9171-0175TADB9B36}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf64}\{#MyAppName}
ChangesAssociations=yes
DisableProgramGroupPage=yes
;PrivilegesRequired=admin
PrivilegesRequired=none
PrivilegesRequiredOverridesAllowed=dialog
OutputBaseFilename=AuthB2CSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\logo.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[InstallDelete]
Type: files; Name: "{app}\application.json"
Type: filesandordirs; Name: "{app}\Config"
Type: filesandordirs; Name: "{app}\runtimes"
Type: filesandordirs; Name: "{app}\refs"

[UninstallDelete]
Type: files; Name: "{app}\application.json"
Type: filesandordirs; Name: "{app}\Config"
Type: filesandordirs; Name: "{app}\runtimes"
Type: filesandordirs; Name: "{app}\refs"

[UninstallRun]
Filename: "schtasks.exe"; Parameters: "/DELETE /TN ""{#MyAppName}"" /F"; Flags: runhidden; RunOnceId: "DeleteScheduledTask"

[Files]
Source: "C:\Users\dell\source\repos\AuthB2C\AuthB2C\{#MyAppPublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\dell\source\repos\AuthB2C\AuthB2C\{#MyAppPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\dell\source\repos\AuthB2C\logo.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\dell\source\repos\AuthB2C\AuthB2C.xml"; DestDir: "{app}"; Flags: ignoreversion
;Service
;Source: "C:\Users\dell\Downloads\csharp-userapp\csharp-userapp-master\ElevatedPrivilegeService_Net\{#MyServicePublishDir}\{#MyServiceExeName}"; DestDir: "{app}"; Flags: ignoreversion
;Source: "C:\Users\dell\Downloads\csharp-userapp\csharp-userapp-master\ElevatedPrivilegeService_Net\{#MyServicePublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files 

[Registry]
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocExt}\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".myp"; ValueData: ""

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: runascurrentuser nowait postinstall skipifsilent;
;Filename: "{app}\{#MyAppExeName}"; Flags: runascurrentuser; Parameters: "-install -svcName ""AuthB2C"" -svcDesc ""AuthB2C"" -mainExe ""AuthB2C.exe""  ";
;Filename: "{app}\{#MyAppExeName}"; Parameters: "--install"


[Code]
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  AppPath: String;
  Command: String;
begin
  if CurStep = ssPostInstall then
  begin
    AppPath := ExpandConstant('"''{app}\{#MyAppExeName}''"');
    //Command := Format('/CREATE /F /TN "{#MyAppName}" /TR %s /SC ONLOGON /RL HIGHEST', [AppPath]);
    Command := '/CREATE /XML "' + ExpandConstant('{app}\AuthB2C.xml') + '" /TN "{#MyAppName}"'
    // Create a scheduled task to run the app at system startup for all users with highest privileges
    if not Exec('schtasks.exe', Command, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      MsgBox('Failed to create scheduled task. Error code: ' + IntToStr(ResultCode), mbError, MB_OK);
  end;
end;