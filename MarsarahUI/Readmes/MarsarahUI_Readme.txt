Marsarah UI v1.1.1
================================================================
Marsarah UI is a standalone collection of UI and information improvements for Valheim.
The features originally started as the UI section of Marsarah Tweaks and have now been separated into their own mod so players can use the interface improvements without installing the gameplay tweaks.

The mod adds HUD information, improved nameplates and item indicators, detailed object hovers, better loading tips, weather information, and other interface improvements. Most features can be configured independently.

Marsarah UI also includes ServerSync support for selected gameplay/information-oriented UI features. Local UI preferences remain local by default, while server administrators can optionally override selected settings.


DEVELOPMENT NOTES
================================================================
DEEP NORTH / SPOILER NOTE:
Deep North-specific UI integration is intentionally limited for now. I want to experience the new biome and progression myself before digging through its mechanics in detail so I can play through it without spoiling the experience for myself.

After completing the Deep North, I plan to review new gear for Smart Biome and check whether any new mechanics would benefit from additional indicators or hover information.

AI USAGE DISCLOSURE:
The original UI features were developed as part of my MarsarahMod / Marsarah Tweaks projects. AI tools are now used as part of my development workflow for tasks such as debugging, refactoring, compatibility updates, researching game API changes, and documentation.

Development remains human-directed: I decide what features are added, how they should behave, and I write code, review and test the changes included in releases. The mod's logo was also created using generative AI.


RELATED MARSARAH MODS
================================================================
MarsarahTweaks
  Gameplay, balance, grind-reduction and quality-of-life tweaks.
  Marsarah UI includes optional compatibility with some Tweaks features.

MarsarahBuildPieces
  Custom functional and decorative build pieces, including portals, lights and the Mystical Light Ward.

Neither mod is required to use Marsarah UI.


PERMISSIONS
================================================================
Reuploading this mod, whether in part or in full, is not permitted.
Anyone is free to take inspiration or implement similar features, but must do so with their own code and assets.


REQUIREMENTS
================================================================
This mod requires BepInEx for Valheim:
https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/

ServerSync is bundled with MarsarahUI.dll and does not need to be installed separately.

For multiplayer servers using Marsarah UI Server Overrides, install Marsarah UI on the SERVER and all CLIENTS.


INSTALLATION
================================================================
1. Unpack the .zip file.
2. Copy MarsarahUI.dll into your Valheim/BepInEx/plugins folder.

Or use a mod manager.

If the server uses Marsarah UI Server Overrides, the mod must also be installed on the server and all connecting clients.


CONFIGURATION
================================================================
The config file is automatically created on first launch:
Valheim/BepInEx/config/Marsarah.MarsarahUI.cfg

Settings are organized into five categories:

01 - General & HUD (Local)
  General interface and HUD features.

02 - Information Rail (Local)
  Information Rail appearance, contents and related displays.

03 - Items & Interaction (Local)
  Build-menu information, nameplates, taming and item indicators.

04 - Detailed Hovers (Local)
  Detailed object-hover information and display modes.

05 - Server Overrides (Synced)
  Optional server controls for selected information-oriented features.

Local settings normally remain under each player's control.
Most UI settings can be changed while playing and update without requiring a restart.


SERVER OVERRIDES
----------------------------------------------------------------
- Lock Server Overrides
  Default: On
  Prevents non-admin players from changing synchronized override settings.
  Local UI settings are not locked.

- Enable Server Overrides
  Default: On
  Master switch for the override system.
  Individual overrides default to UserChoice, so this does not force anything by itself.

- Bool override options:
  UserChoice - respect the player's local setting.
  ForceOn    - force the feature on.
  ForceOff   - force the feature off.

- Mode-based overrides can force a specific display mode where applicable.

Server-overridable features:
  • Information Rail Display Mode
  • Enemy Detector
  • Current Day
  • Current Time
  • Weather Forecast
  • Smart Biome
  • Ashlands Heat Meter
  • Enemy Nameplate Mode
  • Taming Progress
  • Detailed Hover Information
  • Container Contents
  • Global Chat By Default

