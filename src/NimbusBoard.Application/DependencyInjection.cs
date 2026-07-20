using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NimbusBoard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNimbusBoardApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
