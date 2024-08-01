"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" -exec bypass -Command "Stop-VM -Name testuefidbgvhdxv2 -TurnOff -Force"
"C:\Program Files (x86)\Windows Kits\10\Debuggers\x64\symstore.exe" add /r /f "E:\git\UefiAotHyperV\WindbgUefiSharp\Windbg\bin\Release\net7.0\win-x64\publish" /s "%SYMBOL_STORE%" /t niii
"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" -exec bypass -Command " Mount-VHD -Path 'F:\hyperv\testuefidbgvhdxv2\Virtual Hard Disks\testuefidbgvhdxv2.vhdx'"
copy "E:\git\UefiAotHyperV\WindbgUefiSharp\Windbg\bin\Release\net7.0\win-x64\native\Windbg.exe" "K:\Windbg.efi"  /Y
"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" -exec bypass -Command "Dismount-VHD 'F:\hyperv\testuefidbgvhdxv2\Virtual Hard Disks\testuefidbgvhdxv2.vhdx'"