using ControleDeLeiloes.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ControleDeLeiloes.Configuration
{
    public static class DbContextConfig
    {
        public static IServiceCollection AddDbContextConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ControleDeLeiloesDbContext>(options =>
                options.UseMySql(configuration.GetConnectionString("ControleDeLeiloesDbContext"), builder =>
                builder.MigrationsAssembly("ControleDeLeiloes")));
            return services;
        }
    }
}