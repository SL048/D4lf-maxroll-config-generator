using System.Collections.Generic;

namespace JsonTest.Constants;

internal static class InventorySlot
{

  public static string GetD4lfTypeByMaxrollItemKey(string maxrollItemKey)
    => D4LfTypeByMaxrollType.GetValueOrDefault(
      MaxrollItemTypeByItemKey.GetValueOrDefault(maxrollItemKey, "Ooops,no mapping for this one"),
      "Ooops,no mapping for this one");

  public static Dictionary<string, string> SlotDictionary = new()
  {
    { "4", "Helm" },
    { "5", "Chest Armor" },
    { "6", "Offhand" },
    { "7", "Mainhand" },
    { "8", "Bludgeoning Weapon" },
    { "9", "Slicing Weapon" },
    { "10", "Ranged Weapon" },
    { "11", "OtherMainhand" },
    { "12", "OtherOffhand" },
    { "13", "Gloves" },
    { "14", "Pants" },
    { "15", "Boots" },
    { "16", "Right Ring" },
    { "17", "Left Ring" },
    { "18", "Amulet" }
  };

  public static Dictionary<string, string> D4LfTypeByMaxrollType = new()
  {
    { "Helm", "helm" },
    { "ChestArmor", "chest armor" },
    { "Legs", "pants" },
    { "Gloves", "gloves" },
    { "Boots", "boots" },
    { "Ring", "ring" },
    { "Amulet", "amulet" },
    { "Axe", "axe" },
    { "Axe2H", "two-handed axe" },
    { "Sword", "sword" },
    { "Sword2H", "two-handed sword" },
    { "Mace", "mace" },
    { "Mace2H", "two-handed mace" },
    { "Mace2HDruid", "two-handed mace" },
    { "Scythe", "scythe" },
    { "Scythe2H", "two-handed scythe" },
    //??{ "18", "bracers" },
    { "Crossbow2H", "crossbow" },
    { "Dagger", "dagger" },
    { "DaggerOffHand", "dagger" },
    { "Polearm", "polearm" },
    { "Shield", "shield" },
    { "Staff", "staff" },
    { "StaffDruid", "staff" },
    { "StaffSorcerer", "staff" },
    { "Wand", "wand" },
    //?{ "18", "offhand" },
    { "OffHandTotem", "totem" },
    { "Focus", "focus" },
    { "FocusBookOffHand", "focus" }
    //??{ "18", "material" },
    //??{ "18", "sigil" },
    //??{ "18", "elixir" }
  };

  public static Dictionary<string, string> MaxrollItemTypeByItemKey = new();

}
