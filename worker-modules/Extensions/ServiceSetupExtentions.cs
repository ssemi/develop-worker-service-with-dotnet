using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using NLog.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.IO;
using Worker.Modules.MessageBroker;
using Worker.Modules.Models;

namespace Worker.Modules.Extensions
{
    public static class AppSetupExtentions
    {
        public static void SetAppSettingsJson(this IConfigurationBuilder config, string environmentName, string folder = "")
        {
            config
                .AddJsonFile(Path.Combine(folder, "appsettings.json"), optional: true, reloadOnChange: true)
                .AddJsonFile(Path.Combine(folder, $"appsettings.{environmentName?.ToLower()}.json"), optional: true);
        }

        public static void SetWorker(this IConfigurationBuilder config, IHostEnvironment env)
        {
            config.SetAppSettingsJson(env.EnvironmentName, "config");
        }
    }
    public static class ServiceSetupExtentions
    {
        public static void SetNLogging(this IServiceCollection services, IHostEnvironment env, string path = "")
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                //builder.SetMinimumLevel(LogLevel.Trace);

                var currentDir = $"{Directory.GetCurrentDirectory()}"; // {Path.DirectorySeparatorChar}
                var logConfigFile = env.IsDevelopment() ? "nlog.config" : $"nlog.{env.EnvironmentName?.ToLower()}.config";
                var logConfigPath = Path.Combine(currentDir, path, logConfigFile);
                var IsExists = File.Exists(logConfigPath);

                Console.WriteLine($"[NLog config result] file : {logConfigFile} - exists : {IsExists}");
                if (IsExists) builder.AddNLog(logConfigPath);
            });
        }
        public static void SetWorkerImplement(this IServiceCollection services)
        {
            services.Scan(scan => scan
                 .FromEntryAssembly()
                 .AddClasses(classes => classes.AssignableTo<IWorker>())
                 .AsImplementedInterfaces()
                 .WithTransientLifetime()
             );
            services.AddTransient<Runner>();
        }

        public static void SetMessageBroker(this IServiceCollection services)
        {
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<IPooledObjectPolicy<IModel>, RabbitModelPooledObjectPolicy>();
            services.AddSingleton<IRabbitManager, RabbitManager>();
        }

        public static void SetWorker(this IServiceCollection services, IHostEnvironment env, IConfiguration config)
        {
            services.SetNLogging(env, "config");

            services.AddOptions();
            services.Configure<AppSettings>(config.GetSection("AppSettings"));

            services.AddHttpClient();
            services.SetMessageBroker();

            services.SetWorkerImplement();
        }
    }
}
