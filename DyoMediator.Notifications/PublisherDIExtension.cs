using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DyoMediator.Notifications;

public static class PublisherDIExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMyPublisher<TNotificationBaseType>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, Assembly? assembly = null)
        where TNotificationBaseType : class => AddMyPublisher<TNotificationBaseType>(services, serviceLifetime, ((assembly??Assembly.GetCallingAssembly()).GetTypes()));
       

        public IServiceCollection AddMyPublisher<TNotificationBaseType>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, params Type[] types)
            where TNotificationBaseType : class
        {
            var handlerInterfaceOpen = typeof(IMyNotificationHandler<>);

            var candidates = types
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .ToArray();

            foreach (var type in candidates)
            {
                var handlerInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceOpen)
                    .ToArray();

                foreach (var handlerInterface in handlerInterfaces)
                {
                    var notificationType = handlerInterface.GetGenericArguments()[0];
                    // register only handlers whose notification type is assignable to the provided base type
                    if (typeof(TNotificationBaseType).IsAssignableFrom(notificationType))
                    {
                        services.Add(new ServiceDescriptor(handlerInterface, type, serviceLifetime));
                    }
                }
            }

            // Register the publisher itself so consumers can publish notifications
            services.Add(new ServiceDescriptor(typeof(IMyPublisher), typeof(MyPublisher), serviceLifetime));

            return services;
        }
    }
    
}
