param(
    [string]$FrameRoot = "Assets/Res/Bosses/Potato/Frames",
    [int]$AlphaThreshold = 16,
    [int]$MaximumBottomGap = 24,
    [switch]$ValidateOnly
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$cleanerSource = @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

public sealed class ArtifactComponent
{
    public int Id;
    public int PixelCount;
    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;
}

public sealed class ArtifactSummary
{
    public int FileCount;
    public int FilesWithArtifacts;
    public int ArtifactComponentCount;
    public int ArtifactPixelCount;
}

internal sealed class ArtifactAnalysis
{
    public int Width;
    public int Height;
    public int Stride;
    public byte[] Pixels;
    public int[] Labels;
    public List<ArtifactComponent> Components;
    public int RootComponentId;
}

public static class PotatoFrameArtifactCleaner
{
    public static ArtifactSummary ScanDirectory(string frameRoot, int alphaThreshold, int maximumBottomGap)
    {
        ArtifactSummary summary = new ArtifactSummary();
        foreach (string path in GetFramePaths(frameRoot))
        {
            using (var bitmap = new Bitmap(path))
            {
                ArtifactAnalysis analysis = Analyze(bitmap, alphaThreshold);
                List<ArtifactComponent> artifacts = FindBottomArtifacts(analysis, maximumBottomGap);
                summary.FileCount++;
                if (artifacts.Count == 0)
                {
                    continue;
                }

                summary.FilesWithArtifacts++;
                summary.ArtifactComponentCount += artifacts.Count;
                summary.ArtifactPixelCount += artifacts.Sum(component => component.PixelCount);
            }
        }

        return summary;
    }

    public static ArtifactSummary CleanDirectory(string frameRoot, int alphaThreshold, int maximumBottomGap)
    {
        ArtifactSummary summary = new ArtifactSummary();
        foreach (string path in GetFramePaths(frameRoot))
        {
            ArtifactAnalysis analysis;
            List<ArtifactComponent> artifacts;
            using (var bitmap = new Bitmap(path))
            {
                analysis = Analyze(bitmap, alphaThreshold);
                artifacts = FindBottomArtifacts(analysis, maximumBottomGap);
            }

            summary.FileCount++;
            if (artifacts.Count == 0)
            {
                continue;
            }

            summary.FilesWithArtifacts++;
            summary.ArtifactComponentCount += artifacts.Count;
            summary.ArtifactPixelCount += artifacts.Sum(component => component.PixelCount);
            RewriteWithoutArtifacts(path, analysis, artifacts);
        }

        return summary;
    }

    private static string[] GetFramePaths(string frameRoot)
    {
        return Directory.GetFiles(frameRoot, "*.png", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static ArtifactAnalysis Analyze(Bitmap bitmap, int alphaThreshold)
    {
        ArtifactAnalysis analysis = new ArtifactAnalysis
        {
            Width = bitmap.Width,
            Height = bitmap.Height
        };
        analysis.Pixels = ReadPixels(bitmap, out analysis.Stride);
        analysis.Labels = new int[analysis.Width * analysis.Height];
        for (int index = 0; index < analysis.Labels.Length; index++)
        {
            analysis.Labels[index] = -1;
        }

        analysis.Components = FindComponents(analysis, alphaThreshold);
        if (analysis.Components.Count == 0)
        {
            throw new InvalidDataException("Empty boss frame.");
        }

        analysis.RootComponentId = analysis.Components
            .OrderByDescending(component => component.PixelCount)
            .First()
            .Id;
        return analysis;
    }

    private static List<ArtifactComponent> FindBottomArtifacts(ArtifactAnalysis analysis, int maximumBottomGap)
    {
        ArtifactComponent root = analysis.Components.First(component => component.Id == analysis.RootComponentId);
        return analysis.Components
            .Where(component => component.Id != root.Id && component.MinY > root.MaxY + maximumBottomGap)
            .ToList();
    }

    private static List<ArtifactComponent> FindComponents(ArtifactAnalysis analysis, int alphaThreshold)
    {
        List<ArtifactComponent> components = new List<ArtifactComponent>();
        int[] offsetX = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] offsetY = new int[] { -1, -1, -1, 0, 0, 1, 1, 1 };
        Queue<int> queue = new Queue<int>();

        for (int y = 0; y < analysis.Height; y++)
        {
            for (int x = 0; x < analysis.Width; x++)
            {
                int startIndex = y * analysis.Width + x;
                if (analysis.Labels[startIndex] >= 0 || AlphaAt(analysis, x, y) < alphaThreshold)
                {
                    continue;
                }

                ArtifactComponent component = new ArtifactComponent
                {
                    Id = components.Count,
                    MinX = x,
                    MaxX = x,
                    MinY = y,
                    MaxY = y
                };
                analysis.Labels[startIndex] = component.Id;
                queue.Enqueue(startIndex);

                while (queue.Count > 0)
                {
                    int index = queue.Dequeue();
                    int pixelX = index % analysis.Width;
                    int pixelY = index / analysis.Width;
                    component.PixelCount++;
                    if (pixelX < component.MinX) component.MinX = pixelX;
                    if (pixelX > component.MaxX) component.MaxX = pixelX;
                    if (pixelY < component.MinY) component.MinY = pixelY;
                    if (pixelY > component.MaxY) component.MaxY = pixelY;

                    for (int neighbor = 0; neighbor < offsetX.Length; neighbor++)
                    {
                        int neighborX = pixelX + offsetX[neighbor];
                        int neighborY = pixelY + offsetY[neighbor];
                        if (neighborX < 0 || neighborX >= analysis.Width || neighborY < 0 || neighborY >= analysis.Height)
                        {
                            continue;
                        }

                        int neighborIndex = neighborY * analysis.Width + neighborX;
                        if (analysis.Labels[neighborIndex] >= 0 || AlphaAt(analysis, neighborX, neighborY) < alphaThreshold)
                        {
                            continue;
                        }

                        analysis.Labels[neighborIndex] = component.Id;
                        queue.Enqueue(neighborIndex);
                    }
                }

                components.Add(component);
            }
        }

        return components;
    }

    private static void RewriteWithoutArtifacts(string path, ArtifactAnalysis analysis, List<ArtifactComponent> artifacts)
    {
        bool[] removedComponents = new bool[analysis.Components.Count];
        foreach (ArtifactComponent artifact in artifacts)
        {
            removedComponents[artifact.Id] = true;
        }

        using (var bitmap = new Bitmap(analysis.Width, analysis.Height, PixelFormat.Format32bppArgb))
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, analysis.Width, analysis.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                int outputStride = Math.Abs(data.Stride);
                byte[] output = new byte[outputStride * analysis.Height];
                for (int y = 0; y < analysis.Height; y++)
                {
                    Buffer.BlockCopy(analysis.Pixels, y * analysis.Stride, output, y * outputStride, analysis.Width * 4);
                }

                for (int y = 0; y < analysis.Height; y++)
                {
                    for (int x = 0; x < analysis.Width; x++)
                    {
                        int componentId = analysis.Labels[y * analysis.Width + x];
                        if (componentId < 0 || !removedComponents[componentId])
                        {
                            continue;
                        }

                        int pixelOffset = y * outputStride + x * 4;
                        output[pixelOffset] = 0;
                        output[pixelOffset + 1] = 0;
                        output[pixelOffset + 2] = 0;
                        output[pixelOffset + 3] = 0;
                    }
                }

                Marshal.Copy(output, 0, data.Scan0, output.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            string temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".artifact.tmp.png";
            bitmap.Save(temporaryPath, ImageFormat.Png);
            File.Copy(temporaryPath, path, true);
            File.Delete(temporaryPath);
        }
    }

    private static byte[] ReadPixels(Bitmap bitmap, out int stride)
    {
        BitmapData data = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);
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

    private static int AlphaAt(ArtifactAnalysis analysis, int x, int y)
    {
        return analysis.Pixels[y * analysis.Stride + x * 4 + 3];
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

Add-Type -TypeDefinition $cleanerSource -ReferencedAssemblies System.Drawing

$resolvedRoot = (Resolve-Path -LiteralPath $FrameRoot).Path
if ($ValidateOnly)
{
    $result = [PotatoFrameArtifactCleaner]::ScanDirectory($resolvedRoot, $AlphaThreshold, $MaximumBottomGap)
    if ($result.ArtifactComponentCount -gt 0)
    {
        throw "Found $($result.ArtifactComponentCount) lower artifact components in $($result.FilesWithArtifacts) boss frames."
    }
}
else
{
    $result = [PotatoFrameArtifactCleaner]::CleanDirectory($resolvedRoot, $AlphaThreshold, $MaximumBottomGap)
}

Write-Host "Boss frame artifact cleanup finished"
Write-Host "Frames scanned: $($result.FileCount)"
Write-Host "Frames cleaned: $($result.FilesWithArtifacts)"
Write-Host "Artifact components: $($result.ArtifactComponentCount)"
Write-Host "Artifact pixels: $($result.ArtifactPixelCount)"