Information Rail Display Mode Override:
  UserChoice
  Icons
  Text

NOTE FOR v1.0.0 USERS
----------------------------------------------------------------
The config layout and setting keys were reorganized in v1.1.0.
Existing selections from v1.0.0 may need to be configured again after updating.


KEYS
----------------------------------------------------------------
Insert
  Temporarily hides/shows many custom HUD widgets.

Home
  Toggles the Online Players name list.
  The total Online count remains visible while the list is hidden.


MOD CONFIGS
================================================================

===================== [01 - General & HUD] =====================

---------------------- [Better Loading Tips] -------------------
► Description:
  Replaces the vanilla loading-tip selection with a curated pool of 51 gameplay tips.
  Tips focus on controls, progression, combat, building, food, exploration and less-obvious mechanics.
  Several very basic vanilla tips are intentionally omitted.
  Only works with English localization.  


------------------------ [Show Boat Speed] ---------------------
► Description:
  Displays current ship speed while controlling a boat.
  Reverse speed uses an R prefix.
  Text color changes according to speed.

► Compatibility:
  Automatically moves when Status Effects Under Minimap changes the sailing HUD. Uses alternate positioning in No Map worlds.
  Not compatible with Minimal Status Effects.


------------------------ [Show Current Day] --------------------
► Description:
  Displays the current world day above the minimap.
  Also works in No Map worlds.


----------------------- [Show Current Time] --------------------
► Modes:
  DigitalClock - 24-hour HH:MM display.
  DayPhases    - Night, Dawn, Morning, Day, Afternoon, Evening, Dusk.
  Off          - disables the time display.

  Day-phase text is color coded.
  The display also works in No Map worlds.


-------------- [Show Weather Forecast Indicator] ---------------
► Description:
  Shows the next scheduled weather using an icon and countdown near the minimap.
  Weather icons are embedded in MarsarahUI.dll.
  The feature searches upcoming environment periods for the next different weather.
  If no different weather is found in the forecast window, the current weather icon is shown with --:--.
  Alternative-biome forecasting is supported for Dark Meadows.

► Compatibility:
  Includes mappings for several environment names used by Seasons and similar weather changes where supported.


---------------------- [Smart Biome Indicator] -----------------
► Description:
  Replaces the normal minimap biome text with a color-coded indicator based on currently equipped armor and armor quality.

► Colors:
  Purple - severely undergeared
  Red    - difficult
  Orange - below expected progression
  Yellow - around expected progression
  Green  - well prepared / above expected progression
  White  - biome has no configured gear range

► Progression Ranges:
  Meadows
  Black Forest
  Swamp
  Mountain
  Plains
  Mistlands
  Ashlands

► MarsarahTweaks Compatibility:
  If Tweaks is installed and Gear Upgrade Unlock is enabled, Smart Biome uses expanded expected armor ranges where Tweaks permits earlier level-4 upgrades.


----------------------- [Show Online Players] ------------------
► Description:
  Shows total online players and player names at the bottom-right.
  Hidden when only one player is online.
  Displays up to 20 player names.
  Home Key hides/shows player names while retaining the total Online count.
  Player names temporarily hide while the chat dialog is open.


--------------- [Show Boss Power Expiration Message] ----------
► Description:
  Displays a center-screen message when an active Forsaken Power expires.


------------------ [Show Heat Meter in Ashlands] --------------
► Description:
  Displays a heat meter at the top-center while relevant Ashlands water/lava heat mechanics are active.


------------------- [Status Effects Under Minimap] -------------
► Description:
  Moves active status effects into a compact vertical layout below the minimap.
  The sailing wind indicator and sailing controls are repositioned and resized while the feature is active.
  Boat Speed automatically moves to match the adjusted sailing HUD.
  The vanilla status-effect and sailing layouts are restored automatically in No Map worlds.


---------------------- [Global Chat By Default] -----------------
► Description:
  Makes normal chat messages global by default.
  Preserves the player's original capitalization instead of forcing Shout text to uppercase.
  Explicit commands such as /w and /say remain available and are not redirected.


