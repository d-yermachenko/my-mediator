# Code Review: Security and Performance Issues

## Security Issues

---

### SEC-1 · Unsafe reflection-based handler invocation (`MyPublisher.CallService`)

**File:** `DyoMediator.Notifications/MyPublisher.cs`

```csharp
private Task CallService(object? serviceInstance, object notification, CancellationToken cancellationToken)
{
    if(serviceInstance is null)
        return Task.CompletedTask;
    var handleMethod = serviceInstance.GetType().GetMethod(nameof(IMyNotificationHandler<object>.HandleAsync));
    if (handleMethod == null)
    {
        throw new InvalidOperationException($"...");
    }
    var task = (Task)handleMethod.Invoke(serviceInstance, new object[] { notification, cancellationToken })!;
    return task;
}
```

`CallService` accepts a plain `object?` and resolves the method to call solely by name (`"HandleAsync"`).  
There is no verification that the resolved method belongs to an `IMyNotificationHandler<>` implementation.  
Any object that happens to have a public `HandleAsync(object, CancellationToken)` method could be invoked, which breaks the type-safety contract of the notification pipeline.

**Recommendation:** Cast `serviceInstance` to `IMyNotificationHandler<TNotificationBaseType>` (or the concrete closed generic) before invoking. If a cast fails, throw a meaningful `InvalidOperationException` instead of proceeding with uncontrolled reflection.

---

### SEC-2 · `serviceLifetime` parameter silently ignored in `AddDecorator`

**File:** `DyoMediator.Decorators/DIExtensions.cs`

```csharp
public IServiceCollection AddDecorator(Type decoratorType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
{
    bool decoratorSucceed = services.TryDecorate(typeof(IRequestHandler<,>), decoratorType);
    return services;  // serviceLifetime is never passed anywhere
}
```

The public API advertises a `serviceLifetime` parameter, giving callers the expectation that the decorator will be registered with that lifetime.  
`Scrutor.TryDecorate` does not accept a service lifetime; it inherits the lifetime of the decorated registration. The parameter is accepted and silently discarded, which is a broken API contract.

If a caller passes `ServiceLifetime.Singleton` expecting singleton semantics for a stateful decorator, they will silently receive whatever lifetime the original handler was registered with.

**Recommendation:** Either remove the `serviceLifetime` parameter to match what `TryDecorate` actually provides, or document prominently that the lifetime is ignored and the decorator inherits the handler's lifetime.

---

## Performance Issues

---

### PERF-1 · Reflection overhead on every `SendAsync` call (`GetBehaviours`)

**File:** `DyoMediator.Mediator/Mediator/Mediator.cs`

```csharp
private IEnumerable<IRequestBehaviour<TRequest, TResponse>> GetBehaviours<TRequest, TResponse>()
{
    var bahaviourType = typeof(IRequestBehaviour<,>).MakeGenericType(typeof(TRequest), typeof(TResponse)); // note: 'bahaviourType' is also a typo — should be 'behaviourType'
    var behaviours = serviceProvider.GetServices(bahaviourType);
    return behaviours.Cast<IRequestBehaviour<TRequest, TResponse>>();
}
```

`MakeGenericType` performs a reflection-based type construction on **every single call** to `SendAsync`. The result for a given pair of `(TRequest, TResponse)` type arguments is always identical, so this work is redundant on repeated invocations.

**Recommendation:** Cache constructed generic types in a `ConcurrentDictionary<(Type, Type), Type>` keyed by `(typeof(TRequest), typeof(TResponse))`. Alternatively, since the generic method already has compile-time type information, consider resolving `IEnumerable<IRequestBehaviour<TRequest, TResponse>>` directly through the strongly-typed `GetServices<T>` overload, which avoids the `MakeGenericType` call entirely:

```csharp
serviceProvider.GetServices<IRequestBehaviour<TRequest, TResponse>>()
```

---

### PERF-2 · Reflection-based invocation in `CallService` fallback path

**File:** `DyoMediator.Notifications/MyPublisher.cs`

```csharp
var handleMethod = serviceInstance.GetType().GetMethod(nameof(IMyNotificationHandler<object>.HandleAsync));
var task = (Task)handleMethod.Invoke(serviceInstance, new object[] { notification, cancellationToken })!;
```

`Type.GetMethod` and `MethodInfo.Invoke` are significantly slower than direct interface dispatch (~20–100× overhead per call). The fallback path is reached whenever `exactHandlers` and `baseHandlers` both return empty collections. Under high throughput this becomes a bottleneck.

**Recommendation:** Cast `serviceInstance` to a known interface and call it directly. If the type is unknown at compile time, compile and cache a delegate (e.g., via `Expression` trees or `Delegate.CreateDelegate`) so the cost is paid only once per handler type.

---

### PERF-3 · Potential duplicate handler invocation when `TMessageType == TNotificationBaseType`

**File:** `DyoMediator.Notifications/MyPublisher.cs`

