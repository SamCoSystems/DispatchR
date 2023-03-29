namespace DispatchR;

public interface IDispatcher
{
	void Dispatch(object notification);

	IAdvancedDispatcher Advanced { get; }
}
