using DispatchR;

namespace BlazorStarterApp.Counters;

public static class Count
{
	public record Request;
	public record Result(int Currentcount);

	public class Handler : INotificationHandler<Request>
	{
		private readonly IDispatcher _dispatcher;
		private int _count = 0;

		public Handler(IDispatcher dispatcher)
			=> _dispatcher = dispatcher;

		public void Handle(Request notification)
			=> _dispatcher.Dispatch(new Result(++_count));
	}
}
