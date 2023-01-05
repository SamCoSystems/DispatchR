namespace DispatchR;

public interface INotificationHandler<in T>
{
	void Handle(T notification);
}
