using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DispatchR;

internal sealed class Dispatcher : IDispatcher, IAdvancedDispatcher
{
	private readonly Func<Type, object> _getService;
	private readonly Handlers _handlers;
	private readonly DispatcherLog _log;

	public IAdvancedDispatcher Advanced => this;

	public Dispatcher(Func<Type, object> getService)
	{
		_getService = getService;
		var logger = _getService(typeof(ILogger<Dispatcher>)) as ILogger;
		_log = new DispatcherLog(logger);

		var handlersLogger = _getService(typeof(ILogger<Handlers>)) as ILogger<Handlers>;
		_handlers = new Handlers(handlersLogger);
	}

	internal void Register(Type handlerType)
	{
		var factory = () => _getService(handlerType);
		_handlers.Register(handlerType, factory);
	}

	public void Register(object handler)
		=> _handlers.Register(handler);

	public void Unregister(object handler)
		=> _handlers.Unregister(handler);

	public void Dispatch(object notification)
	{
		_ = DispatchAndWait(notification);
	}

	public async Task DispatchAndWait(object notification)
	{
		List<Task> dispatchTasks = new();
		_log.Debug.Dispatching(notification);
		dispatchTasks.AddRange(DispatchUpBaseTypes(notification));
		dispatchTasks.AddRange(DispatchAcrossInterfaces(notification));
		_log.Debug.Dispatched(notification);
		await Task.WhenAll(dispatchTasks);
		_log.Debug.AsyncHandlersCompleted(notification);
	}

	private List<Task> DispatchUpBaseTypes(object notification)
	{
		List<Task> dispatchTasks = new();
		Type? asType = notification.GetType();
		while(asType is not null)
		{
			dispatchTasks.Add(DispatchAndWait(notification, asType));
			asType = asType.BaseType;
		}
		return dispatchTasks;
	}

	private List<Task> DispatchAcrossInterfaces(object notification)
	{
		List<Type> interfaces = notification.GetType().GetInterfaces().ToList();
		return interfaces.ConvertAll(interfaceType =>
			DispatchAndWait(notification, interfaceType));
	}

	private Task DispatchAndWait(object notification, Type asType)
	{
		var typed = _handlers.GetHandlers(asType);
		typed.Handlers.ForEach(handler =>
			DispatchIfHandler(notification, handler, asType));

		var asyncDispatches = typed.AsyncHandlers.Select(asyncHandler =>
			DispatchIfAsyncHandler(notification, asyncHandler, asType));

		return Task.WhenAll(asyncDispatches);
	}

	private void DispatchIfHandler(object notification, object handler, Type dispatchAs)
	{
		if (InstantiationFailedFor(handler)) return;
		
		var expectedHandlerType = typeof(INotificationHandler<>).MakeGenericType(dispatchAs);
		if (CannotBeAssigned(handler, expectedHandlerType, dispatchAs)) return;

		var handleMethod = expectedHandlerType.GetMethod("Handle", new[] { dispatchAs });
		if (MethodNotFound(handleMethod, handler.GetType(), expectedHandlerType)) return;

		InvokeHandler(handler, handleMethod, notification, dispatchAs);

		StateChangeIfComponent(handler);
	}

	private async Task DispatchIfAsyncHandler(object notification, object handler, Type dispatchAs)
	{
		if (InstantiationFailedFor(handler)) return;

		var expectedHandlerType = typeof(IAsyncNotificationHandler<>).MakeGenericType(dispatchAs);
		if (CannotBeAssigned(handler, expectedHandlerType, dispatchAs)) return;

		var handleMethod = expectedHandlerType.GetMethod("HandleAsync", new[] { dispatchAs });
		if (MethodNotFound(handleMethod, handler.GetType(), expectedHandlerType)) return;

		if (IsNotAsync(handleMethod, handler.GetType())) return;

		await InvokeAsyncHandler(handler, handleMethod, notification, dispatchAs);

		StateChangeIfComponent(handler);
	}

	private void InvokeHandler(
		object handler,
		MethodInfo handleMethod,
		object notification,
		Type dispatchAs)
	{
		_log.Debug.Dispatching(dispatchAs, notification);
		handleMethod.Invoke(handler, new object[] { notification });
		_log.Debug.Dispatched(dispatchAs, notification);
	}

	private async Task InvokeAsyncHandler(
		object handler,
		MethodInfo handleMethod,
		object notification,
		Type dispatchAs)
	{
		_log.Debug.Dispatching(dispatchAs, handler);
		var task = (Task)handleMethod.Invoke(handler, new object[] { notification })!;
		_log.Debug.Dispatched(dispatchAs, handler);
		await task;
		_log.Debug.AsyncHandlerFinished(dispatchAs, handler);
	}

	private bool InstantiationFailedFor(object handler)
	{
		if (handler is InstantiationFailure failure)
		{
			_log.Errors.InsantiationFailure(failure);
			return true;
		}

		return false;
	}

	private bool MethodNotFound(
		[NotNullWhen(false)] MethodInfo? handleMethod,
		Type handlerType,
		Type expectedHandlerType)
	{
		if (handleMethod is null)
		{
			_log.Errors.MethodNotFound(handlerType, expectedHandlerType);
			return true;
		}
		return false;
	}

	private bool CannotBeAssigned(object handler, Type expectedHandlerType, Type dispatchAs)
	{
		if (expectedHandlerType.IsAssignableFrom(handler.GetType()) is false)
		{
			_log.Errors.CannotBeAssigned(handler, dispatchAs);
			return true;
		}

		return false;
	}

	private bool IsNotAsync(MethodInfo handleMethod, Type handlerType)
	{
		if (handleMethod.ReturnType != typeof(Task))
		{
			_log.Errors.HandlerIsNotAsync(handlerType, handleMethod);
			return true;
		}

		return false;
	}

	private static void StateChangeIfComponent(object handler)
	{
		if (handler is DispatcherComponent component
			&& component.StateChangeOnHandle)
		{
			component.StateChange();
		}
	}
}
