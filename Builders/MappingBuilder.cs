using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using F23.StringSimilarity;
using JsonTest.Constants;
using JsonTest.Extensions;
using JsonTest.JavaScript;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;

namespace JsonTest.Builders;

internal class MappingBuilder
{
  private const string JsFromReactObjectStringsConcatFunction = 
    """
    function concatStrings(node, sumString) {
      if (node && !(node instanceof Array)) {
        if (node.props.children && node.props.children instanceof Array) {
          for (let child of node.props.children) {
            if (typeof child === "string") {
              sumString = sumString.concat(child)
            }
            else {
              sumString = concatStrings(child, sumString)
            }
          }
        }
        if (node.props.children && !(node.props.children instanceof Array)) {
          if (typeof node.props.children === "string") {
            sumString = sumString.concat(node.props.children)
          }
          else {
            sumString = concatStrings(node.props.children, sumString)
          }
        }
      }
      return sumString;
    }
    """;

  private const string JsGetAffixDescriptions =
    """
    WL1().loadData().then(() => {
    
      var staticData = Q().default(Z1()).default;
      var result = [];
      var exception = [];
    
      Object.keys(staticData.affixes).forEach(function (key, index) {
        var request = { affix: staticData.affixes[key], ipower: 925, multiplier: 1, values: [0.001, 0.002, 0.003] }
        try {
          result.push({ id: staticData.affixes[key].id, description: concatStrings(c0().AffixEffect(request).type(request), "") });
        } catch (exceptionVar) {
          exception.push(staticData.affixes[key])
        }
      });
    
      if (exception.length > 0) {
        return exception;
      }
    
      return result;
    })
    """;

  private const string JsGetItemsTypes =
    """
    WL1().loadData().then(() => {
    
      var staticData = Q().default(Z1()).default;
      var result = [];
      var exception = [];
    
      Object.keys(staticData.items).forEach(function (key, index) {
        try {
          result.push({ key: key, type: staticData.items[key].type});
        } catch (exceptionVar) {
          exception.push(staticData.items[key])
        }
      });
    
      if (exception.length > 0) {
        return exception;
      }
    
      return result;
    })
    """;

  public static void InitItemsTypesDictionary()
  {
    using var engine = new V8ScriptEngine();
    engine.DocumentSettings.SearchPath = Directory.GetCurrentDirectory();
    engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

    dynamic result = engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, JS.Code + JsGetItemsTypes);

