$ErrorActionPreference = 'Stop';

Install-BinFile -Name 'cake.mug' -Path 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -command "`"-ExecutionPolicy Bypass -Command $env:ChocolateyInstall\lib\cake.mug.chocotools\tools\build.ps1"`"
