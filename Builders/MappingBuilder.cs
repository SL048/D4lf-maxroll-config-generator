using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using JsonTest.Extensions;

namespace JsonTest.Builders;

internal class MappingBuilder
{
  public async Task<string> BuildMaxRollToD4lfAspectsMapping(string maxrollJsonDataPath, string d4lfJsonDataPath)
  {
    var maxrollFilePath = Path.Combine(Directory.GetCurrentDirectory(), maxrollJsonDataPath);
    var maxrollJson = await File.ReadAllTextAsync(maxrollFilePath);
    var maxrollNode = JsonNode.Parse(maxrollJson);

    var legendaryNodesSuffixesAffixes = maxrollNode["affixes"].AsObject().AsEnumerable()
      .Where(w => w.Key.Contains("legend", StringComparison.CurrentCultureIgnoreCase)
                  && (w.Value["suffix"] is not null
                      || w.Value["prefix"] is not null))
      .Select(s => new
      {
        SuffixPrefix = (s.Value["suffix"] ?? s.Value["prefix"]).ToString().Replace(" ", "_").Replace("-", "").Replace("'", "").Replace("’", "").ToLower().TryGetMapping(),
        Id = s.Value["id"].ToString(),
      })
      .ToList();

    var d4lfPath = Path.Combine(Directory.GetCurrentDirectory(), d4lfJsonDataPath);
    var d4lfJson = await File.ReadAllTextAsync(d4lfPath);
    var d4lfNode = JsonNode.Parse(d4lfJson);
    var d4lfLegendaryNodes = d4lfNode.AsObject().AsEnumerable().Select(s => s.Key).ToList();

    var inD4lf = new List<AspectMapDto>();
    var notInD4lf = legendaryNodesSuffixesAffixes.Where(w => false).ToList();
    foreach (var item in legendaryNodesSuffixesAffixes)
    {
      var entry = d4lfLegendaryNodes.FirstOrDefault(f => f.Contains(item.SuffixPrefix));
      if (entry is not null)
      {
        inD4lf.Add(new AspectMapDto
        {
          MaxrollId = int.Parse(item.Id),
          MaxrollSuffixPrefix = item.SuffixPrefix,
          D4lfString = entry
        });
        continue;
      }

      notInD4lf.Add(item);

    }

    var mappingDictionary = inD4lf.ToDictionary(key => key.MaxrollId, value => value.D4lfString);

    return ObjectDumper.Dump(mappingDictionary);
  }
}