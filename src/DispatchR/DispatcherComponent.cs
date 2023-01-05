using Microsoft.AspNetCore.Components;

namespace DispatchR;

public abstract class DispatcherComponent : ComponentBase, IDisposable
{
	[Inject]
	public IDispatcher Dispatcher { get; set; } = null!;

	public bool StateChangeOnHandle { get; protected set; } = true;

	private readonly bool _isHandler;

	protected DispatcherComponent()
	{
		var myType = GetType();
		_isHandler = Handlers.GetNotificationTypesHandledBy(myType).Any()
			|| Handlers.GetAsyncNotificationTypesHandledBy(myType).Any();
	}

	protected sealed override void OnInitialized()
	{
		if (_isHandler)
		{
			Dispatcher.Register(this);
		}
		OnInitializedAndRegistered();
	}

	protected virtual void OnInitializedAndRegistered() { }

	internal void StateChange()
	{
		InvokeAsync(StateHasChanged);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		if (_isHandler)
		{
			Dispatcher.Unregister(this);
		}

	}
}
