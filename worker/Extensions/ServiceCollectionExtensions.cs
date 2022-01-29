using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Worker_A;
using Worker_B;
using Worker_Converter;

namespace Worker.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddScopedWorkerDynamic<TInterface>(this IServiceCollection services, string workerType)
        {
            //var d = from val in Assembly.GetExecutingAssembly().GetReferencedAssemblies() select Assembly.Load(val.ToString());

            var list = new List<Assembly>
        {
            Assembly.GetAssembly(typeof(WorkerA)),
            Assembly.GetAssembly(typeof(WorkerB)),
            Assembly.GetAssembly(typeof(WorkerConverter))
        };

            services.Scan(scan => scan
                .FromAssemblies(list.AsEnumerable())
                .AddClasses(classes => classes.AssignableTo<TInterface>())
                .AsSelf()
                .WithScopedLifetime()
            );

            // workerType 에 맞게 ServiceType 구현
            services.AddScoped(typeof(TInterface), serviceProvider =>
            {
                var type = services.Where(x => x.ServiceType.Name.Contains(workerType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.ServiceType;

                if (null == type)
                    throw new KeyNotFoundException("No instance found for the given tenant.");

                return (TInterface)serviceProvider.GetService(type);
            });
        }
    }
}
