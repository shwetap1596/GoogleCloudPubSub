using ConsolePubSubApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsolePubSubApp
{
    class Program
    {
        public static IConfiguration Configuration { get; private set; }

        public static async Task Main(string[] args)
        {
            try
            {
                //Configure settings
                Configuration=new ConfigurationBuilder()//Microsoft.Extensions.Configuration
                                .SetBasePath(Directory.GetCurrentDirectory())//Microsoft.Extensions.Configuration.FileExtensions
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)//Microsoft.Extensions.Configuration.Json
                                .Build();

                //Configure Log
                Log.Logger= new LoggerConfiguration().WriteTo.File("app.log").CreateLogger();//Serilog.Sinks.File for console you have to add Serilog.Sinks.Console and so on

                // Create service collection and configure our services
                IServiceCollection services=ConfigureServices();

                //Set environment variable so that while creating publisher and subscriber client it will take service account details from environment variable.
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Configuration.GetSection("GOOGLE_APPLICATION_CREDENTIALS").Value);


                IServiceProvider serviceProvider =services.BuildServiceProvider();//Microsoft.Extensions.DependencyInjection
                await serviceProvider.GetService<Publisher>().Publish();
                await serviceProvider.GetService<Subscriber>().Subscription();
            }
            catch (Exception ex)
            {
                Log.Information($"Exception in Main Method: {ex.ToString()}");
            }
        }

        private static IServiceCollection ConfigureServices()
        {
            //Dependency injection, allows strongly-typed configuration values to be injected in constructors
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(Configuration);
            serviceCollection.AddTransient<Publisher>();
            serviceCollection.AddTransient<Subscriber>();
            return serviceCollection;
        }
    }
}
