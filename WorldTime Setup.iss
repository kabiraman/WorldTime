; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{2F770525-535E-48F0-9B97-130621AA8A84}
AppName=WorldTime
AppVerName=WorldTime 0.9.11
AppPublisher=Karthik Abiraman
AppPublisherURL=http://www.codeplex.com/worldtime/
AppSupportURL=http://www.codeplex.com/worldtime/
AppUpdatesURL=http://www.codeplex.com/worldtime/
DefaultDirName={localappdata}\WorldTime
DefaultGroupName=WorldTime
AllowNoIcons=yes
OutputBaseFilename=WorldTime_0.9.11_setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=none

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\Karthik\Documents\Work\Tech\Desktop Application\DotNet\C#\WorldTime\WorldTime\bin\x86\Release\WorldTime.exe"; DestDir: "{localappdata}\WorldTime"; Flags: ignoreversion
Source: "C:\Users\Karthik\Documents\Work\Tech\Desktop Application\DotNet\C#\WorldTime\WorldTime\bin\x86\Release\WorldTime.exe.config"; DestDir: "{localappdata}\WorldTime"; Flags: ignoreversion
Source: "C:\Users\Karthik\Documents\Work\Tech\Desktop Application\DotNet\C#\WorldTime\WorldTime\bin\x86\Release\System.Data.SQLite.dll"; DestDir: "{localappdata}\WorldTime"; Flags: ignoreversion
Source: "C:\Users\Karthik\Documents\Work\Tech\Desktop Application\DotNet\C#\WorldTime\WorldTime\bin\x86\Release\System.Data.SQLite.xml"; DestDir: "{localappdata}\WorldTime"; Flags: ignoreversion
Source: "C:\Users\Karthik\Documents\Work\Tech\Desktop Application\DotNet\C#\WorldTime\WorldTime\WorldTime.db"; DestDir: "{localappdata}\WorldTime"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\WorldTime"; Filename: "{localappdata}\WorldTime\WorldTime.exe"
Name: "{group}\{cm:ProgramOnTheWeb,WorldTime}"; Filename: "http://www.codeplex.com/worldtime/"
Name: "{group}\{cm:UninstallProgram,WorldTime}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\WorldTime"; Filename: "{localappdata}\WorldTime.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\WorldTime"; Filename: "{localappdata}\WorldTime\WorldTime.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{localappdata}\WorldTime\WorldTime.exe"; Description: "{cm:LaunchProgram,WorldTime}"; Flags: nowait postinstall skipifsilent