--------------- [Logon Screen Character Statistics] ------------
► Description:
  Adds a Character Statistics panel to the character selection screen.

  Displays the following statistics: 
  - Playtime: Total character playtime, calculated from TimeInBase + TimeOutOfBase.
  - Foes Dispatched: Enemy kills.
  - Bosses Demoted: Boss kills.
  - Valhalla Rejections: Deaths.
  - Longest Survival: Highest consecutive days survived.
  - Arrows Liberated: Arrows fired.
  - Walks of Shame: Own tombstones opened.
  - Items Crafted: Items crafted.
  - Items Upgraded: Items upgraded.
  - Pieces Built: Build pieces placed.
  - Rocks and Ores Brutally Smashed: Rocks/ore deposits mined.
  - Trees Brutally Murdered: Trees felled.
  - Creatures Tamed: Creatures tamed.
  - Fish Acquired Legally: Fish caught.
  - Time at Home: Time spent inside the player's base.
  - Distance Traveled: Total distance traveled, displayed in kilometers.
  - Distance Sailed: Total distance sailed, displayed in kilometers.
  - Portals Used: Number of portal uses.
  - Jumps: Number of jumps.

  The panel also displays a conditional notable fact based on the selected character's history. These are not the vanilla achievements.
  List of facts:
  - Unusual death facts:
    - The Forest Remembers: Killed by a falling tree at least 1 time.
    - Curiosity Won: Killed by the edge of the world at least 1 time.
    - Occupational Hazard: Killed by a cart at least 1 time.
    - Flight Test Failed: Killed by a catapult at least 1 time.
    - Workplace Safety Violation: Killed by a drawbridge at least 1 time.
    - Disposal Error: Killed by the obliterator at least 1 time.
    - Security System Working as Intended: Killed by a turret at least 1 time.
    - Captain Went Down With the Ship: Killed by a boat at least 1 time.
    - The Floor Was Lava: Killed by Ashlands lava at least 1 time.
    - Hostile Waters: Killed by the Ashlands ocean at least 1 time.
  - Repeated death facts:
    - Natural Enemy: Gravity: Died from falling at least 3 times.
    - Swimming Lessons Recommended: Drowned at least 2 times.
    - Ventilation Required: Died from smoke inhalation at least 2 times.
    - Own Worst Enemy: Died by own hand at least 2 times.
    - Fire Safety Optional: Burned to death at least 3 times.
    - Should Have Packed a Cloak: Frozen to death at least 3 times.
    - Poison Control: Died from poison at least 3 times.
  - Activity & Events facts:
    - The Finishing Touch: Dealt the final blow to at least 5 bosses.
    - The Ones That Got Away: Lost at least 10 fish.
    - Odin's HR Has Been Notified: Hit a raven at least 5 times.
    - Necromancy Is a Hobby: Summoned at least 250 skeletons.
    - Retrieval Specialist: Opened at least 5 tombstones belonging to other players.
    - Born in a Barn: At least 100 more doors opened than closed, with at least 10% of opened doors left unclosed.
    - Thar She Blows: Caused at least 5 leviathans to sink.
    - Treasure Hunter: Found at least 10 treasures from buried treasure and location treasure counters combined.
      - Note: Dungeon treasure is intentionally excluded.
    - Captain: Traveled at least 100 km while sailing at the helm.
    - Measure Twice, Build Once? Built at least 100 pieces and removed at least 50% as many pieces as were built.
  - Level 100 skill facts:
    - Mountain Goat: Jump level 100.
    - Still Faster Than a Boat: Swim level 100.
    - You Were Never Here: Sneak level 100.
    - The Forest Definitely Remembers: Wood Cutting level 100.
    - Professional Rock Argument: Pickaxes level 100.
    - Not Today: Blocking level 100.
    - Cardio Is a Lifestyle: Run level 100.
    - Fish Fear Me: Fishing level 100.
    - Born in the Saddle: Ride level 100.
    - Outstanding in the Field: Farming level 100.
    - Yes, Chef: Cooking level 100.
  - Master Skill fact:
    - Master of Everything: Every vanilla skill has reached level 100.
    - When Master of Everything qualifies, the individual level 100 skill facts are not added to the random pool. Master of Everything represents the achievement instead.


