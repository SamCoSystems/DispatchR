# DisptatchR

DispatchR is an implementation of the [Event Aggregator pattern](https://www.codeproject.com/Articles/812461/Event-Aggregator-Pattern) 
pattern, custom fit to Server-Side Blazor applications

DispatchR aims to solve the problem of state management in Blazor apps by allowing components to 
manage (and mutate) their own local state, communicating with the rest of the application via the publishing of events.

Blazor apps built with DispatchR aim to achieve a similar architecture to event driven services in a
distributed system. A component may dispatch an event indicating that it is loading, to which a service handler
may subscribe, which then executes the work of loading page data, and publishes the success (or failure) of this activity.
The component, likely subscribed to a successful load of it's page state, would render the data out to the user.

Components may also dispatch user events, which may cause a service handler to execute some process, and dispatch an event
indicating the outcome of such process.

## Dispatcher Components

In a DispatchR app, if a component is going to participate in the publishing and subscribing of events, it must extend 
DispatcherComponent. This gives the component access to the IDispatcher interface via the Dispatcher property, and automatically
registers the component to receive any notification for which it implements the INotificationHandler or IAsyncNotificationHandler
interfaces. The component will also be unregistered on Dispose.

DisptacherComponents are primarily used to dispatch user events to service handlers, or render updated data, either through
their own view, or by passing parameters to child components.

By default, DispatcherComponents will have 'StateHasChanged' called after a notification has been handled.

```csharp
public class NewTodoComponent : DispatcherComponent, INotificationHandler<CreateTodo.Success>
{
  public void OnSubmit() // User clicks the submit button
  {
    if (TodoForm.IsValid)
    {
      ShowWorkingAnimation = true;
      var notification = new CreateTodo.Request(TodoForm.Value);
      Dispatcher.Dispatch(notification);
    }
  }

  public void Handle(CreateTodo.Success notification)
  {
    ShowWorkingAnimation = false;
    Growl.Show("Todo has been created!");
  }
}
```

## ServiceHandlers

For the execution of 'backend' work, Dispatcher can be configured with 'Service Handlers' or POCOs which implement one
or more INotificationHandler or IAsyncNotificationHandler interfaces. The creation and lifetime of these handlers is
delegated to the IoC container (currently only ServiceProvider), but an instance of the handler is called whenever a
notification is dispatched, to which it handles.

```csharp
public class CreateTodoHandler : IAsyncNotificationHandler<CreateTodo.Request>
{
  private readonly IDispatcher _dispatcher;
  private readonly IDocumentStore _store;

  public CreateTodoHandler(
    IDispatcher dispatcher,
    IDocumentStore store)
    => (_dispatcher, _store) = (dispatcher, store);

  public async Task HandleAsync(CreateTodo.Request request)
  {
    object notification;
    try
    {
      using var session = _store.OpenAsyncSession();
      var newTodo = request.Todo;
      session.Store(newTodo);
      await session.SaveChangesAsync();
      notification = new CreateTodo.Success(new Todo.Id);
    }
    catch (Exception ex)
    {
      notification = new CreateTodo.Failure(ex.Message);
    }
    _dispatcher.Disptach(notification);
  }
}
```

## Polymorphic Dispatch

DispatchR will respect polymorphism when dispatch notifications. For any notification of type A which extends type B and
implements interfaces C and D, DisptachR will call the appropriate handler methods for any DispatcherComponent or service handler
which implements a handler interface for any combination of A, B, C, and D.

If an object implements multiple of these interfaces, it will be called multiple times.