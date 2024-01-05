using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using JsonTest.Constants;
using JsonTest.Mappings;
using Microsoft.Extensions.Options;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using System.Net.Http;

namespace JsonTest.Parsers;

internal class PlannerParser
{
  private readonly Settings.Settings _settings;
  private readonly HttpClient _httpClient = new();

  public PlannerParser(IOptions<Settings.Settings> settings)
  {
    _settings = settings.Value;
  }

  public async Task ParsePlanner()
  {
    var plannerResponseUri = new Uri(new Uri(_settings.PlannerJsonUrl), _settings.PlannerId);
    var plannerResponseJson = await _httpClient.GetStringAsync(plannerResponseUri);
    var plannerResponseNode = JsonNode.Parse(plannerResponseJson);

    var plannerDataString = plannerResponseNode["data"].AsValue().ToString();

    var  plannerJson =  JsonNode.Parse(plannerDataString);

    var items = plannerJson["items"].AsObject();

    foreach (var profile in plannerJson["profiles"].AsArray())
    {
      var profileName = profile["name"].ToString();
      var config = new D4lfConfig();
      foreach (var profileItem in profile["items"].AsObject())
      {
        var itemName = InventorySlot.SlotDictionary[profileItem.Key].Replace(" ", "");
        var item = items[profileItem.Value.ToString()];
        var itemId = item["id"].ToString();

        // Uniques not implemented yet
        if (itemId.Contains("unique", StringComparison.InvariantCultureIgnoreCase))
          continue;

        var itemPower = item["power"].ToString();
        var explicitList = new List<AffixDto>();

        foreach (var itemAffix in item["explicits"].AsArray())
        {
          explicitList.Add(new AffixDto
          {
            D4lfAffixKey = MaxrollToD4lfMap.FromMaxrollToD4lf.GetValueOrDefault(int.Parse(itemAffix["nid"].ToString()), "Oops,no mapping for this one"),
            FirstAffixValue = double.Parse(itemAffix["values"].AsArray().FirstOrDefault()?.ToString(),
              CultureInfo.InvariantCulture)
          });
        }

        if (item.AsObject().ContainsKey("legendaryPower"))
        {
          config.Aspects.Add(new object[]
            { MaxrollToD4lfMap.FromMaxrollToD4lf.GetValueOrDefault(int.Parse(item["legendaryPower"]["nid"].ToString()), "Oops,no mapping for this one")});
        }

        config.Affixes.Add(new Dictionary<string, AffixConfig>()
        {
          {itemName, new AffixConfig
          {
            itemType = InventorySlot.GetD4lfTypeByMaxrollItemKey(itemId),
            minPower = 800, //int.Parse(itemPower),
            affixPool = explicitList.Select(s => new object[] { s.D4lfAffixKey })
            ,
            minAffixCount = explicitList.Count - 1
          }}
        }
          );
      }

      var serializer = new SerializerBuilder()
        .WithEventEmitter(next => new FlowEverythingEmitter(next))
        .Build();

     var yaml = serializer.Serialize(config);

      await using var writer = File.CreateText(Path.Combine(Directory.GetCurrentDirectory(), $"{profileName}.yaml"));

      await writer.WriteAsync(yaml);

      writer.Close();
    }
  }
}

internal class AffixDto
{
  public string D4lfAffixKey { get; set; }
  public double? FirstAffixValue { get; set; }
}

internal class D4lfConfig
{
  public List<object> Aspects { get; set; } = new();
  public List<Dictionary<string, AffixConfig>> Affixes { get; set; } = new();
}

internal class AffixConfig
{
  public string itemType  { get; set; }
  public int minPower  { get; set; }
  public object affixPool { get; set; }
  public int  minAffixCount { get; set; }
}

public class FlowEverythingEmitter(IEventEmitter nextEmitter) : ChainedEventEmitter(nextEmitter)
{
  public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
  {
    if (typeof(object[]).IsAssignableFrom(eventInfo.Source.Type))
    {
      eventInfo = new SequenceStartEventInfo(eventInfo.Source)
      {
        Style = SequenceStyle.Flow
      };
    }
    nextEmitter.Emit(eventInfo, emitter);
  }
}