==================== [02 - Information Rail] ===================

-------------------- [Information Rail Style] ------------------
► Description:
  Selects the visual style used by the Information Rail and Summon Counter (3 style choices).

  Styles affect:
  • Background
  • Borders
  • Icons
  • Text and value colors
  • Separators


---------------- [Information Rail Display Mode] ---------------
► Description:
  Selects whether Information Rail information and the Summon Counter are represented using icons or text labels.


-------------- [Inventory Weight and Free Slots] ---------------
► Description:
  Shows current weight / maximum carry weight in a filling bar near the bottom-left HUD.
  Shows the number of free inventory slots.

► Modes:
  WeightAndFreeSlots
  WeightOnly
  FreeSlotsOnly
  Off


------------------------- [Enemy Detector] ---------------------
► Description:
  Counts characters within 30 meters and displays a nearby-enemy counter.

► Modes: 
  Consolidated
  Split
  Off.

  Consolidated combines hostile enemies into a single counter.
  Split separates normal, tough and boss enemies into individual counters.
  Bosses and minibosses count as normal enemies while using Consolidated mode.
  Enemies in the tough category are: Troll, Bjorn, Abomination, Writhan, Stone Golem, Fuling Berserker, Vile Bear, Seeker Soldier, Gjall, Fallen Valkyrie, Morgen, Serpent and Bonemaw
  Other players, Deer, Hare, summoned roots and tamed creatures are excluded from the hostile count.
  Neutral Dvergr use a separate counter until aggravated.


-------------------- [Enemy Detector Position] -----------------
► Description:
  Selects where Enemy Detector information is displayed.

► Modes:
  InfoRail  - displays enemy information inside the bottom-left Information Rail.
  TopCenter - moves enemy information to a separate top-center rail.


---------------------- [Show Summon Counter] --------------------
► Description:
  Shows the number of active skeletons summoned by the Dead Raiser.
  Summoned Trolls are not counted.
  Counter color changes with active summon count.


----------------------- [Skill Progress Bar] --------------------
► Description:
  Displays a temporary full-width progress bar along the bottom of the screen when a skill advances to a new whole percentage toward its next level.
  Skill information also appears temporarily in the Information Rail.
  Run skill progress is intentionally excluded.
  New progress refreshes the current display instead of stacking notifications.

► Modes:
  Gold
  White
  Green
  Blue
  Cyan
  Red
  Purple
  Off


================== [03 - Items & Interaction] ==================

------------- [Show Owned Resources In Build Menu] ------------
► Description:
  Displays resource requirements as Required / Owned.
  Applies to build and crafting requirement displays.

► Compatibility:
  If Craft From Containers is installed, Marsarah UI leaves this display untouched.


-------------------- [Enemy Nameplate Mode] -------------------
► Description:
  Reworks character nameplates with larger health bars, clearer colors and optional health information.
  Nameplate display distance is increased while enabled.
  Bosses, enemies, neutral/tamed characters, normal players and PVP-enabled players use distinct colors.
  Alerted/aggravated state changes creature-name colors.

► Modes:
  BarsOnly
  BarsWithHealth
  BarsWithPercent
  BarsWithBoth
  Off


------------------ [Show Taming Progress] ---------------------
► Description:
  Displays taming percentage beneath a creature's health bar while acclimatizing.
  Independent of Enemy Nameplate Mode.


----------------- [Item Quality Indicator Mode] ---------------
► Description:
  Replaces the vanilla item-quality number with repeated symbols.

► Modes: Horizontal, Vertical, Off



----------------- [Symbol For Item Quality] -------------------
► Symbols:
  Star ★
  Circle ●
  Diamond ◆
  Empty Diamond ◇

► Dependency: Item Quality Indicator Mode


