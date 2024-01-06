using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using JsonTest.Builders;
using JsonTest.Parsers;

namespace JsonTest
{
  internal class Worker(
    IHostApplicationLifetime hostApplicationLifetime,
    PlannerParser parser,
    ILogger<Worker> logger)
    : BackgroundService
  {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      try
      {
        await MappingBuilder.InitMaxrollJsCode();
        MappingBuilder.InitItemsTypesDictionary();
        await parser.ParsePlanner();
      }
      catch (Exception e)
      {
        logger.LogError(e.Message);
        throw;
      }
      finally
      {
        hostApplicationLifetime.StopApplication();
      }
    }
  }
}