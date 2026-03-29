# Пересборка Assets\app.ico из произвольного PNG (после смены макета иконки).
# Пример: .\png-to-ico.ps1 -InputPng C:\path\to\icon.png
param(
    [Parameter(Mandatory = $true)][string]$InputPng,
    [string]$OutputIco = "$PSScriptRoot\..\Assets\app.ico"
)
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing
$bmp = [System.Drawing.Bitmap]::FromFile($InputPng)
$hIcon = $bmp.GetHicon()
$ico = [System.Drawing.Icon]::FromHandle($hIcon)
$fs = [System.IO.File]::Create($OutputIco)
$ico.Save($fs)
$fs.Close()
$ico.Dispose()
$bmp.Dispose()
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Native {
  [DllImport("user32.dll", CharSet = CharSet.Auto)]
  public static extern bool DestroyIcon(IntPtr hIcon);
}
"@
[Native]::DestroyIcon($hIcon)
Write-Host "OK: $OutputIco"
