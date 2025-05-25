using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DyoMediator.Abstraction;

namespace DyoMediator.Tests.Abstractions.MockImplementation;

#region External Service
public interface ISomeExternalService
{
    public void SetValue(string key, string value);

    public string GetValue(string key);
}

public class SomeExternalService : ISomeExternalService
{

    readonly Dictionary<string, string> _storage = new();
    public string GetValue(string key)
    {
        return _storage.TryGetValue(key, out var value) ? value : String.Empty;
    }

    public void SetValue(string key, string value)
    {
        if(_storage.ContainsKey(key)) 
            _storage[key] = value;
        else
            _storage.Add(key, value);
    }
}

#endregion
public interface IStorageInteractor
{
    string Key { get; }
}

#region Foo command
public record FooWritingCommand(string Value) : IRequest, IStorageInteractor
{
    public string Key => nameof(FooWritingCommand);
}

public class FooWritingCommandHandler(ISomeExternalService externalService) : IRequestHandler<FooWritingCommand>
{
    public Task HandleAsync(FooWritingCommand request, CancellationToken cancellationToken = default)
    {
        externalService.SetValue(request.Key+"-Handler", request.Value);
        return Task.CompletedTask;
    }
}
#endregion


#region Bar command
public record BarWritingCommand(string Value) : IRequest, IStorageInteractor
{
    public string Key => nameof(FooWritingCommand);
}

public class BarWritingCommandHandler(ISomeExternalService externalService) : IRequestHandler<BarWritingCommand>
{
    public Task HandleAsync(BarWritingCommand request, CancellationToken cancellationToken = default)
    {
        externalService.SetValue(request.Key + "-Handler", request.Value);
        return Task.CompletedTask;
    }
}
#endregion

public class StorageServiceBehaviour<TRequest>(ISomeExternalService externalService) : IRequestBehaviour<TRequest>
    where TRequest : IRequest, IStorageInteractor
{
    public Task HandleAsync(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken = default)
    {
        externalService.SetValue(request.Key + "-Behaviour", $"Value writed by BEHAVIOUR {this.GetType().FullName}");
        return Task.CompletedTask;
    }
}

public class StorageServiceDecorator<TRequest>(ISomeExternalService externalService) : IRequestDecorator<TRequest>
    where TRequest : IRequest, IStorageInteractor
{
    public Task HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        externalService.SetValue(request.Key + "-Decorator", $"Value writed by DECORATOR {this.GetType().FullName}");
        return Task.CompletedTask;
    }
}