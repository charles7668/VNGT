﻿; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define SourcePath "bin\GameManager x86"

#define id "EA22D0C2-D9E5-4EBE-964B-4CFC6CD9C086"
[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{{#id}}
AppName=VNGT
AppVersion=0.1.0
AppPublisher=charles
AppPublisherURL=https://github.com/charles7668
AppSupportURL=https://github.com/charles7668/VNGT
AppUpdatesURL=https://github.com/charles7668/VNGT
DefaultDirName={localappdata}/VNGT
DefaultGroupName=VNGT
AllowNoIcons=yes
; The [Icons] "quicklaunchicon" entry uses {userappdata} but its [Tasks] entry has a proper IsAdminInstallMode Check.
UsedUserAreasWarning=no
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=bin/Setup
OutputBaseFilename=Setup x86
SetupIconFile=GameManager\Resources\Icons\app.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
UninstallDisplayIcon=GameManager\Resources\Icons\app.ico
UsePreviousAppDir =false
SetupLogging=yes
UsePreviousTasks=no
;When set to auto, the dialog will only be displayed if Setup does not find a language identifier match. (Default value:yes)
ShowLanguageDialog=auto
UsePreviousLanguage=yes

[Languages]
Name: EN; MessagesFile: "compiler:Default.isl"
Name: CN; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: CT; MessagesFile: "compiler:Languages\ChineseTraditional.isl"


[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
SOURCE: "{#SourcePath}\*" ; DestDir:"{app}\" ; Flags: ignoreversion recursesubdirs createallsubdirs

;icon
SOURCE: "GameManager\Resources\Icons\app.ico" ; DestDir:"{app}" ;DestName:"icon.ico" ; Flags: ignoreversion ; Check: Is64BitInstallMode

; NOTE: Don't use "Flags: ignoreversion" on any shared system files
;Source: "ComCtl32.ocx"; DestDir: "{sys}"; Flags: restartreplace sharedfile regserver


[UninstallDelete]     
Type:filesandordirs; Name:"{app}";


[Icons]
Name: "{group}\VNGT"; Filename: "{app}\GameManager.exe"
Name: "{autodesktop}\VNGT"; Filename: "{app}\GameManager.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\VNGT"; Filename: "{app}\GameManager.exe"; Tasks: quicklaunchicon

[Code]

function SetUninstallIcon(iconPath:string): Boolean;
var
  //InstalledVersion,SubKeyName: String;
  SubKeyName: String;
begin
if (IsWin64()) then begin
SubKeyName :=  'Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{{#id}}_is1';
    RegWriteStringValue(HKLM64,SubKeyName,'DisplayIcon',iconPath);
end else
  begin
    SubKeyName :=  'Software\Microsoft\Windows\CurrentVersion\Uninstall\{{#id}}_is1';
      RegWriteStringValue(HKLM,SubKeyName,'DisplayIcon',iconPath);
  end;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpFinished then
  begin
    SetUninstallIcon(ExpandConstant('{app}\icon.ico'));

  end;
end;
