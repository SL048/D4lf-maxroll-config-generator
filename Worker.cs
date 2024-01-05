using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using JsonTest.Builders;
using JsonTest.Parsers;

namespace JsonTest
{
  internal class Worker : BackgroundService
  {
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly PlannerParser _parser;
    private readonly ILogger<Worker> _logger;

    public Worker(
      IHostApplicationLifetime hostApplicationLifetime,
      PlannerParser parser,
      ILogger<Worker> logger)

    {
      _hostApplicationLifetime = hostApplicationLifetime;
      _parser = parser;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      try
      {
        await MappingBuilder.InitMaxrollJsCode();
        MappingBuilder.InitItemsTypesDictionary();
        await _parser.ParsePlanner();
      }
      catch (Exception e)
      {
        _logger.LogError(e.Message);
        throw;
      }
      finally
      {
        _hostApplicationLifetime.StopApplication();
      }
    }
  }
}