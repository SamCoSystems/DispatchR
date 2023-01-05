using Microsoft.Extensions.Logging;

namespace DispatchR;

internal class DispatcherDebug
{
	private LogAction _log;

	public DispatcherDebug(LogAction log)
		=> _log = log;

	internal void Dispatching(object notification)
		=> _log("Dispatching {notificationType}", notification.GetType().FullName);

	internal void Dispatched(object notification)
	 => _log("Dispatched {notificationType}", notification.GetType().FullName);

	internal void AsyncHandlersCompleted(object notification)
	 => _log("All async handlers for {notificationType} completed.", notification.GetType().FullName);

	internal void Dispatching(Type dispatchAs, object handler)
	 => _log("Dispatching {notificationType} to {handlerType}", dispatchAs.FullName, handler.GetType().FullName);

	internal void Dispatched(Type dispatchAs, object handler)
	 => _log("Dispatching {notificationType} to {handlerType}", handler.GetType().FullName, dispatchAs.FullName);

	internal void AsyncHandlerFinished(Type dispatchAs, object handler)
		=> _log("async handler {handlerType} finished handling {notificationType}", handler.GetType().FullName, dispatchAs.FullName);

	internal void RegisterServiceHandler(Type handlerType, Type notificationType)
		=> _log("Registering service handler {handlerType} for {notificationType}", handlerType.FullName, notificationType.FullName);

	internal void RegisterAsyncServiceHandler(Type handlerType, Type asyncNotificationType)
		=> _log("Registering async service handler {handlerType} for {notificationType}", handlerType.FullName, asyncNotificationType.FullName);

	internal void RegisterInstanceHandler(Type handlerType, Type notificationType)
		=> _log("Registering instance handler {handlerType} for {notificationType}", handlerType.FullName, notificationType.FullName);

	internal void RegisterAsyncInstanceHandler(Type handlerType, Type asyncNotificationType)
		=> _log("Registering async instance handler {handlerType} for {notificationType}", handlerType.FullName, asyncNotificationType.FullName);

	internal void UnregisterInstanceHandler(Type handlerType, Type notificationType)
		=> _log("Unregistering instance handler {handlerType} for {notificationType}", handlerType.FullName, notificationType.FullName);

	internal void UnregisterAsyncInstanceHandler(Type handlerType, Type asyncNotificationType)
		=> _log("Unregistering async instance handler {handlerType} for {notificationType}", handlerType.FullName, asyncNotificationType.FullName);
}
