using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace DispatchR;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDispatchR(
		this IServiceCollection services,
		Action<DispatcherConfig> configure)
	{
		DispatcherConfig config = new(services);
		configure(config);

		services.AddScoped<IDispatcher>(provider =>
		{
			Dispatcher dispatcher = new(provider.GetRequiredService);
			config.RegisterServiceHandlers(dispatcher);
			return dispatcher;
		});

		return services;
	}
}
