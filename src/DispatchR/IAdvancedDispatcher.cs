
namespace DispatchR;

public interface IAdvancedDispatcher
{
	Task DispatchAndWait(object notification);
}
