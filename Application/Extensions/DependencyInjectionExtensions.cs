using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Extensions
{
	public static class DependencyInjectionExtensions
	{
		/// <summary>
		/// Quét một assembly và đăng ký các dịch vụ theo quy ước đặt tên I{ClassName} -> {ClassName}.
		/// </summary>
		/// <param name="services">IServiceCollection để thêm dịch vụ vào.</param>
		/// <param name="assembly">Assembly cần quét.</param>
		/// <param name="lifetime">Vòng đời của dịch vụ (mặc định là Scoped).</param>
		/// <returns>IServiceCollection để có thể gọi chuỗi (fluent chaining).</returns>
		public static IServiceCollection RegisterServicesByConvention(
			this IServiceCollection services,
			Assembly assembly,
			ServiceLifetime lifetime = ServiceLifetime.Scoped)
		{
			var types = assembly.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && t.IsPublic);

			foreach (var implementationType in types)
			{
				// Tìm một interface khớp với quy ước I{TypeName}
				var interfaceType = implementationType.GetInterfaces()
					.FirstOrDefault(i => i.Name == $"I{implementationType.Name}");

				if (interfaceType != null)
				{
					// Đăng ký cặp interface-implementation với vòng đời đã chỉ định
					services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
				}
			}

			return services;
		}
	}
}
