# <strong> Marsarah UI </strong>

**Version:** 1.1.1  
**Author:** Marsarah

---

## <strong> 🧰 About the Mod </strong>

Marsarah UI is a standalone collection of UI and information improvements for Valheim.  
The features originally started as the UI section of **Marsarah Tweaks** and have now been separated into their own mod so players can use the interface improvements without installing the gameplay tweaks.  

The mod adds useful HUD information, improved nameplates and item indicators, detailed object hovers, better loading tips, weather information, and other interface improvements. Most features can be configured independently.

Marsarah UI also includes **ServerSync** support for selected gameplay/information-oriented UI features. Local UI preferences remain local by default, while server administrators can optionally override selected settings.

---

## **Development Notes**

**Deep North / Spoiler Note:** Deep North-specific UI integration is intentionally limited for now.  
I want to experience the new biome and progression myself before digging through its mechanics in detail, so I can play through it without spoiling the experience for myself.  
After completing the Deep North, I plan to review new gear for Smart Biome and check whether any new mechanics would benefit from additional indicators or hover information.

**AI Usage Disclosure:** The original UI features were developed as part of my MarsarahMod / Marsarah Tweaks projects. AI tools are now used as part of my development workflow for tasks such as debugging, refactoring, compatibility updates, researching game API changes, and documentation.  
Development remains human-directed: I decide what features are added, how they should behave, and I write code, review and test the changes included in releases. The mod's logo was also created using generative AI.

### **Related Marsarah Mods**

