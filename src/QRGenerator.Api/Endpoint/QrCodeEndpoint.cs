using Microsoft.AspNetCore.Mvc;
using QRCodeService;

namespace QRGenerator.Api.Endpoint;

public class QrCodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {

        app.MapGet(
                "/QR/{text}",
                ([FromRoute] string text, IQRCodeService qrService) =>
                {
                    var qrCode = qrService.GenerateQRCodeByte(text);
                    return Results.File(qrCode, "image/png");
                }
            )
            .WithName("QR")
            .WithOpenApi();


        app.MapGet(
                "/QR/Download/{text}",
                ([FromRoute] string text, IQRCodeService qrService) =>
                {
                    var qrCode = qrService.GenerateQRCodeByte(text);

                    return Results.File(
                        qrCode,
                        "application/image",
                        $"QRCode-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png"
                    );
                }
            )
            .WithName("QRsFiles")
            .WithOpenApi();

        app.MapPost(
                "/QR/DownloadZip",
                async ([FromBody] List<string> texts, IQRCodeService qrService) =>
                {
                    var files = await qrService.GenerateZip(texts);

                    return Results.File(
                        files,
                       "application/zip",
                        $"QRCode-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip"
                    );
                }
            )
            .WithName("QRDownloadZip")
            .WithOpenApi();

    }
}
