using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using QRCoder;
using SkiaSharp;

namespace QRCodeService;

public class QRCodeService(ILogger<QRCodeService> logger) : IQRCodeService
{
    private readonly ILogger<QRCodeService> _logger = logger;

    public byte[] GenerateQRCodeByte(string inputText)
    {
        try
        {
            using MemoryStream memoryStream = new();
            QRCodeGenerator qrGenerator = new();

            if (
                Uri.TryCreate(
                    Uri.UnescapeDataString(inputText),
                    new UriCreationOptions(),
                    out Uri? result
                )
            )
            {
                inputText = result.AbsoluteUri;
            }

            QRCodeData qrCodeData = qrGenerator.CreateQrCode(inputText, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode imageByte = new(qrCodeData);

            return imageByte.GetGraphic(50);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred at {nameof(GenerateQRCodeByte)}");
            return GenerateDefaultImage(ex.Message);
        }
    }

    public async Task<byte[]> GenerateZip(List<string> inputs)
    {
        try
        {
            List<byte[]> images = [];

            foreach (var inputText in inputs)
            {
                images.Add(GenerateQRCodeByte(inputText));
            }

            var botFilePaths = Directory.GetFiles(Path.GetTempPath());
            using var zipFileMemoryStream = new MemoryStream();
            using (
                ZipArchive archive =
                    new(zipFileMemoryStream, ZipArchiveMode.Update, leaveOpen: true)
            )
            {
                List<string> files = [];

                for (int i = images.Count - 1; i >= 0; i--)
                {
                    using MemoryStream stream = new();
                    stream.Write(images[i], 0, images[i].Length);
                    var image2 = Image.FromStream(stream);
                    var filePath = Path.GetTempPath() + $"\\QRCode_{i}.png";
                    files.Add(filePath);
                    image2.Save(filePath, ImageFormat.Png);
                }

                foreach (var imagePath in files)
                {
                    // create destination file name
                    var botFileName = Path.GetFileName(imagePath);
                    var entry = archive.CreateEntry(botFileName);
                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(imagePath);
                    await fileStream.CopyToAsync(entryStream);
                }

                foreach (var item in files)
                {
                    File.Delete(item);
                }
            }

            zipFileMemoryStream.Seek(0, SeekOrigin.Begin);

            return zipFileMemoryStream.ToArray();
            // use stream as needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred at {nameof(GenerateQRCodeByte)}");
            throw;
        }
    }

    private byte[] GenerateDefaultImage(string error)
    {
        // crate a surface
        var info = new SKImageInfo(512, 512);
        using var surface = SKSurface.Create(info);
        // the the canvas and properties
        var canvas = surface.Canvas;

        // make sure the canvas is blank
        canvas.Clear(SKColors.White);

        // draw some text
        var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Center,
            TextSize = 24
        };
        var coord = new SKPoint(info.Width / 2, (info.Height + paint.TextSize) / 2);

        canvas.DrawText(error, coord, paint);

        // save the file
        var image = surface.Snapshot();
        var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }
}
