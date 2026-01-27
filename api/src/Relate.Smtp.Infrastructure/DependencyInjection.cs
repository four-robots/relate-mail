using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Relate.Smtp.Core.Interfaces;
using Relate.Smtp.Infrastructure.Data;
using Relate.Smtp.Infrastructure.Repositories;

namespace Relate.Smtp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
