using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DyoMediator.Abstraction;

namespace DyoMediator.Mediator.DIExtension;

public static class MyMediatorDIExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMyMediator(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services.Add(new(typeof(Abstraction.IMyMediator), sp => new MyMediator(sp), serviceLifetime));
            return services;
        }

        public IServiceCollection AddHandler<TRequest, TResponse, THandler>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TRequest : IRequest<TResponse>
            where TResponse : class
            where THandler : class, IRequestHandler<TRequest, TResponse>
        {
            services.Add(new(typeof(IRequestHandler<TRequest, TResponse>), typeof(THandler), serviceLifetime));
            return services;
        }

        public IServiceCollection AddHandler<TRequest, THandler>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TRequest : IRequest

        where THandler : class, IRequestHandler<TRequest>
        {
            services.Add(new(typeof(IRequestHandler<TRequest>), typeof(THandler), serviceLifetime));
            return services;
        }

        public IServiceCollection AddBehaviour(Type behaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            if(behaviorType == null)
                throw new ArgumentNullException(nameof(behaviorType));
            if(!behaviorType.IsGenericType || !(behaviorType.GetGenericArguments().Length is 1 or 2))
                throw new ArgumentException($"Seems your behaviour of type {behaviorType.Name} not corresponds {typeof(IRequestBehaviour<>).Name} or {typeof(IRequestBehaviour<,>).Name} signature", nameof(behaviorType));
            int genericArguments = behaviorType.GetGenericArguments().Length;   
            if(genericArguments == 2)
                services.Add(new(typeof(IRequestBehaviour<,>), behaviorType, serviceLifetime));
            if(genericArguments == 1)
                services.Add(new(typeof(IRequestBehaviour<>), behaviorType, serviceLifetime));
            return services;
        }

        private static bool HasIRequestHandlerInConstructorArguments(Type type)
        {
            return type
                .GetConstructors()
                    .All(ctor=>ctor.GetParameters().Select(x=>x.ParameterType)
                    .Any(par=> 
                        (par.IsGenericType && par.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                     || par.GetInterfaces()
                    .Any(i=>i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))));
        }

        private static bool HasIRequestHandlerInConstructorArgumentsDetailed(Type type)
        {
            var constructors = type.GetConstructors();
            foreach(var constructor in constructors)
            {
                var parametersTypes = constructor.GetParameters().Select(x=>x.ParameterType).ToArray();

                foreach (var parameterType in parametersTypes)
                {
                    if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof (IRequestHandler<,>))
                        return true;

                    var parameterInterfaces = parameterType.GetInterfaces();
                    foreach (var i in parameterInterfaces)
                    {
                        bool isIRequestHandler = i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>);
                        if(isIRequestHandler)
                            return true;
                    }

                }
            }
            return false;
        }

        public IServiceCollection AddHandlersFromAssembly(Assembly assembly, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));
            foreach (Type handlerType in handlerTypes)
            {
                if (HasIRequestHandlerInConstructorArguments(handlerType))
                    continue;
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType 
                    && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                    ) //It is 
                    .ToList();
                foreach (var interfaceType in interfaces)
                {
                    var requestType = interfaceType.GenericTypeArguments[0];
                    var responseType = interfaceType.GenericTypeArguments[1];
                    services.Add(new(interfaceType, handlerType, serviceLifetime));
                }
            }
            return services;
        }

        public IServiceCollection AddBehavioursFromAssembly(Assembly assembly, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) =>
            AddBehavioursFromTypesSet(services : services,  types: assembly.GetTypes(), serviceLifetime: serviceLifetime);

        public IServiceCollection AddBehavioursFromTypesSet(IEnumerable<Type> types, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var handlerTypes = 
                types.Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestBehaviour<,>)));
            foreach (Type handlerType in handlerTypes)
            {
                services.AddBehaviour(handlerType, serviceLifetime);
            }
            return services;
        }
    }

}
