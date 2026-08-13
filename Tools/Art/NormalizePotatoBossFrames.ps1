param(
    [string]$FrameRoot = "Assets/Res/Bosses/Potato/Frames",
    [int]$FootBandHeight = 12,
    [int]$HorizontalPadding = 4,
    [int]$TopPadding = 4,
    [int]$AlphaThreshold = 16,
    [switch]$ValidateOnly
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$normalizerSource = @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

public sealed class BossFrameMetrics
{
    public string Path;
    public int Width;
    public int Height;
    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;
    public double FootCenterX;

    public int ContentWidth { get { return MaxX - MinX + 1; } }
    public int ContentHeight { get { return MaxY - MinY + 1; } }
}

public sealed class BossFrameNormalizationResult
{
    public int FrameCount;
    public int CanvasWidth;
    public int CanvasHeight;
    public int AnchorX;
    public int AnchorY;
    public double MaximumFootCenterError;
}

public static class BossFrameNormalizer
{
    public static BossFrameMetrics Analyze(string path, int footBandHeight, int alphaThreshold)
    {
        using (var bitmap = new Bitmap(path))
        {
            var metrics = new BossFrameMetrics
            {
                Path = path,
                Width = bitmap.Width,
                Height = bitmap.Height,
                MinX = bitmap.Width,
                MinY = bitmap.Height,
                MaxX = -1,
                MaxY = -1
            };

            int stride;
            byte[] pixels = ReadPixels(bitmap, out stride);
            ScanBounds(pixels, stride, bitmap.Width, bitmap.Height, alphaThreshold, metrics);
            if (metrics.MaxX < 0)
            {
                throw new InvalidDataException("空白帧无法计算脚底锚点: " + path);
            }

            int bandStartY = Math.Max(metrics.MinY, metrics.MaxY - footBandHeight + 1);
            long weightedX = 0;
            long alphaWeight = 0;
            for (int y = bandStartY; y <= metrics.MaxY; y++)
            {
                int row = y * stride;
                for (int x = metrics.MinX; x <= metrics.MaxX; x++)
                {
                    int alpha = pixels[row + x * 4 + 3];
                    if (alpha < alphaThreshold)
                    {
                        continue;
                    }

                    weightedX += (long)x * alpha;
                    alphaWeight += alpha;
                }
            }

            if (alphaWeight == 0)
            {
                throw new InvalidDataException("脚底区域没有有效像素: " + path);
            }

            metrics.FootCenterX = (double)weightedX / alphaWeight;
            return metrics;
        }
    }

    public static BossFrameNormalizationResult Normalize(
        string frameRoot,
        int footBandHeight,
        int horizontalPadding,
        int topPadding,
        int alphaThreshold)
    {
        string[] paths = Directory.GetFiles(frameRoot, "*.png", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (paths.Length == 0)
        {
            throw new FileNotFoundException("没有找到土豆 Boss 序列帧", frameRoot);
        }

        List<BossFrameMetrics> frames = paths
            .Select(path => Analyze(path, footBandHeight, alphaThreshold))
            .ToList();

        double maximumHorizontalExtent = frames.Max(frame => Math.Max(
            frame.FootCenterX - frame.MinX,
            frame.MaxX - frame.FootCenterX));
        int halfWidth = (int)Math.Ceiling(maximumHorizontalExtent) + horizontalPadding;
        int canvasWidth = halfWidth * 2 + 1;
        int canvasHeight = frames.Max(frame => frame.ContentHeight) + topPadding;
        int anchorX = halfWidth;
        int anchorY = canvasHeight - 1;

        foreach (BossFrameMetrics frame in frames)
        {
            int offsetX = (int)Math.Round(anchorX - frame.FootCenterX, MidpointRounding.AwayFromZero);
            int offsetY = anchorY - frame.MaxY;
            RewriteFrame(frame.Path, canvasWidth, canvasHeight, offsetX, offsetY);
        }

        return Validate(
            frameRoot,
            footBandHeight,
            alphaThreshold,
            canvasWidth,
            canvasHeight,
            anchorX,
            anchorY);
    }

    public static BossFrameNormalizationResult Validate(
        string frameRoot,
        int footBandHeight,
        int alphaThreshold,
        int expectedWidth,
        int expectedHeight,
        int expectedAnchorX,
        int expectedAnchorY)
    {
        string[] paths = Directory.GetFiles(frameRoot, "*.png", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        List<BossFrameMetrics> frames = paths
            .Select(path => Analyze(path, footBandHeight, alphaThreshold))
            .ToList();

        foreach (BossFrameMetrics frame in frames)
        {
            if (frame.Width != expectedWidth || frame.Height != expectedHeight)
            {
                throw new InvalidDataException("帧尺寸不一致: " + frame.Path);
            }

            if (frame.MaxY != expectedAnchorY)
            {
                throw new InvalidDataException("脚底没有贴齐画布底部: " + frame.Path);
            }
        }

        return new BossFrameNormalizationResult
        {
            FrameCount = frames.Count,
            CanvasWidth = expectedWidth,
            CanvasHeight = expectedHeight,
            AnchorX = expectedAnchorX,
            AnchorY = expectedAnchorY,
            MaximumFootCenterError = frames.Max(frame => Math.Abs(frame.FootCenterX - expectedAnchorX))
        };
    }

    private static void RewriteFrame(string path, int width, int height, int offsetX, int offsetY)
    {
        string temporaryPath = path + ".normalize.tmp.png";
        using (var source = new Bitmap(path))
        using (var destination = new Bitmap(width, height, PixelFormat.Format32bppArgb))
        using (Graphics graphics = Graphics.FromImage(destination))
        {
            graphics.Clear(Color.Transparent);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImageUnscaled(source, offsetX, offsetY);
            destination.Save(temporaryPath, ImageFormat.Png);
        }

        File.Copy(temporaryPath, path, true);
        File.Delete(temporaryPath);
    }

    private static byte[] ReadPixels(Bitmap bitmap, out int stride)
    {
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            stride = Math.Abs(data.Stride);
            byte[] pixels = new byte[stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
            if (data.Stride < 0)
            {
                FlipRows(pixels, stride, bitmap.Height);
            }

            return pixels;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    private static void ScanBounds(
        byte[] pixels,
        int stride,
        int width,
        int height,
        int alphaThreshold,
        BossFrameMetrics metrics)
    {
        for (int y = 0; y < height; y++)
        {
            int row = y * stride;
            for (int x = 0; x < width; x++)
            {
                if (pixels[row + x * 4 + 3] < alphaThreshold)
                {
                    continue;
                }

                if (x < metrics.MinX) metrics.MinX = x;
                if (x > metrics.MaxX) metrics.MaxX = x;
                if (y < metrics.MinY) metrics.MinY = y;
                if (y > metrics.MaxY) metrics.MaxY = y;
            }
        }
    }

    private static void FlipRows(byte[] pixels, int stride, int height)
    {
        byte[] row = new byte[stride];
        for (int top = 0, bottom = height - 1; top < bottom; top++, bottom--)
        {
            Buffer.BlockCopy(pixels, top * stride, row, 0, stride);
            Buffer.BlockCopy(pixels, bottom * stride, pixels, top * stride, stride);
            Buffer.BlockCopy(row, 0, pixels, bottom * stride, stride);
        }
    }
}
'@

Add-Type -TypeDefinition $normalizerSource -ReferencedAssemblies System.Drawing

$resolvedRoot = (Resolve-Path -LiteralPath $FrameRoot).Path

if ($ValidateOnly)
{
    $firstFrame = Get-ChildItem -Recurse -File -LiteralPath $resolvedRoot -Filter "*.png" |
        Sort-Object FullName |
        Select-Object -First 1
    if ($null -eq $firstFrame)
    {
        throw "没有找到土豆 Boss 序列帧：$resolvedRoot"
    }

    $firstMetrics = [BossFrameNormalizer]::Analyze($firstFrame.FullName, $FootBandHeight, $AlphaThreshold)
    $anchorX = [int](($firstMetrics.Width - 1) / 2)
    $anchorY = $firstMetrics.Height - 1
    $result = [BossFrameNormalizer]::Validate(
        $resolvedRoot,
        $FootBandHeight,
        $AlphaThreshold,
        $firstMetrics.Width,
        $firstMetrics.Height,
        $anchorX,
        $anchorY)
}
else
{
    $result = [BossFrameNormalizer]::Normalize(
        $resolvedRoot,
        $FootBandHeight,
        $HorizontalPadding,
        $TopPadding,
        $AlphaThreshold)
}

Write-Host "土豆 Boss 序列帧验证通过"
Write-Host "帧数：$($result.FrameCount)"
Write-Host "统一画布：$($result.CanvasWidth)x$($result.CanvasHeight)"
Write-Host "脚底锚点像素：($($result.AnchorX), $($result.AnchorY))"
Write-Host ("最大脚底中心误差：{0:F3} 像素" -f $result.MaximumFootCenterError)

