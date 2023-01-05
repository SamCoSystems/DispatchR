using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DispatchR.Tests
{
	public class HandlersShould
	{
		class Handler1 :
			INotificationHandler<string>,
			IAsyncNotificationHandler<int>
		{
			public void Handle(string notification) { }

			public Task HandleAsync(int notification) => Task.CompletedTask;
		}

		[Fact]
		public void ShouldCorrectlyIdentifyHandlerTypes()
		{
			var handlers = Handlers.GetNotificationTypesHandledBy(typeof(Handler1));
			handlers.Count().ShouldBe(1);
			handlers.First().ShouldBe(typeof(string));
		}

		[Fact]
		public void ShouldCorrectlyIdentifyAsyncHandlerTypes()
		{
			var handlers = Handlers.GetAsyncNotificationTypesHandledBy(typeof(Handler1));
			handlers.Count().ShouldBe(1);
			handlers.First().ShouldBe(typeof(int));
		}
	}
}