------------------ [Color For Item Quality] -------------------
► Colors:
  White, Yellow, Green, Red, Blue, Cyan

► Dependency: Item Quality Indicator Mode


----------------- [Better Item Durability Bar] ----------------
► Description:
  Replaces the flat durability appearance with a textured bar.
  Color transitions from green through yellow toward red as durability decreases.


==================== [04 - Detailed Hovers] ====================

----------------- [Detailed Hover Information] -----------------
► Description:
  Master setting for additional information shown while hovering supported objects.

► Modes:
  ColoredText - progress/context colors are used.
  WhiteText   - additional information is shown without progress coloring.
  Off         - vanilla hover behavior.

► Supported Object Types:
  Containers
  Beehives
  Plants
  Fermenters
  Cooking Stations and Ovens
  Smelters and similar processing stations
  Eggs


------------------- [Container Contents Mode] ------------------
► Description:
  Lists the actual contents of a chest/container in its hover text.
  Stacks sharing the same display name are combined.
  Up to 10 different item types are shown; additional types are summarized as +X Others.
  Has options to display contents as icons or text.

► Container Access / Privacy:
  Custom container hover information is only displayed when the local player has access to the container.
  Personal Chests owned by other players retain their vanilla hover and do not reveal their contents.
  The owner of a Personal Chest continues to receive the configured Detailed Hover and Container Contents information.

► MarsarahTweaks Compatibility:
  If Progression Halt seals a chest, its real contents are replaced with:
  "This chest is sealed."

► Dependency: Detailed Hover Information


--------------------- [Container Hover Mode] -------------------
► Modes:
  CurrentPerMax       - used slots / total slots.
  AmountOfFreeSlots   - remaining free slots.
  Percent             - percentage of slots used.

► Dependency: Detailed Hover Information


---------------------- [Beehive Hover Mode] --------------------
► Modes: RemainingTime, Percent, PercentAndTime
► Description: Shows honey production progress and current honey amount where applicable.
► Dependency: Detailed Hover Information


----------------------- [Plant Hover Mode] ---------------------
► Modes: RemainingTime, Percent, PercentAndTime
► Description: Shows plant growth progress until ready.
► Dependency: Detailed Hover Information


--------------------- [Fermenter Hover Mode] -------------------
► Modes: RemainingTime, Percent, PercentAndTime
► Description: Shows fermentation progress and current contents while fermenting.
► Dependency: Detailed Hover Information


------------------ [Cooking Station Hover Mode] ----------------
► Modes: RemainingTime, Percent, PercentAndTime
► Description:
  Shows progress for individual occupied cooking slots.
  Ready/overcooking states are handled separately.
  Fuel-based cooking stations can show remaining fuel time where supported.

► Dependency: Detailed Hover Information


--------------------- [Smelter Hover Mode] ---------------------
► Current Mode: RemainingTime
► Description:
  Shows remaining processing time together with relevant queue/fuel information where supported.

► Dependency: Detailed Hover Information


----------------------- [Egg Hover Mode] -----------------------
► Modes: RemainingTime, Percent, PercentAndTime
► Description: Shows hatching progress for supported eggs.
► Dependency: Detailed Hover Information


COMPATIBILITY
================================================================
MarsarahTweaks
----------------------------------------------------------------
Optional soft dependency. Marsarah UI works without MarsarahTweaks.

Smart Biome:
  Accounts for MarsarahTweaks Gear Upgrade Unlock when enabled.

Detailed Hovers / Container Contents:
  Checks MarsarahTweaks Progression Halt. Progression-sealed chests do not reveal their real contents.

Craft From Containers
----------------------------------------------------------------
If detected, Marsarah UI does not alter the Owned Resources requirement display and lets Craft From Containers handle it instead.

Minimal Status Effects
----------------------------------------------------------------
If detected, Boat Speed uses an alternate location to avoid overlap.


FUTURE PLANS
================================================================
Possible future additions include:
- Additional hover display modes, including a Bars mode for processing stations.
- Additional Online Players information.

