
namespace DispatchR;

public interface IAdvancedDispatcher
{
	Task DispatchAndWait(object notification);
	void Register(object handler);
	void Unregister(object handler);
}
