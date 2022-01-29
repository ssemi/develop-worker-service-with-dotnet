using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Worker.Modules;
using Worker.Modules.Extensions;

namespace Worker_Converter5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var env = builderContext.HostingEnvironment;

                    config.SetWorker(env);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var env = hostContext.HostingEnvironment;
                    var config = hostContext.Configuration;

                    services.SetWorker(env, config);
                })
                .Build();

            var svc = ActivatorUtilities.CreateInstance<Runner>(host.Services);
            svc.DoAction("");
        }
    }
}