Deep North-specific UI additions will be reviewed after I have completed the biome myself.

These are development ideas and are not guaranteed to be implemented exactly as described.


FEEDBACK
================================================================
Suggestions and bug reports are welcome on the Posts or Bugs tabs of the Nexusmods page.
Thanks for checking out Marsarah UI!


CREDITS
================================================================
Blaxxun-bloop - ServerSync


VERSION HISTORY
================================================================
v1.1.1
- Updated compatibility with Marsarah Tweaks mod to account for the new configuration naming and ordering.

v1.1.0
- Fixes / Updates:
  - Updated Weather Forecast for Valheim 1.0 BiomeSector / AltBiome weather behavior.
  - Fixed Better Item Durability Bar quickslot mapping when empty quickslots appear before an item, preserving broken-item flashing at zero durability.
  - Fixed restoration of the original durability-bar sprite when Better Item Durability Bar is disabled.

- New UI Features:
  - Added Status Effects Under Minimap.
  - Added Global Chat By Default with server override support.
  - Added Skill Progress Bar and Information Rail skill display.
  - Added Logon Screen Character Statistics with character statistics and conditional notable facts.

- Information Rail:
  - Reworked the bottom-left information area into a unified Information Rail.
  - Inventory Weight, Free Slots, Enemy Detector and Skill Progress now share the rail.
  - Added animated transitions for temporary information.
  - Added three selectable visual styles.
  - Added Icons and Text display modes.
  - Added optional top-center positioning for the Enemy Detector.
  - Summon Counter now follows Information Rail styling and display mode.

- Enemy Detector:
  - Added Consolidated and Split modes.
  - Split mode separates normal, tough and boss enemies.
  - Fixed the normal enemy counter remaining visible at zero enemies.

- Container Contents:
  - Added IconsHorizontal, IconsVertical, Text and Off modes.
  - Added actual item icons and combined stack totals.
  - Added 5x2 and 2x5 icon layouts.
  - Improved Text mode alignment.

- Configuration:
  - Reorganized settings into General & HUD, Information Rail, Items & Interaction, Detailed Hovers and Server Overrides categories.
  - Added explicit setting ordering for compatible Configuration Manager mods without adding a dependency.
  - Added server overrides for Information Rail Display Mode and Global Chat By Default.
  - Config section/key names changed in v1.1.0, so users upgrading from v1.0.0 may need to configure their preferences again.

v1.0.0
- Initial standalone release.

- Standalone UI Mod:
  - Moved the UI features previously included in Marsarah Tweaks into MarsarahUI.
  - Removed the old alternate UI layout system and retained the preferred layout.
  - Added ServerSync support for optional server-side overrides while keeping normal UI settings local.

- Server Overrides:
  - Added optional server overrides for Enemy Detector, Current Day, Current Time, Weather Forecast, Smart Biome, Ashlands Heat Meter, Enemy Nameplate Mode, Taming Progress, Detailed Hover Information and Container Contents.
  - Overrides default to UserChoice so local preferences remain in effect unless explicitly overridden.

- HUD / Information Features:
  - Better Loading Tips with 51 gameplay tips.
  - Inventory Weight and Free Slots.
  - Enemy Detector.
  - Boat Speed.
  - Current Day and Current Time.
  - Weather Forecast with embedded icons.
  - Smart Biome Indicator.
  - Summon Counter.
  - Online Players.
  - Owned Resources display.
  - Boss Power Expiration Message.
  - Ashlands Heat Meter.

- Interface Improvements:
  - Enemy Nameplate display modes.
  - Taming Progress.
  - Item Quality indicator layouts, symbols and colors.
  - Better Item Durability Bar.
  - Detailed Hover Information and optional Container Contents.

- Compatibility:
  - MarsarahTweaks Gear Upgrade Unlock and Progression Halt integration.
  - Craft From Containers compatibility.
  - Minimal Status Effects compatibility.

- Valheim 1.0 Updates:
  - Added Lox Fur to Smart Biome Plains weighting.
  - Expanded Better Loading Tips with additional useful gameplay tips.
