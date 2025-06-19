using Masstransient.RabbitMq.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace Masstransient.RabbitMq.Configurations
{
    public static class WorkerConfiguration
    {
        public static IServiceCollection AddWorkerDemo(this IServiceCollection services)
        {
            // Register the worker as a hosted service
            services.AddHostedService<WorkerDemo>();
            return services;
        }
    }
}
