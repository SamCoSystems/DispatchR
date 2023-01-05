namespace DispatchR;

public interface IAsyncNotificationHandler<in T>
{
	Task HandleAsync(T notification);
}
