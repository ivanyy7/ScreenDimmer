# Сборка валидного для GDI+ app.ico (через System.Drawing — формат принимает new Icon() в .NET).
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$out = Join-Path $root "Assets\app.ico"
New-Item -ItemType Directory -Force -Path (Split-Path $out) | Out-Null

function New-IconFromBmp([System.Drawing.Bitmap]$b) {
    $h = $b.GetHicon()
    $ico = [System.Drawing.Icon]::FromHandle($h)
    $cloned = [System.Drawing.Icon]$ico.Clone()
    $ico.Dispose()
    Add-Type -Namespace W32 -Name Native -MemberDefinition @"
[DllImport("user32.dll", CharSet = CharSet.Auto)] public static extern bool DestroyIcon(System.IntPtr hIcon);
"@
    [W32.Native]::DestroyIcon($h) | Out-Null
    return $cloned
}

# 32x32 — основной ресурс для трея и exe
$b32 = New-Object System.Drawing.Bitmap 32, 32
$g = [System.Drawing.Graphics]::FromImage($b32)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::FromArgb(255, 18, 18, 22))
$br = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 140, 120, 220))
$g.FillEllipse($br, 2, 2, 28, 28)
$br2 = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 240, 240, 240))
$g.FillPie($br2, 8, 6, 18, 18, 200, 280)
$g.Dispose()
$br.Dispose()
$br2.Dispose()

$icon32 = New-IconFromBmp $b32
$fs = [System.IO.File]::Create($out)
$icon32.Save($fs)
$fs.Close()
$icon32.Dispose()
$b32.Dispose()

# Проверка: GDI+ должен принять файл как Icon
$test = New-Object System.Drawing.Icon($out)
Write-Host "OK: $out sizes $($test.Width)x$($test.Height)"
$test.Dispose()
