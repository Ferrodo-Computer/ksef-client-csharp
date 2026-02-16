using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using QRCoder;
using SkiaSharp;

namespace KSeF.Client.Api.Services;

public static class QrCodeService
{

    /// <summary>
    /// Generuje kod QR jako tablicę bajtów PNG.
    /// </summary>
    /// <param name="payloadUrl">URL/link do zakodowania.</param>
    /// <param name="pixelsPerModule">Rozmiar modułu w pikselach (domyślnie 20).</param>
    /// <param name="qrCodeResolutionInPx"></param>
    public static byte[] GenerateQrCode(string payloadUrl, int pixelsPerModule = 20, int qrCodeResolutionInPx = 300)
    {
        using QRCodeGenerator gen = new();
        using QRCodeData qrData = gen.CreateQrCode(payloadUrl, QRCodeGenerator.ECCLevel.Default);

        int modules = qrData.ModuleMatrix.Count;
        float cellSize = qrCodeResolutionInPx / (float)modules;

        SKImageInfo info = new(qrCodeResolutionInPx, qrCodeResolutionInPx);
        using SKSurface surface = SKSurface.Create(info);
        SKCanvas skCanvas = surface.Canvas;

        SkiaCanvas canvas = new()
        {
            Canvas = skCanvas
        };
        canvas.SetDisplayScale(1f);

        canvas.FillColor = Colors.White;
        canvas.FillRectangle(0, 0, qrCodeResolutionInPx, qrCodeResolutionInPx);

        canvas.FillColor = Colors.Black;
        for (int y = 0; y < modules; y++)
        {
            for (int x = 0; x < modules; x++)
            {
                if (qrData.ModuleMatrix[y][x])
                {
                    canvas.FillRectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                }
            }
        }

        // Eksport PNG
        using SKImage img = surface.Snapshot();
        using SKData data = img.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <inheritdoc/>
    public static byte[] ResizePng(byte[] pngBytes, int targetWidth, int targetHeight)
    {
        using SKBitmap skBitmap = SKBitmap.Decode(pngBytes);
        SKImageInfo info = new(targetWidth, targetHeight);
        using SKSurface surface = SKSurface.Create(info);
        SkiaCanvas canvas = new() { Canvas = surface.Canvas };
        canvas.SetDisplayScale(1f);

        IImage image = new SkiaImage(skBitmap);
        canvas.DrawImage(image, 0, 0, targetWidth, targetHeight);

        using SKImage snap = surface.Snapshot();
        using SKData encoded = snap.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }

    /// <summary>Dokleja podpis (label) pod istniejącym PNG z kodem QR.</summary>
    public static byte[] AddLabelToQrCode(byte[] qrCodePng, string label, int fontSizePx = 14)
    {
        using SKBitmap skBitmap = SKBitmap.Decode(qrCodePng);
        IImage qrImage = new SkiaImage(skBitmap);
        int width = skBitmap.Width;
        int height = skBitmap.Height;

        Font font = new("Arial", fontSizePx);

        // Pomiar tekstu
        SkiaCanvas measureCanvas = new() { Canvas = SKSurface.Create(new SKImageInfo(1, 1)).Canvas };
        measureCanvas.SetDisplayScale(1f);
        measureCanvas.Font = font;
        measureCanvas.FontSize = fontSizePx;
        SizeF textSize = measureCanvas.GetStringSize(label, font, fontSizePx);
        float labelHeight = textSize.Height + 4;

        // Nowa powierzchnia dla połączonego obrazu
        SKImageInfo info = new(width, height + (int)labelHeight);
        using SKSurface surface = SKSurface.Create(info);
        SkiaCanvas canvas = new() { Canvas = surface.Canvas };
        canvas.SetDisplayScale(1f);

        // Tło
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(0, 0, width, height + labelHeight);

        // Kod QR
        canvas.DrawImage(qrImage, 0, 0, width, height);

        // Rysuj etykietę
        canvas.Font = font;
        canvas.FontSize = fontSizePx;
        canvas.FontColor = Colors.Black;
        RectF rect = new(0, height, width, labelHeight);
        canvas.DrawString(label, rect, HorizontalAlignment.Center, VerticalAlignment.Center);

        // Eksport PNG
        using SKImage snap2 = surface.Snapshot();
        using SKData pngData = snap2.Encode(SKEncodedImageFormat.Png, 100);
        return pngData.ToArray();
    }
}