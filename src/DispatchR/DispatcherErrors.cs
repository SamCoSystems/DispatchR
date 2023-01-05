using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DispatchR;

internal class DispatcherErrors
{
	private readonly LogAction _log;

	public DispatcherErrors(LogAction log)
		=> _log = log;

	public void InsantiationFailure(InstantiationFailure failure)
		=> _log("Unable to instantiate handler: {handlerType}", failure.HandlerType.FullName);

	internal void MethodNotFound(Type handlerType, Type expectedHandlerType)
		=> _log("{handlerType} is assignable to {expectedHandlerType}, but no suitable handle method found.",
				handlerType.FullName,
				expectedHandlerType.FullName);

	internal void CannotBeAssigned(object handler, Type dispatchAs)
	 => _log("{handlerType} was returned as handler for {notificationType}, but is not assignable to INotificationHandler<{notificationType}>",
				handler.GetType(),
				dispatchAs.FullName,
				dispatchAs.Name);

	internal void HandlerIsNotAsync(Type handlerType, MethodInfo handleMethod)
	 => _log("{handlerType} HandleAsync does not return Task, instead it returns {returnType}",
				handlerType.FullName,
				handleMethod.ReturnType.FullName);
}
