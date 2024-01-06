using System.Collections.Generic;

namespace JsonTest.Extensions;

internal static class AspectMapExtension
{
  private static readonly Dictionary<string, string> ExplicitAffixMappings = new()
  {
    {"maximum evade charge","max evade charges"},
    {"rain of arrows cooldown reduction","rain of arrows skill cooldown reduction"},
    {"damage reduction from burning enemies","damage reduction from enemies that are burning"},
    {"damage reduction from bleeding enemies","damage reduction from enemies that are bleeding"},
    {"damage reduction from poisoned enemies","damage reduction from enemies that are poisoned"},
    {"while berserking you deal fire damage every second to surrounding enemies","while berserking, you deal fire damage every second to"},

    //looks like description mistake in D4lf json
    {"casting a basic skill reduces the mana cost of your next core or mastery skill by","casting a basic or mastery skill reduces the mana cost of your next core skill by ."},

    {"gain increased damage to a set of damage types for seconds this effect alternates between sets fire lightning and physical cold poison and shadow","gain increased damage to a set of damage types for seconds. this effect alternates between sets"},
    {"you deal increased burning damage to enemies below lifewhile enemies are affected by more damage over time than their total life you deal increased burning damage to them","you deal bonus burning damage to enemies who are below of their total life. while enemies are affected by more damage over time than their total life, you deal increased burning damage to them."},

    // hope this is the same descriptions, looks like "casting a storm skill grants your earth.." can not exist w/o "casting a earth skill increases the critical strike chance of storm..." this is one affix in maxroll
    {"casting a storm skill grants your earth skills critical strike damage for secondscasting a earth skill increases the critical strike chance of storm skills by for seconds","casting a storm skill grants your earth skills critical strike damage for seconds."},

    {"unstable currents has a chance to cast an additional shock skill","currents has a chance to cast an additional shock skill."},
    {"your companions also gain the bonuses from the bestial rampage key passive when you do","your companions gain the bonuses from the bestial rampage key passive."},
    {"increase the critical strike damage of meteor and fireball by double this bonus against healthy targets","increases the critical strike damage of meteor and fireball by x . double this bonus against healthy enemies."},
    {"bone storm and blood wave are also darkness skills deal shadow damage and gain additional effects enemies damaged by bone storm take shadow damage over seconds blood wave creates desecrated ground as it travels dealing shadow damage over seconds","bone storm and blood wave are also darkness skills, deal shadow damage, and gain additional effects"},
    {"earth spike launches spikes in a line and has a second cooldown","earth spike launches in a line and has a second cooldown."},
    {"casting ice armor makes you unstoppable and grants increased armor for seconds","casting ice armor makes you unstoppable and grants bonus armor for seconds."},

    // there are already exists description for "rank of the inner flames passive", looks like the same thing, duplication? And what is it - "fire damage"? {"??","fire damage ranks of the inner flames passive"}, 
    // looks like the  same as "damage reduction from shadow damage over timeaffected enemies", duplication? {"??","damage reduction from enemies that are affected by shadow damage over time"},
    // there are already exists description for "lucky hit up to a chance to heal life", looks like the same thing, duplication? And what is it - "crowd control duration"?  {"??","crowd control duration lucky hit up to a chance to heal life"},
    // there are already exists description for "resource regeneration", looks like the same thing, duplication? In maxroll the only resource generation exists, by the way. {"??","resource generation"}, 
  };

  public static string TryGetExplicitAffixMapping(this string maxrollAffixDescription)
    => ExplicitAffixMappings.GetValueOrDefault(maxrollAffixDescription, maxrollAffixDescription);
}