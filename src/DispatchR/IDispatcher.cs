namespace DispatchR;

public interface IDispatcher
{
	void Register(object handler);
	void Unregister(object handler);
	void Dispatch(object notification);

	IAdvancedDispatcher Advanced { get; }
}
