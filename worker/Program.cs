using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using NLog;
using NLog.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using Worker.Extensions;
using Worker.Models;
using Worker.Modules;
using Worker.Modules.MessageBroker;
using Worker.Modules.Models;

namespace Worker
{
    class Program
    {
        private static string environmentName = "development";
        private static string WorkerType = string.Empty;

        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                var p = new Parser(config => config.IgnoreUnknownArguments = true);
                p.ParseArguments<CommandOptions>(args)
                    .WithParsed(opts =>
                    {
                        RunOptionsAndReturnExitCode(opts);
                    })
                .WithNotParsed((errs) => HandleParseError(errs));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        static void RunOptionsAndReturnExitCode(CommandOptions opts)
        {
            environmentName = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development").ToLower();
            WorkerType = Environment.GetEnvironmentVariable("WORKER_TYPE")?.ToLower();
            var optionQueueName = opts.QueueName;

            if (string.IsNullOrEmpty(opts.Environment) == false) environmentName = opts.Environment?.ToLower();
            if (string.IsNullOrEmpty(opts.WorkerType) == false) WorkerType = opts.WorkerType;

            if (WorkerType is null)
                throw new ArgumentNullException(nameof(WorkerType));

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environmentName);
            Environment.SetEnvironmentVariable("WORKER_TYPE", WorkerType);

            Console.WriteLine($"Environment : {environmentName} | _WORKER_TYPE : {WorkerType}");

            MainRun(optionQueueName);

        }
        static void HandleParseError(IEnumerable<Error> opts)
        {
            Console.WriteLine(JsonConvert.SerializeObject(opts, Formatting.Indented));
        }

        static void MainRun(string optionQueueName)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
               .AddJsonFile($"appsettings.{WorkerType}.json", optional: true)
               .AddJsonFile($"appsettings.{WorkerType}.{environmentName}.json", optional: true)
               .AddEnvironmentVariables()
               .Build();

            var servicesProvider = BuildDi(config);
            using (servicesProvider as IDisposable)
            {
                var runner = servicesProvider.GetRequiredService<Runner>();
                runner.DoAction(optionQueueName);

                //Console.WriteLine("Press ANY key to exit");
                //Console.ReadKey();
            }
        }



        private static IServiceProvider BuildDi(IConfiguration config)
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<AppSettings>(config.GetSection("AppSettings"));

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

                var logConfigPath = "nlog.config";
                var logFolderPath = Directory.GetCurrentDirectory();
                var nlogConditionalPath1 = $"nlog.{WorkerType}.{environmentName}.config";
                var nlogConditionalPath2 = $"nlog.{WorkerType}.config";
                var nlogConditionalPath3 = $"nlog.{environmentName}.config";

                if (File.Exists(Path.Combine(logFolderPath, nlogConditionalPath1)))
                    logConfigPath = nlogConditionalPath1;
                else if (File.Exists(Path.Combine(logFolderPath, nlogConditionalPath2)))
                    logConfigPath = nlogConditionalPath2;
                else if (File.Exists(Path.Combine(logFolderPath, nlogConditionalPath3)))
                    logConfigPath = nlogConditionalPath3;

                Console.WriteLine($"nlog config file name result : {logConfigPath}");
                builder.AddNLog(logConfigPath);
            });

            services.AddHttpClient();
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<IPooledObjectPolicy<IModel>, RabbitModelPooledObjectPolicy>();
            services.AddSingleton<IRabbitManager, RabbitManager>();

            services.AddScopedWorkerDynamic<IWorker>(WorkerType);
            services.AddTransient<Runner>();

            return services.BuildServiceProvider();
        }
    }
}
