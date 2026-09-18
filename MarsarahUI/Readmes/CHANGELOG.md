## <strong> 📜 Version History </strong>

v1.1.0
- **Fixes/Updates:**

- **Better Item Durability Bar:**
  - Fixed durability styling being applied to the wrong quickslot when empty quickslots appeared before an item.
  - Fixed the original durability-bar sprite not being restored correctly when Better Item Durability Bar was disabled.
  
- **Weather Forecast:**
  - Updated weather forecasting for Valheim 1.0's BiomeSector / AltBiome weather system.
  - Added proper alternative-biome forecasting support, including Dark Meadows.

- **New Features:**

- **Information Rail:**
  - Reworked the bottom-left information area into a unified Information Rail.
  - Inventory Weight, Free Slots, Enemy Detector and Skill Progress information now share the rail.
  - Added animated transitions as temporary elements appear and disappear.
  - Added three selectable Information Rail visual styles with different backgrounds, borders, icons and colors.
  - Added Icons and Text display modes.
  - Summon Counter remains a separate display but now follows the selected Information Rail style and Icons/Text display mode.
  - Added a server override for Information Rail Display Mode, allowing servers to use UserChoice, Icons or Text.

- **Enemy Detector:**
  - Added Consolidated and Split display modes.
  - Consolidated mode counts nearby hostile enemies together.
  - Split mode separates normal, tough and boss enemies into individual counters.
  - Bosses and minibosses are included in the normal enemy count while using Consolidated mode.
  - Neutral Dvergr continue to use their own counter until aggravated.
  - Added an option to move the Enemy Detector from the Information Rail to a separate top-center display.
  - The separate Enemy Detector display follows the selected Information Rail visual style and display mode.
  - The normal enemy counter no longer remains visible when no enemies are nearby.

- **Status Effects Under Minimap:**
  - Added an optional compact vertical status-effect layout below the minimap.
  - Sailing wind and control UI is repositioned to the left of the minimap and resized while the layout is active.
  - Boat Speed automatically moves with the adjusted sailing UI.
  - The vanilla layout is restored when the feature is disabled or when playing with No Map enabled.

- **Global Chat By Default:**
  - Added an option that sends normal chat input globally by default.
  - Preserves the player's original capitalization instead of converting Shout messages to uppercase.
  - Explicit chat commands such as /w and /say continue to work normally.
  - Added a synced server override with UserChoice, ForceOn and ForceOff.

- **Skill Progress Bar:**
  - Added a temporary full-width progress bar along the bottom edge of the screen when skill progress crosses a whole percentage toward the next level.
  - Skill level and progress percentage are displayed temporarily in the Information Rail.
  - Supports both Information Rail Icons and Text display modes.
  - Icon mode uses the skill's actual in-game icon.
  - Progress from the same or another skill refreshes the current display instead of stacking notifications.
  - Run skill progress is intentionally excluded.
  - Added selectable bar colors: Gold, White, Green, Blue, Cyan, Red and Purple.
  - Added Off directly to the color selection to disable the feature.

- **Logon Screen Character Statistics:**
  - Added an optional Character Statistics panel to the character selection screen.
  - Displays playtime together with combat, survival, crafting, homestead, travel and exploration statistics for the selected character.
  - Added a collection of conditional notable character facts based on unusual deaths, activities, events and mastered skills.
  - These notable facts are not the same as the vanilla achievements. 
  - Eligible facts are selected randomly when switching characters. 
  - Individual level-100 skill facts are replaced by Master of Everything when every vanilla skill has reached level 100.

- **Container Contents:**
  - Renamed Show Container Contents to Container Contents Mode.
  - Added IconsHorizontal, IconsVertical, Text and Off modes.
  - Icon modes use actual vanilla or modded item icons with combined stack totals.
  - Horizontal mode uses a 5x2 layout and Vertical mode uses a 2x5 layout.
  - Displays up to 10 unique item types with +N Others for additional types.
  - Improved Text mode alignment for item quantities and names.
  - Default mode is now IconsHorizontal.
  - MarsarahTweaks Progression Halt compatibility continues to prevent sealed chest contents from being revealed.
  - Added container access checks so custom hover information and contents are only shown when the local player has permission to access the container.
  - Other players' Personal Chests retain their vanilla hover and do not reveal their contents.

- **Configuration:**
  - Reorganized local settings into General & HUD, Information Rail, Items & Interaction and Detailed Hovers categories.
  - Server Overrides now use their own fifth synchronized category.
  - Removed numeric prefixes from individual setting names while retaining ordered categories.
  - Added Configuration Manager ordering metadata without requiring Configuration Manager as a dependency.
  - Added Information Rail Display Mode to the available server overrides.
  - Added Global Chat By Default to the available server overrides.
  - Note for users updating from v1.0.0: config sections and setting keys were reorganized in v1.1.0, so previous selections will need to be configured again.

v1.0.0
- **Initial standalone release.**

- **Standalone UI Mod:**
  - Moved the UI features previously included in Marsarah Tweaks into the standalone MarsarahUI mod.
  - Removed the old alternate UI layout system and retained the preferred UI layout.
  - Added ServerSync support for optional server-side overrides while keeping normal UI preferences local to each player.

- **Server Overrides:**
  - Added a master Enable Server Overrides setting.
  - Added optional server overrides for Enemy Detector, Current Day, Current Time, Weather Forecast, Smart Biome, Ashlands Heat Meter, Enemy Nameplate Mode, Taming Progress, Detailed Hover Information and Container Contents.
  - Override settings default to UserChoice so players keep their local preferences unless the server explicitly overrides them.

- **HUD / Information Features:**
  - Better Loading Tips with a curated pool of 51 gameplay tips.
  - Inventory Weight and Free Slots.
  - Enemy Detector.
  - Boat Speed.
  - Current Day.
  - Current Time with Digital Clock and Day Phase modes.
  - Weather Forecast with embedded weather icons.
  - Smart Biome Indicator.
  - Summon Counter.
  - Online Players.
  - Show Owned Resources in build and crafting menus.
  - Boss Power Expiration Message.
  - Ashlands Heat Meter.

- **Interface Improvements:**
  - Enemy Nameplate display modes with health values/percentages and improved colors.
  - Taming Progress.
  - Item Quality indicator layouts, symbols and colors.
  - Better Item Durability Bar.
  - Detailed Hover Information for Containers, Beehives, Plants, Fermenters, Cooking Stations/Ovens, Smelters/processing stations and Eggs.
  - Optional Container Contents display.

- **Compatibility:**
  - MarsarahTweaks Smart Biome compatibility for Gear Upgrade Unlock.
  - MarsarahTweaks Progression Halt compatibility prevents sealed chest contents from being revealed.
  - Craft From Containers compatibility for Owned Resources display.
  - Minimal Status Effects compatibility for Boat Speed positioning.

- **Valheim 1.0 Updates:**
  - Added the Lox Fur set to Smart Biome Plains weighting.
  - Better Loading Tips updated with additional useful gameplay tips.
