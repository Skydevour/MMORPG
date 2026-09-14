param([long]$WindowHandle, [string]$OutputPath)
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class GameWindowCapture {
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr window, out Rect rect);
}
'@
$rectangle = New-Object GameWindowCapture+Rect
if (-not [GameWindowCapture]::GetWindowRect([IntPtr]$WindowHandle, [ref]$rectangle)) { throw '无法读取游戏窗口区域。' }
$bitmap = New-Object System.Drawing.Bitmap(($rectangle.Right - $rectangle.Left), ($rectangle.Bottom - $rectangle.Top))
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
try {
    $graphics.CopyFromScreen($rectangle.Left, $rectangle.Top, 0, 0, $bitmap.Size)
    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
} finally { $graphics.Dispose(); $bitmap.Dispose() }
Write-Output $OutputPath
