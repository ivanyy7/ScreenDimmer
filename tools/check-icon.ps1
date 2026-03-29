$icoPath = "c:\Work\ScreenDimmer\Assets\app.ico"
$exePath = "c:\Work\ScreenDimmer\bin\Release\net8.0-windows\ScreenDimmer.exe"
Add-Type -AssemblyName System.Drawing
$b = [System.IO.File]::ReadAllBytes($icoPath)
$count = [BitConverter]::ToUInt16($b, 4)
Write-Host "ICO file bytes: $($b.Length) image count: $count"
Write-Host "First 8 bytes:" ($b[0..7] | ForEach-Object { $_.ToString("X2") }) -join " "
if (Test-Path $exePath) {
    $ic = [System.Drawing.Icon]::ExtractAssociatedIcon($exePath)
    Write-Host "EXE ExtractAssociatedIcon size: $($ic.Width)x$($ic.Height) handle: $($ic.Handle)"
} else {
    Write-Host "EXE not found"
}