- [**MarsarahTweaks**](https://old.thunderstore.io/c/valheim/p/Marsarah/MarsarahTweaks/) - Gameplay, balance, grind-reduction and quality-of-life tweaks. Marsarah UI includes optional compatibility with some Tweaks features.
- [**MarsarahBuildPieces**](https://old.thunderstore.io/c/valheim/p/Marsarah/MarsarahBuildPieces/) - Custom functional and decorative build pieces, including portals, lights and the Mystical Light Ward.
- Neither mod is required to use Marsarah UI.

---

## <strong> 🔒 Permissions </strong>

Reuploading this mod, whether in part or in full, is **not permitted**.  
Anyone is free to take inspiration or implement similar features, but must do so with their own code and assets.

---

## <strong> 🧱 Requirements </strong>

This mod requires **BepInEx for Valheim**, available here:  
https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/

For multiplayer servers using Marsarah UI's Server Overrides, install the mod on the **server** and **all clients**.

---

## <strong> 📦 Installation </strong>

- Unpack the `.zip` file and copy `MarsarahUI.dll` into your `Valheim/BepInEx/plugins` folder.
- Or use a mod manager.

If the server uses Marsarah UI Server Overrides, the mod must also be installed on the server and all connecting clients.

---

<strong> ⚙️ Configuration </strong>

A config file is generated on first launch:
Valheim/BepInEx/config/Marsarah.MarsarahUI.cfg

Most Marsarah UI settings are local and remain under each player's control. Settings can be changed through the config file or a compatible configuration manager, and most update during gameplay without requiring a restart.

Selected information-oriented features also support synchronized Server Overrides. These use UserChoice by default, allowing players to keep their local preferences unless a server administrator explicitly overrides them.

Updating from v1.0.0:
The configuration layout and setting keys were reorganized in v1.1.0. Existing selections from v1.0.0 may need to be configured again after updating.

Insert temporarily toggles many custom HUD widgets.
Home toggles the Online Players name list while keeping the online-player count visible.

---

---

## <strong> 💰 Donations </strong>

My mods are and will always be free to use. If you'd like to support my work, you can donate here:  
https://paypal.me/Marsarah9

---

## <strong> ⚡ General & HUD </strong>

### <strong>🔧 Better Loading Tips</strong>
- Replaces the vanilla loading-tip selection with a larger pool of more useful gameplay tips.
- Includes **51 tips** covering controls, progression, combat, building, food, exploration and less-obvious mechanics.
- Intentionally leaves out several very basic vanilla tips in favor of more useful information.
- Only applies when using the **English** localization.

### <strong>🔧 Show Boat Speed</strong>
- Displays current ship speed while controlling a boat.
- Changes color according to current speed. Reverse movement uses an **R** prefix.
- Repositions automatically when **Status Effects Under Minimap** is enabled and accounts for No Map worlds.

### <strong>🔧 Show Current Day</strong>
- Displays the current world day above the minimap and continues to work in No Map worlds.

### <strong>🔧 Show Current Time</strong>
- Displays the current time above the minimap.
- Modes: **DigitalClock**, **DayPhases**, **Off**.
- DayPhases uses Night, Dawn, Morning, Day, Afternoon, Evening and Dusk.
- Continues to work in No Map worlds.

### <strong>🔧 Show Weather Forecast Indicator</strong>
- Displays the next scheduled weather using an icon and countdown near the minimap.
- Uses weather icons embedded directly in the mod.
- If no different weather is scheduled in the forecast window, the current weather is shown with a `--:--` timer.
- Supports Valheim 1.0 alternative-biome forecasting, including **Dark Meadows**.
- Includes mappings for weather used by Seasons and similar environment changes where supported.

### <strong>🔧 Smart Biome Indicator</strong>
- Replaces the normal minimap biome text with a color-coded indicator based on equipped armor and upgrade level.
- Colors progress from **purple** for severely undergeared through red, orange, yellow and **green** for well prepared.
- Evaluates progression from Meadows through Ashlands. Other biomes display in white.
- Includes Bear, Vilebone and Lox Fur armor.
- If **MarsarahTweaks** is installed and **Gear Upgrade Unlock** is enabled, Smart Biome adjusts its expected armor ranges accordingly.

### <strong>🔧 Show Online Players</strong>
- Displays the total number of online players and up to **20 player names** at the bottom-right.
- Hidden when only one player is online.
- **Home** hides/shows the names while retaining the total online count.

### <strong>🔧 Show Boss Power Expiration Message</strong>
- Displays a center-screen message when an active Forsaken Power expires.

### <strong>🔧 Show Heat Meter in Ashlands</strong>
- Displays a heat meter at the top-center of the screen while relevant Ashlands water/lava heat mechanics are active.

### <strong>🔧 Status Effects Under Minimap</strong>
- Moves status effects into a compact vertical layout below the minimap.
- Repositions and resizes the sailing wind indicator and sailing controls to avoid overlap.
- Boat Speed automatically moves with the adjusted sailing UI.
- Automatically restores the vanilla layout when disabled or when No Map mode is active.

### <strong>🔧 Global Chat By Default</strong>
- Makes normal chat messages global by default without requiring the player to manually select Shout.
- Preserves the player's original capitalization.
- Explicit commands such as **/w and /say**
- Supports an optional synced server override.

### <strong>🔧 Logon Screen Character Statistics</strong>
- Adds an optional statistics panel to the character selection screen.
- Displays **19 character statistics** covering playtime, combat, survival, crafting, homestead activities, travel and exploration.
- Also displays a conditional notable fact based on the selected character's history. This is not to be confused with the vanilla achievements.
- List of facts:
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
  - Activity & Events facts
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


## <strong> ⚡ Information Rail </strong>

### <strong>🔧 Information Rail</strong>
- Adds a unified information area at the bottom-left of the HUD.
- Inventory Weight, Free Slots, Enemy Detector and temporary Skill Progress information use the Information Rail.
- Temporary elements animate smoothly into and out of the rail as they become relevant.
- Includes **3 selectable visual styles** with different backgrounds, borders, icons and colors.
- Display Mode can use **Icons** or **Text**.
- The Enemy Detector can optionally be moved out of the rail to a separate top-center display.
- The Summon Counter remains separate but follows the selected Information Rail style and Icons/Text mode.

### <strong>🔧 Inventory Weight and Free Slots</strong>
- Displays inventory information inside the Information Rail.
- Modes: **WeightAndFreeSlots, WeightOnly, FreeSlotsOnly, Off**.
- Weight uses a filling bar and current/max weight display.
- Free Slots shows remaining inventory slots.
- Colors and presentation follow the selected Information Rail style.

### <strong>🔧 Enemy Detector</strong>
- Counts nearby characters within **30 meters**.
- Modes: **Consolidated, Split, Off**.
- Consolidated combines hostile enemies into a single counter.
- Split separates normal, tough and boss enemies into individual counters.
- Bosses and minibosses count as normal enemies while using Consolidated mode.
- Neutral Dvergr are shown separately until aggravated.
- Other players, Deer, Hare, summoned roots, T.W.I.G. and tamed creatures are excluded from the hostile count.
- Enemies in the tough category are: Troll, Bjorn, Abomination, Writhan, Stone Golem, Fuling Berserker, Vile Bear, Seeker Soldier, Gjall, Fallen Valkyrie, Morgen, Serpent and Bonemaw
- Can be displayed inside the Information Rail or moved to a separate top-center display.
- Uses the selected Information Rail visual style and Icons/Text display mode.

### <strong>🔧 Show Summon Counter</strong>
- Displays the number of active summoned skeletons created by the Dead Raiser.
- Does not count summoned Trolls.
- Appears only while summons are active.
- Uses the selected Information Rail visual style and Icons/Text display mode.

### <strong>🔧 Skill Progress Bar</strong>
- Displays a temporary progress bar along the bottom edge of the screen when a skill advances to a new whole percentage toward its next level.
- Skill level and progress percentage appears temporarily in the Information Rail.
- Icons mode uses the skill's actual in-game icon; Text mode displays the skill name.
- Run skill progress is intentionally excluded.
- Colors: **Gold, White, Green, Blue, Cyan, Red, Purple**.
- Selecting **Off** disables the feature.


## <strong> ⚡ Items & Interaction</strong>

### <strong>🔧 Show Owned Resources In Build Menu</strong>
- Displays resource requirements as **required / owned** values in build and crafting requirement displays.
- Example: `2/20` when 2 Wood is required and 20 is owned.
- Automatically steps aside when **Craft From Containers** is installed.

### <strong>🔧 Enemy Nameplate Mode</strong>
- Reworks character nameplates with larger health bars, clearer colors and increased display distance.
- Bosses, enemies, neutral/tamed characters, normal players and PVP-enabled players use distinct colors.
- Alerted and aggravated states change creature-name colors.
- Modes: **BarsOnly, BarsWithHealth, BarsWithPercent, BarsWithBoth, Off**.

### <strong>🔧 Show Taming Progress</strong>
- Displays the current taming percentage beneath the creature's health bar while an animal is acclimatizing.
- Works independently of Enemy Nameplate Mode.

### <strong>🔧 Item Quality Indicator</strong>
- Replaces the vanilla item-quality number with repeated symbols.
- Layouts: **Horizontal, Vertical, Off**.
- Symbols: **Star ★, Circle ●, Diamond ◆, Empty Diamond ◇**.
- Colors: **White, Yellow, Green, Red, Blue, Cyan**.

### <strong>🔧 Better Item Durability Bar</strong>
- Uses a textured durability bar that transitions from green through yellow toward red as durability decreases.



## <strong> ⚡ Detailed Hovers</strong>

### <strong>🔧 Detailed Hover Information</strong>
- Adds additional information to Containers, Beehives, Plants, Fermenters, Cooking Stations/Ovens, Smelters and similar processing stations, and Eggs.
- Master modes: **ColoredText, WhiteText, Off**.
- Most progress-based hovers support remaining time, percentage, or both.

**Container Hover Information:**
- Can show used/max slots, free slots, or percentage filled.
- **Container Contents Mode** supports:
  - **IconsHorizontal** - actual item icons in a 5x2 layout.
  - **IconsVertical** - actual item icons in a 2x5 layout.
  - **Text** - aligned item quantities and names.
  - **Off** - disables contents display.
- Multiple stacks of the same item are combined.
- Displays up to 10 different item types with **+N Others** for additional types.
- If **MarsarahTweaks Progression Halt** seals a chest, its contents are hidden and replaced with **"This chest is sealed."**
- Custom container information and contents are only shown when the local player has access to the container.
- Containers protected by another player's Ward do not reveal their contents.
- Other players' **Personal Chests** retain their vanilla hover and do not reveal their contents.

---

## <strong> 🔗 Compatibility </strong>

### **MarsarahTweaks**
- Optional soft dependency. Marsarah UI works without Tweaks.
- Smart Biome accounts for **Gear Upgrade Unlock**.
- Detailed Hovers / Container Contents respects **Progression Halt** and will not reveal sealed chest contents.

### **Craft From Containers**
- Marsarah UI steps aside and lets Craft From Containers handle the Owned Resources display.

---

## <strong> 🔮 Future Plans </strong>

Future ideas include additional hover display modes, improved Online Players information, additional UI indicators, and other interface improvements.

Deep North-specific UI changes will be reviewed after I have completed the biome myself.

---

## <strong> 💬 Feedback </strong>

Suggestions and bug reports are welcome on the **Posts** or **Bugs** tabs of the Nexusmods page.  
Thanks for checking out Marsarah UI!

## <strong> 🧑‍🤝‍🧑 Credits </strong>
Blaxxun-bloop - for ServerSync

## <strong> 📜 Version History </strong>
Check the Changelog tab.
