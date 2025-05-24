using Microsoft.Extensions.DependencyInjection;
using MyMediator.Abstraction;
using Scrutor;

namespace MyMediator.Decorators.DIExtension;

public static class DIExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDecorator(Type decoratorType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            bool decoratorSucceed = services.TryDecorate(typeof(IRequestHandler<,>), decoratorType);
            return services;
        }

        public IServiceCollection AddDecoratorsFromAssembly(IEnumerable<Type> types)
        {
            var handlerTypes = types.Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestDecorator<,>)));
            foreach (Type handlerType in handlerTypes)
            {
                services.TryDecorate(typeof(IRequestHandler<,>), handlerType);
            }
            return services;
        }
    }
}
