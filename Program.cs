using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JsonTest.Parsers;
using Microsoft.Extensions.Configuration;

namespace JsonTest
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
              services.AddHostedService<Worker>();
              services.AddSingleton<PlannerParser>();
              services.Configure<Settings.Settings>(options => hostContext.Configuration.GetSection("Settings").Bind(options));
            });
  }
}
