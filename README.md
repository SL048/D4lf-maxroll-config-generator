# D4lf maxroll config  generator

YAML generator for D4 lootfilter https://github.com/aeon0/d4lf

# Features
- Generate YAML with affixes and aspects for build
- Unique items are skipped
- hardcoded default value for min item power - 800
- hardcoded value for min affixes - (count of affixPool -1)
- some d4lf consts not mapped for ones from  maxroll. Some of  them commented in map classes. [Example](https://github.com/SL048/D4lf-maxroll-config-generator/blob/bc12856c34153f522ef6831f7547805a518c1c0b/Extensions/AspectMapExtension.cs#L31)
- threshold values and conditions for affixes and aspects ignored

# Usage

- Unpack release rar file
- Change Settings.PlannerId in appsettings.json. You can get that id from maxroll planner
![image](https://github.com/SL048/D4lf-maxroll-config-generator/assets/82326638/382ada7a-6b34-488c-9f6d-1ca8164ea851)
- Make sure you didn't copy #2 (or any other digit after #)
- Start JsonTest.exe
- Copy generated YAMLs to D4lf
- files are generated in the same path where the JsonTest.exe is located
