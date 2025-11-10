using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
	public static class ApplicationDI
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			var assembly = typeof(ApplicationDI).Assembly;

			services.RegisterServicesByConvention(assembly);

			// Mapster
			var config = TypeAdapterConfig.GlobalSettings.Clone();
			config.Scan(assembly);
			services.AddSingleton(config);
			services.AddScoped<IMapper, ServiceMapper>();

			// FluentValidation
			services.AddValidatorsFromAssembly(assembly);

			return services;
		}
	}
}