    foreach (var resultItem in result)
    {
      InventorySlot.MaxrollItemTypeByItemKey.Add(resultItem.key, resultItem.type);
    }
  }

  public static async Task<string> BuildMaxRollToD4lfMapping(string d4lfAffixesFileName, string d4lfAspectsFileName)
  {
    using var engine = new V8ScriptEngine();
    engine.DocumentSettings.SearchPath = Directory.GetCurrentDirectory();
    engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

    dynamic result = engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, JS.Code + JsFromReactObjectStringsConcatFunction + JsGetAffixDescriptions);
    var affixMap = new Dictionary<int, AffixMaxRollToD4lfMapDto>();

    var xRegex = new Regex(@"(\[.*?\])");
    var braceRegex = new Regex(@"(\(.*?\))");
    var azRegex = new Regex("[^a-zA-Z ]");
    var spaceRegex = new Regex("[ \\t]{2,}");

    foreach (var resultItem in result)
    {
      affixMap.Add((int)resultItem.id, new AffixMaxRollToD4lfMapDto
        {
          MaxrollId = (int)resultItem.id,
          MaxrollDescription = spaceRegex
            .Replace(azRegex.Replace(xRegex.Replace(braceRegex.Replace((string)resultItem.description, ""), ""), ""), " ")
            .ToLower()
            .Trim()
            .TryGetExplicitAffixMapping()
      }
      );
    }

    {
      var d4lfAffixesPath = Path.Combine(Directory.GetCurrentDirectory(), d4lfAffixesFileName);
      var d4lfAffixesJson = await File.ReadAllTextAsync(d4lfAffixesPath);
      var d4lfAffixesNode = JsonNode.Parse(d4lfAffixesJson);
      var d4lfAffixes = d4lfAffixesNode.AsObject().AsEnumerable()
        .Select(s => new { Value = s.Value.ToString(), s.Key });

      foreach (var d4lfAffixNode in d4lfAffixes)
      {
        var affixMapItems = affixMap.Values.Where(a => a.MaxrollDescription == d4lfAffixNode.Value);

        foreach (var affixMapItem in affixMapItems)
        {
          affixMapItem.D4lfDescription = d4lfAffixNode.Value;
          affixMapItem.D4lfKey = d4lfAffixNode.Key;
        }
      }

      var matchedD4lfAffixes = affixMap.Values
        .Where(w => w.D4lfDescription is not null)
        .Select(s => s.D4lfDescription)
        .Distinct()
        .ToList();

      var notMatchedD4lfAffixes = d4lfAffixesNode.AsObject().AsEnumerable()
        .Where(w => !matchedD4lfAffixes.Contains(w.Value.ToString())).ToList();

      var levenshtein = new NormalizedLevenshtein();

      foreach (var notMatchedD4LfAffix in notMatchedD4lfAffixes)
      {
        var highSimilarityMatches = affixMap.Values.Where(f =>
          levenshtein.Similarity(f.MaxrollDescription, notMatchedD4LfAffix.Value.ToString()) > 0.9);

        foreach (var highSimilarityMatch in highSimilarityMatches)
        {
          highSimilarityMatch.D4lfDescription = notMatchedD4LfAffix.Value.ToString();
          highSimilarityMatch.D4lfKey = notMatchedD4LfAffix.Key;
        }
      }

      matchedD4lfAffixes = affixMap.Values
        .Where(w => w.D4lfDescription is not null)
        .Select(s => s.D4lfDescription)
        .Distinct()
        .ToList();

      notMatchedD4lfAffixes = d4lfAffixesNode.AsObject().AsEnumerable()
        .Where(w => !matchedD4lfAffixes.Contains(w.Value.ToString())).ToList();

      var duplicateDescriptions = affixMap.Values.GroupBy(g => g.MaxrollDescription)
        .Where(w => w.Select(s => s).Count() > 1).ToList();

      var lowStringSimilarityMatches = notMatchedD4lfAffixes
        .Select(s => s.Value.ToString())
        .Select(s =>
          affixMap.Values
            .Select(f => new
            {
              similarity = levenshtein.Similarity(f.MaxrollDescription, s), f, D4lfDescription = s, f.MaxrollDescription
            })
            .OrderByDescending(o => o.similarity)
        )
        .SelectMany(s => s)
        .GroupBy(l => l.D4lfDescription);
    }

    {
      var d4lfAspectsPath = Path.Combine(Directory.GetCurrentDirectory(), d4lfAspectsFileName);
      var d4lfAspectsJson = await File.ReadAllTextAsync(d4lfAspectsPath);
      var d4lfAspectsNode = JsonNode.Parse(d4lfAspectsJson);
      var d4lfAspects = d4lfAspectsNode.AsObject().AsEnumerable()
        .Select(s => new { Value = s.Value.ToString(), s.Key });

      foreach (var d4lfAspectNode in d4lfAspects)
      {
        var aspectMapItems = affixMap.Values.Where(a => a.MaxrollDescription == d4lfAspectNode.Value);

        foreach (var aspectMapItem in aspectMapItems)
        {
          aspectMapItem.D4lfDescription = d4lfAspectNode.Value;
          aspectMapItem.D4lfKey = d4lfAspectNode.Key;
        }
      }

      var matchedD4lfAspectes = affixMap.Values
        .Where(w => w.D4lfDescription is not null)
        .Select(s => s.D4lfDescription)
        .Distinct()
        .ToList();

      var notMatchedD4lfAspectes = d4lfAspectsNode.AsObject().AsEnumerable()
        .Where(w => !matchedD4lfAspectes.Contains(w.Value.ToString())).ToList();

      var levenshtein = new NormalizedLevenshtein();

      foreach (var notMatchedD4LfAspect in notMatchedD4lfAspectes)
      {
        var highSimilarityMatches = affixMap.Values.Where(f =>
          levenshtein.Similarity(f.MaxrollDescription, notMatchedD4LfAspect.Value.ToString()) > 0.9);

        foreach (var highSimilarityMatch in highSimilarityMatches)
        {
          highSimilarityMatch.D4lfDescription = notMatchedD4LfAspect.Value.ToString();
          highSimilarityMatch.D4lfKey = notMatchedD4LfAspect.Key;
        }
      }

      matchedD4lfAspectes = affixMap.Values
        .Where(w => w.D4lfDescription is not null)
        .Select(s => s.D4lfDescription)
        .Distinct()
        .ToList();

      notMatchedD4lfAspectes = d4lfAspectsNode.AsObject().AsEnumerable()
        .Where(w => !matchedD4lfAspectes.Contains(w.Value.ToString())).ToList();

      var lowStringSimilarityMatches = notMatchedD4lfAspectes
        .Select(s => s.Value.ToString())
        .Select(s =>
          affixMap.Values
            .Select(f => new
            {
              similarity = levenshtein.Similarity(f.MaxrollDescription, s),
              f,
              D4lfDescription = s,
              f.MaxrollDescription
            })
            .OrderByDescending(o => o.similarity)
        )
        .SelectMany(s => s)
        .GroupBy(l => l.D4lfDescription);
    }
    var maxrollAffixIdToD4lfKeyMap = affixMap.Where(w => w.Value.D4lfDescription is not null)
      .Select(s => s.Value)
      .ToDictionary(k => k.MaxrollId, v => v.D4lfKey);

    var codeInitialization = ObjectDumper.Dump(maxrollAffixIdToD4lfKeyMap);
    return codeInitialization;
  }

  public static async Task InitMaxrollJsCode()
  {
    JS.Code = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "JavaScript/JsCode.js"));
  }
}

internal class AffixMaxRollToD4lfMapDto
{
  public int MaxrollId { get; set; }
  public string MaxrollDescription { get; set; }
  public string D4lfDescription { get; set; }
  public string D4lfKey { get; set; }
}