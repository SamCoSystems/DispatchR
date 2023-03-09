using Microsoft.Extensions.DependencyInjection;

namespace DispatchR;

public class DispatcherConfig
{
	private readonly IServiceCollection _services;
	private readonly List<Type> _typesToRegister = new();

	public DispatcherConfig(IServiceCollection services)
		=> _services = services;

	public void RegisterTransient<T>()
		where T : class
	{
		_services.AddTransient<T>();
		_typesToRegister.Add(typeof(T));
	}

	public void RegisterTransient<T>(
		Func<IServiceProvider, T> implementationFactory)
		where T : class
	{
		_services.AddTransient(implementationFactory);
		_typesToRegister.Add(typeof(T));
	}

	public void RegisterScoped<T>()
		where T : class
	{
		_services.AddScoped<T>();
		_typesToRegister.Add(typeof(T));
	}

	public void RegisterScoped<T>(
		Func<IServiceProvider, T> implementationFactory)
		where T : class
	{
		_services.AddScoped(implementationFactory);
		_typesToRegister.Add(typeof(T));
	}

	public void RegisterSingleton<T>()
		where T : class
	{
		_services.AddSingleton<T>();
		_typesToRegister.Add(typeof(T));
	}

	public void RegisterSingleton<T>(
		Func<IServiceProvider, T> implementationFactory)
		where T : class
	{
		_services.AddSingleton(implementationFactory);
		_typesToRegister.Add(typeof(T));
	}

	public void RegisterSingleton<T>(
		T implementationInstance)
		where T : class
	{
		_services.AddSingleton(implementationInstance);
		_typesToRegister.Add(typeof(T));
	}

	internal void RegisterServiceHandlers(Dispatcher dispatcher)
		=> _typesToRegister.ForEach(dispatcher.Register);

}