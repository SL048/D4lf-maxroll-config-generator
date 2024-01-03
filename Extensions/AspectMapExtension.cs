using System.Collections.Generic;

namespace JsonTest.Extensions;

internal static class AspectMapExtension
{
  private static readonly Dictionary<string, string> ExplicitAspectsMappings = new()
  {
    { "of_the_frozen_wake", "of_frozen_wake" },
    { "of_gore_quills", "gore_quills" },
  };

  public static string TryGetMapping(this string aspectName) 
    => ExplicitAspectsMappings.GetValueOrDefault(aspectName, aspectName);
}