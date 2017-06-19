MSBuild
Compress-Archive -Path .\InCollege\bin\Release\* -DestinationPath .\InCollege.Installer\Resources\InCollege_Client -Update
mv .\InCollege.Installer\Resources\InCollege_Client.zip .\InCollege.Installer\Resources\InCollege_Client -force
Compress-Archive -Path .\InCollege.Server\bin\Release\* -DestinationPath .\InCollege.Installer\Resources\InCollege_Server -Update
mv .\InCollege.Installer\Resources\InCollege_Server.zip .\InCollege.Installer\Resources\InCollege_Server -force
MSBuild .\InCollege.Installer\InCollege.Installer.csproj
.\InCollege.Installer\bin\Release\InCollege.Installer.exe