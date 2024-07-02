using Microsoft.Extensions.DependencyInjection;

namespace QRCodeService;

public static class ServiceExtension
{
    public static IServiceCollection AddQRCodeService(this IServiceCollection services)
    {
        services.AddTransient<IQRCodeService, QRCodeService>();
        return services;
    }
}