```csharp
var exactHandlers = serviceProvider.GetServices<IMyNotificationHandler<TMessageType>>();
var baseHandlers  = serviceProvider.GetServices<IMyNotificationHandler<TNotificationBaseType>>();

tasks.AddRange(exactHandlers.Select(h => h.HandleAsync(notification, cancellationToken)));
tasks.AddRange(baseHandlers.Select(h => h.HandleAsync((TNotificationBaseType)notification, cancellationToken)));
```

When `TMessageType` and `TNotificationBaseType` are the same type (which is valid — the publisher is typed to its own base type), the DI container returns the same set of handlers for both calls. Every registered handler is then invoked **twice**.

**Recommendation:** Add a type-equality guard before appending `baseHandlers`:

```csharp
if (typeof(TMessageType) != typeof(TNotificationBaseType))
    tasks.AddRange(baseHandlers.Select(h => h.HandleAsync((TNotificationBaseType)notification, cancellationToken)));
```

---

### PERF-4 · Unused variable `requestType` computed on every call

**File:** `DyoMediator.Mediator/Mediator/Mediator.cs`

```csharp
public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
    where TRequest : IRequest
{
    var requestType = request.GetType();   // computed but never referenced
    var handler = serviceProvider.GetService(typeof(IRequestHandler<>).MakeGenericType(typeof(TRequest))) as IRequestHandler<TRequest>;
    ...
}
```

`request.GetType()` is called and the result stored in `requestType`, but `requestType` is never used. The runtime cost is negligible, but this is dead code that pollutes the method and may indicate an incomplete refactoring.

**Recommendation:** Remove the unused variable.

---

### PERF-5 · Logic error in `HasIRequestHandlerInConstructorArguments` causes incorrect assembly scanning

**File:** `DyoMediator.Mediator/DIExtension/MyMediatorDIExtensions.cs`

```csharp
private static bool HasIRequestHandlerInConstructorArguments(Type type)
{
    return type
        .GetConstructors()
        .All(ctor => ctor.GetParameters().Select(x => x.ParameterType)
            .Any(par =>
                (par.IsGenericType && par.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
             || par.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))));
}
```

The method uses `.All(ctor => ...)` which returns `true` only when **every** constructor has an `IRequestHandler` parameter. The intended purpose is to detect decorator types (which wrap a handler) so they can be excluded from `AddHandlersFromAssembly`. With `.All()`, a type that has two constructors — one with an `IRequestHandler` dependency and one without — is **not** identified as a decorator and gets registered incorrectly.

**Recommendation:** Change `.All(...)` to `.Any(...)` so that a type is treated as a decorator (and skipped) as soon as any of its constructors accepts an `IRequestHandler` parameter.

---

### PERF-6 · Dead code: `HasIRequestHandlerInConstructorArgumentsDetailed` is never called

**File:** `DyoMediator.Mediator/DIExtension/MyMediatorDIExtensions.cs`

`HasIRequestHandlerInConstructorArgumentsDetailed` is a private method that reimplements the same logic as `HasIRequestHandlerInConstructorArguments` using imperative loops instead of LINQ. It is never invoked anywhere in the project.

**Recommendation:** Remove the method. If the intent was to replace the LINQ-based version with a more readable imperative form, complete the replacement (and fix the `.All()`→`.Any()` bug from PERF-5 at the same time) and delete the superseded version.

---

## Code Quality Issues

---

### QA-1 · Unused import of `System.Security.Cryptography.X509Certificates`

**File:** `DyoMediator.Mediator/DIExtension/MyMediatorDIExtensions.cs`

```csharp
using System.Security.Cryptography.X509Certificates;
```

This namespace is imported but never used anywhere in the file. It adds noise, can confuse reviewers, and may trigger warnings in static-analysis tools.

**Recommendation:** Remove the unused `using` directive.

---

## Summary Table

| ID | Category | Severity | File | Short Description |
|----|----------|----------|------|-------------------|
| SEC-1 | Security | High | `MyPublisher.cs` | Untyped reflection invocation in `CallService` |
| SEC-2 | Security | Medium | `DIExtensions.cs` (Decorators) | `serviceLifetime` parameter silently ignored in `AddDecorator` |
| PERF-1 | Performance | High | `Mediator.cs` | `MakeGenericType` called on every `SendAsync` invocation |
| PERF-2 | Performance | High | `MyPublisher.cs` | Reflection `GetMethod`/`Invoke` in notification fallback path |
| PERF-3 | Performance | Medium | `MyPublisher.cs` | Duplicate handler invocation when `TMessageType == TNotificationBaseType` |
| PERF-4 | Performance | Low | `Mediator.cs` | Unused variable `requestType` computed on every call |
| PERF-5 | Performance/Bug | Medium | `MyMediatorDIExtensions.cs` | `.All()` should be `.Any()` in `HasIRequestHandlerInConstructorArguments` |
| PERF-6 | Performance | Low | `MyMediatorDIExtensions.cs` | Dead-code method `HasIRequestHandlerInConstructorArgumentsDetailed` |
| QA-1 | Code Quality | Low | `MyMediatorDIExtensions.cs` | Unused `System.Security.Cryptography.X509Certificates` import |
