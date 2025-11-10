using Application.Extensions;
using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.DBContext;
using Persistence.Repositories;

namespace Infrastructure.Extensions
{
	public static class InfrastructureDI
	{
		public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
		{
			var persistenceAssembly = typeof(GenericRepository<>).Assembly;
			services.RegisterServicesByConvention(persistenceAssembly);
			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

			// DBContext
			services.AddDbContext<NewsManagementDBContext>(
				   options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

			// CORS
			var webUrl = configuration["Front-end:Url"] ?? throw new Exception("Missing url!!");
			services.AddCors(options =>
			{
				options.AddPolicy("AllowFrontend", builder =>
				{
					builder
						.WithOrigins(webUrl)
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials();
				});
			});

			return services;
		}
	}
}
