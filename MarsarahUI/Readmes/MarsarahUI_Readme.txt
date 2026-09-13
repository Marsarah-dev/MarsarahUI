Marsarah UI v1.0.0
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

The config contains two main sections:

1 - UI Settings (Local)
  Normal player UI preferences. These remain local to each player.

2 - Server Overrides (Synced)
  Optional server controls for selected information-oriented features.

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

- Mode-based overrides can also force a specific display mode where applicable.

Server-overridable features:
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

LOCAL-ONLY SETTINGS
----------------------------------------------------------------
The remaining settings stay under the player's control, including Better Loading Tips, Inventory Weight/Slots, Boat Speed, Summon Counter, Online Players, Owned Resources, Boss Power Expiration, Item Quality, Item Durability, and individual hover display modes.

KEYS
----------------------------------------------------------------
Insert
  Temporarily hides/shows many custom HUD widgets.

Home
  Toggles the Online Players name list. The total Online count remains visible while the list is hidden.


MOD CONFIGS
================================================================

===================== [UI Settings - Local] ====================

--------------------- [01 - Better Loading Tips] ---------------
► Description:
  Replaces the vanilla loading-tip selection with a curated pool of 51 gameplay tips.
  Tips focus on controls, progression, combat, building, food, exploration and less-obvious mechanics.
  Several very basic vanilla tips are intentionally omitted.

► Localization:
  English only.

► Default: Enabled


--------------- [02 - Inventory Weight and Free Slots] ---------
► Description:
  Shows current weight / maximum carry weight in a filling bar near the bottom-left HUD.
  Shows the number of free inventory slots.
  Colors change as the player approaches the weight/slot limits.

► Default: Enabled


------------------- [03 - Enemy Detector] ----------------------
► Description:
  Counts characters within approximately 30 meters and displays a nearby-enemy counter.

  Excluded from the hostile count:
  • Other players
  • Deer
  • Hare
  • Summoned Roots
  • Tamed creatures

  Neutral Dvergr use a separate counter until aggravated.
  Symbols and colors change as the enemy count rises.

► Server Override: Supported
► Default: Enabled


---------------------- [04 - Boat Speed] -----------------------
► Description:
  Displays current ship speed while controlling a boat.
  Reverse speed uses an R prefix.
  Text color changes according to speed.

► Compatibility:
  Alternate positioning for Minimal Status Effects and No Map worlds.

► Default: Enabled


--------------------- [05 - Current Day] -----------------------
► Description:
  Displays the current world day above the minimap.
  Also works in No Map worlds.

► Server Override: Supported
► Default: Enabled


--------------------- [06 - Current Time] ----------------------
► Modes:
  DigitalClock - 24-hour HH:MM display.
  DayPhases    - Night, Dawn, Morning, Day, Afternoon, Evening, Dusk.
  Off          - disables the time display.

  Day-phase text is color coded.
  The display also works in No Map worlds.

► Server Override:
  UserChoice, DigitalClock, DayPhases, or Off.

► Default: DigitalClock


--------------- [07 - Weather Forecast Indicator] -------------
► Description:
  Shows the next scheduled weather using an icon and countdown near the minimap.
  Weather icons are embedded in MarsarahUI.dll.

  The feature searches upcoming environment periods for the next different weather.
  If no different weather is found in the forecast window, the current weather icon is shown with --:--.

► Compatibility:
  Includes mappings for several environment names used by Seasons and similar weather changes where supported.

► Server Override: Supported
► Default: Enabled


------------------- [08 - Smart Biome Indicator] --------------
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

► Recognized newer/special sets include:
  Bear
  Vilebone
  Lox Fur

► MarsarahTweaks Compatibility:
  If Tweaks is installed and Gear Upgrade Unlock is enabled, Smart Biome uses expanded expected armor ranges where Tweaks permits earlier level-4 upgrades.

► Server Override: Supported
► Default: Enabled


--------------------- [09 - Summon Counter] --------------------
► Description:
  Shows the number of active skeletons summoned by the Dead Raiser.
  Summoned Trolls are not counted.
  Counter color changes with active summon count.

► Default: Enabled


------------------- [10 - Online Players] ----------------------
► Description:
  Shows total online players and player names at the bottom-right.
  Hidden when only one player is online.
  Displays up to 20 player names.

► Home Key:
  Hides/shows player names while retaining the total Online count.

► Chat:
  Player names temporarily hide while the chat dialog is open.

► Default: Enabled


------------- [11 - Owned Resources In Build Menu] ------------
► Description:
  Displays resource requirements as Required / Owned.

  Example:
  Required Wood: 2
  Owned Wood:    20
  Display:       2/20

  Applies to build and crafting requirement displays.

