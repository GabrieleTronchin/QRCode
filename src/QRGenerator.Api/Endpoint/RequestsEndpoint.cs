using Microsoft.AspNetCore.Mvc;
using QRCodeService;

namespace QRGenerator.Api.Endpoint;

public class RequestsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/Equipment/QR/Download/{id}",
                async ([FromRoute] string id, IQRCodeService qrService) =>
                {
                    var qrCode = qrService.GenerateQRCodeByte(id);

                    return Results.File(
                        qrCode,
                        "application/image",
                        $"QRCode-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png"
                    );
                }
            )
            .WithName("EquipmentQRsFiles")
            .WithOpenApi();

        app.MapGet(
                "/Equipment/QR/{id}",
                async ([FromRoute] string id, IQRCodeService qrService) =>
                {
                    var qrCode = qrService.GenerateQRCodeByte(id);
                    return Results.File(qrCode, "image/png");
                }
            )
            .WithName("Equipment/QR")
            .WithOpenApi();
    }
}
