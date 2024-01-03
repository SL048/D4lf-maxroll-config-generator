using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using JsonTest.Extensions;

namespace JsonTest
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      
    }
  }

  public class AspectMapDto
  {
    public int MaxrollId { get; set; }
    public string MaxrollSuffixPrefix  { get; set; }
    public string D4lfString { get; set; }
  }
}