► Compatibility:
  If Craft From Containers is installed, Marsarah UI leaves this display untouched.

► Default: Enabled


------------ [12 - Boss Power Expiration Message] -------------
► Description:
  Displays a center-screen message when an active Forsaken Power expires.

► Default: Enabled


-------------- [13 - Ashlands Heat Meter] ---------------------
► Description:
  Displays a heat meter at the top-center while relevant Ashlands water/lava heat mechanics are active.

► Server Override: Supported
► Default: Enabled


----------------- [14 - Enemy Nameplate Mode] -----------------
► Description:
  Reworks character nameplates with larger health bars, clearer colors and optional health information.
  Nameplate display distance is increased while enabled.

► Colors:
  Bosses, enemies, neutral/tamed characters, normal players and PVP-enabled players use distinct colors.
  Alerted/aggravated state changes creature-name colors.

► Modes:
  BarsOnly
  BarsWithHealth
  BarsWithPercent
  BarsWithBoth
  Off

► Server Override: Supported, including specific display modes
► Default: BarsWithHealth


------------------ [15 - Taming Progress] ---------------------
► Description:
  Displays taming percentage beneath a creature's health bar while acclimatizing.
  Independent of Enemy Nameplate Mode.

► Server Override: Supported
► Default: Enabled


-------------- [16 - Item Quality Indicator Mode] -------------
► Description:
  Replaces the vanilla item-quality number with repeated symbols.

► Modes: Horizontal, Vertical, Off
► Default: Horizontal


---------------- [17 - Item Quality Symbol] --------------------
► Symbols:
  Star ★
  Circle ●
  Diamond ◆
  Empty Diamond ◇

► Dependency: Item Quality Indicator Mode
► Default: Star


----------------- [18 - Item Quality Color] --------------------
► Colors:
  White, Yellow, Green, Red, Blue, Cyan

► Dependency: Item Quality Indicator Mode
► Default: Yellow


--------------- [19 - Better Item Durability Bar] -------------
► Description:
  Replaces the flat durability appearance with a textured bar.
  Color transitions from green through yellow toward red as durability decreases.

► Default: Enabled


-------------- [20 - Detailed Hover Information] --------------
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

► Server Override: Supported
► Default: ColoredText


---------------- [21 - Show Container Contents] ----------------
► Description:
  Lists the actual contents of a chest/container in its hover text.
  Stacks sharing the same display name are combined.
  Up to 10 different item types are shown; additional types are summarized as +X Others.

► MarsarahTweaks Compatibility:
  If Progression Halt seals a chest, its real contents are replaced with:
  "This chest is sealed."

► Dependency: Detailed Hover Information
► Server Override: Supported
► Default: Disabled


------------------ [22 - Container Hover Mode] -----------------
► Modes:
  CurrentPerMax       - used slots / total slots.
  AmountOfFreeSlots   - remaining free slots.
  Percent             - percentage of slots used.

► Dependency: Detailed Hover Information
► Default: CurrentPerMax


------------------- [23 - Beehive Hover Mode] ------------------
► Modes: RemainingTime, Percent, PercentAndTime
► Description: Shows honey production progress and current honey amount where applicable.
► Dependency: Detailed Hover Information
► Default: RemainingTime


-------------------- [24 - Plant Hover Mode] -------------------
► Modes: RemainingTime, Percent, PercentAndTime
► Description: Shows plant growth progress until ready.
► Dependency: Detailed Hover Information
► Default: RemainingTime


------------------ [25 - Fermenter Hover Mode] -----------------
► Modes: RemainingTime, Percent, PercentAndTime
► Description: Shows fermentation progress and current contents while fermenting.
► Dependency: Detailed Hover Information
► Default: RemainingTime


--------------- [26 - CookingStation Hover Mode] --------------
► Modes: RemainingTime, Percent, PercentAndTime
► Description:
  Shows progress for individual occupied cooking slots.
  Ready/overcooking states are handled separately.
  Fuel-based cooking stations can show remaining fuel time where supported.

► Dependency: Detailed Hover Information
► Default: RemainingTime


------------------ [27 - Smelter Hover Mode] -------------------
► Current Mode: RemainingTime
► Description:
  Shows remaining processing time together with relevant queue/fuel information where supported.

► Dependency: Detailed Hover Information


-------------------- [28 - Egg Hover Mode] ---------------------
► Modes: RemainingTime, Percent, PercentAndTime
► Description: Shows hatching progress for supported eggs.
► Dependency: Detailed Hover Information
► Default: RemainingTime


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
- Skill progress information.
- Additional Online Players information.
- Player statistics on the login/character screen.
- Optional item icons for container contents.
- Additional status-effect/UI improvements.

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
