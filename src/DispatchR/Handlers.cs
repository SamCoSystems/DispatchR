using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace DispatchR;

internal class Handlers
{
	private readonly InstanceHandlers _instanceHandlers = new();
	private readonly InstanceHandlers _asyncInstanceHandlers = new();
	private readonly ServiceHandlers _serviceHandlers = new();
	private readonly ServiceHandlers _asyncServiceHandlers = new();
	private readonly DispatcherLog _log;

	public Handlers(
		ILogger? logger)
		=> _log = new DispatcherLog(logger);

	public void Register(Type handlerType, Func<object?> factory)
	{
		var safeFactory = () => _instantiate(handlerType, factory);
		foreach (var notificationType in GetNotificationTypesHandledBy(handlerType))
		{
			_log.Debug.RegisterServiceHandler(handlerType, notificationType);
			_serviceHandlers.Add(notificationType, safeFactory);
		}
		foreach (var asyncNotificationType in GetAsyncNotificationTypesHandledBy(handlerType))
		{
			_log.Debug.RegisterAsyncServiceHandler(handlerType, asyncNotificationType);
			_asyncServiceHandlers.Add(asyncNotificationType, safeFactory);
		}
	}

	public void Register(object handler)
	{
		Type handlerType = handler.GetType();
		foreach (var notificationType in GetNotificationTypesHandledBy(handlerType))
		{
			_log.Debug.RegisterInstanceHandler(handlerType, notificationType);
			_instanceHandlers.Add(notificationType, handler);
		}
		foreach (var asyncNotificationType in GetAsyncNotificationTypesHandledBy(handlerType))
		{
			_log.Debug.RegisterAsyncInstanceHandler(handlerType, asyncNotificationType);
			_asyncInstanceHandlers.Add(asyncNotificationType, handler);
		}
	}

	public void Unregister(object handler)
	{
		Type handlerType = handler.GetType();
		foreach (var notificationType in GetNotificationTypesHandledBy(handlerType))
		{
			_log.Debug.UnregisterInstanceHandler(handlerType, notificationType);
			_instanceHandlers.Remove(notificationType, handler);
		}
		foreach (var asyncNotificationType in GetAsyncNotificationTypesHandledBy(handlerType))
		{
			_log.Debug.UnregisterAsyncInstanceHandler(handlerType, asyncNotificationType);
			_asyncInstanceHandlers.Remove(asyncNotificationType, handler);
		}
	}

	public HandlersForDispatch GetHandlers(Type notificationType)
	{
		var handlers = _instanceHandlers.GetHandlers(notificationType)
			.Concat(_serviceHandlers.GetHandlers(notificationType))
			.ToImmutableList();
		var asyncHandlers = _asyncInstanceHandlers.GetHandlers(notificationType)
			.Concat(_asyncServiceHandlers.GetHandlers(notificationType))
			.ToImmutableList();

		return new HandlersForDispatch(handlers, asyncHandlers);
	}

	internal static IEnumerable<Type> GetNotificationTypesHandledBy(Type handlerType)
		=> GetNotificationTypesHandledBy(handlerType, typeof(INotificationHandler<>));

	internal static IEnumerable<Type> GetAsyncNotificationTypesHandledBy(Type type)
		=> GetNotificationTypesHandledBy(type, typeof(IAsyncNotificationHandler<>));

	private static object _instantiate(Type handlerType, Func<object?> factory)
		=> factory() ?? new InstantiationFailure(handlerType);

	private static IEnumerable<Type> GetNotificationTypesHandledBy(
		Type handlerType,
		Type syncOrAsync) =>
		handlerType.GetInterfaces()
			.Where(interfaceType =>
				interfaceType.IsGenericType
				&& interfaceType.GetGenericTypeDefinition() == syncOrAsync)
			.Select(interfaceType => interfaceType.GetGenericArguments()[0]);
}

internal class InstanceHandlers
{
	private ImmutableDictionary<Type, ImmutableList<object>> _handlers = 
		ImmutableDictionary<Type, ImmutableList<object>>.Empty;

	public IEnumerable<object> GetHandlers(Type notificationType)
		=> _handlers.ContainsKey(notificationType)
		? _handlers[notificationType]
		: Enumerable.Empty<object>();

	public void Add(Type notificationType, object handler)
	{
		var currentList = _handlers.ContainsKey(notificationType)
			? _handlers[notificationType]
			: ImmutableList<object>.Empty;

		var newList = currentList.Add(handler);

		_handlers = _handlers.SetItem(notificationType, newList);
	}

	public void Remove(Type notificationType, object handler)
	{
		if (_handlers.ContainsKey(notificationType))
		{
			_handlers.SetItem(
				notificationType,
				_handlers[notificationType].Remove(handler));
		}
	}
}

internal class ServiceHandlers
{
	private ImmutableDictionary<Type, ImmutableList<Func<object>>> _factories =
		ImmutableDictionary<Type, ImmutableList<Func<object>>>.Empty;

	public IEnumerable<object> GetHandlers(Type notificationType)
		=> _factories.ContainsKey(notificationType)
		? _factories[notificationType].Select(f => f())
		: Enumerable.Empty<object>();

	public void Add(Type notificationType, Func<object> factory)
	{
		var currentList = _factories.ContainsKey(notificationType)
			? _factories[notificationType]
			: ImmutableList<Func<object>>.Empty;

		var newList = currentList.Add(factory);
		
		_factories = _factories.SetItem(notificationType, newList);
	}
}

internal record HandlersForDispatch(
	ImmutableList<object> Handlers,
	ImmutableList<object> AsyncHandlers);