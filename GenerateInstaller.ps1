Compress-Archive -Path .\InCollege\bin\Debug\* -DestinationPath .\InCollege.Installer\Resources\InCollege_Client -Update
mv .\InCollege.Installer\Resources\InCollege_Client.zip .\InCollege.Installer\Resources\InCollege_Client -force
Compress-Archive -Path .\InCollege.Server\bin\Debug\* -DestinationPath .\InCollege.Installer\Resources\InCollege_Server -Update
mv .\InCollege.Installer\Resources\InCollege_Server.zip .\InCollege.Installer\Resources\InCollege_Server -force
MSBuild