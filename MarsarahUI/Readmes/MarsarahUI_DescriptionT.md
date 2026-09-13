# <strong> Marsarah UI </strong>

**Version:** 1.0.0  
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

## <strong> ⚙️ Configuration </strong>

A config file is generated on first launch:  
`Valheim/BepInEx/config/Marsarah.MarsarahUI.cfg`

The config is divided into two sections:
- **UI Settings (Local):** Individual player preferences. These are not normally synced with the server.
- **Server Overrides (Synced):** Optional server-side controls for selected information-oriented features.

Server Overrides use **UserChoice** by default, which means the player's local setting is respected. Server administrators can change supported options to **ForceOn**, **ForceOff**, or a specific display mode where applicable. The master **Enable Server Overrides** option can disable the override system entirely.

The following features support server overrides:
- Enemy Detector
- Current Day
- Current Time
- Weather Forecast
- Smart Biome
- Ashlands Heat Meter
- Enemy Nameplate Mode
- Taming Progress
- Detailed Hover Information
- Container Contents

**Insert** toggles many of the custom HUD widgets on or off temporarily.  
**Home** toggles the Online Players name list while keeping its online-player count visible.

---

## <strong> 💰 Donations </strong>

My mods are and will always be free to use. If you'd like to support my work, you can donate here:  
https://paypal.me/Marsarah9

---

## <strong> ⚡ Main Features </strong>

### <strong>🔧 Better Loading Tips</strong>
- Replaces the vanilla loading-tip selection with a larger pool of more useful gameplay tips.
- Includes **51 tips** covering controls, progression, combat, building, food, exploration and less-obvious mechanics.
- Intentionally leaves out several very basic vanilla tips in favor of more useful information.
- Only applies when using the **English** localization.

### <strong>🔧 Show Inventory Weight and Free Slots</strong>
- Displays current carry weight and maximum carry weight near the bottom-left HUD.
- Adds a filling weight bar that changes color as the player approaches the weight limit.
- Displays the number of free inventory slots with color feedback.

### <strong>🔧 Show Enemy Detector</strong>
- Displays the number of nearby enemies within roughly **30 meters**.
- Uses different symbols and colors as the number of nearby enemies increases.
- Neutral Dvergr are displayed separately until aggravated.
- Does not count other players, Deer, Hare, summoned roots, or tamed creatures as enemies.

### <strong>🔧 Show Boat Speed</strong>
- Displays current ship speed while controlling a boat.
- Changes color according to current speed. Reverse movement uses an **R** prefix.
- Repositions automatically when **Minimal Status Effects** is installed and accounts for No Map worlds.

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
- Includes mappings for weather used by Seasons and similar environment changes where supported.

### <strong>🔧 Smart Biome Indicator</strong>
- Replaces the normal minimap biome text with a color-coded indicator based on equipped armor and upgrade level.
- Colors progress from **purple** for severely undergeared through red, orange, yellow and **green** for well prepared.
- Evaluates progression from Meadows through Ashlands. Other biomes display in white.
- Includes Bear, Vilebone and Lox Fur armor.
- If **MarsarahTweaks** is installed and **Gear Upgrade Unlock** is enabled, Smart Biome adjusts its expected armor ranges accordingly.

### <strong>🔧 Show Summon Counter</strong>
- Displays the number of active summoned skeletons created by the Dead Raiser.
- Does not count summoned Trolls.

### <strong>🔧 Show Online Players</strong>
- Displays the total number of online players and up to **20 player names** at the bottom-right.
- Hidden when only one player is online.
- **Home** hides/shows the names while retaining the total online count.

### <strong>🔧 Show Owned Resources In Build Menu</strong>
- Displays resource requirements as **required / owned** values in build and crafting requirement displays.
- Example: `2/20` when 2 Wood is required and 20 is owned.
- Automatically steps aside when **Craft From Containers** is installed.

### <strong>🔧 Show Boss Power Expiration Message</strong>
- Displays a center-screen message when an active Forsaken Power expires.

### <strong>🔧 Show Heat Meter in Ashlands</strong>
- Displays a heat meter at the top-center of the screen while relevant Ashlands water/lava heat mechanics are active.

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
- Replaces the flat durability appearance with a textured, gradually colored durability bar.
- Transitions from green through yellow toward red as durability decreases.

### <strong>🔧 Detailed Hover Information</strong>
- Adds additional information to Containers, Beehives, Plants, Fermenters, Cooking Stations/Ovens, Smelters and similar processing stations, and Eggs.
- Master modes: **ColoredText, WhiteText, Off**.
- Most progress-based hovers support remaining time, percentage, or both.

**Container Hover Information:**
- Can show used/max slots, free slots, or percentage filled.
- Optional **Show Container Contents** lists combined item stack amounts directly in the hover.
- Displays up to 10 different item types, followed by a count of additional types when needed.
- If **MarsarahTweaks Progression Halt** seals a chest, its contents are hidden and replaced with **"This chest is sealed."**

---

## <strong> 🔗 Compatibility </strong>

### **MarsarahTweaks**
- Optional soft dependency. Marsarah UI works without Tweaks.
- Smart Biome accounts for **Gear Upgrade Unlock**.
- Detailed Hovers respects **Progression Halt** and will not reveal sealed chest contents.

### **Craft From Containers**
- Marsarah UI steps aside and lets Craft From Containers handle the Owned Resources display.

### **Minimal Status Effects**
- Boat Speed uses an alternate position to avoid overlap.

---

## <strong> 🔮 Future Plans </strong>

Future ideas include additional hover display modes, skill progress information, improved player information, optional item icons for container contents, and additional status-effect/UI improvements.

Deep North-specific UI changes will be reviewed after I have completed the biome myself.

---

## <strong> 💬 Feedback </strong>

Suggestions and bug reports are welcome on the **Posts** or **Bugs** tabs of the Nexusmods page.  
Thanks for checking out Marsarah UI!

## <strong> 🧑‍🤝‍🧑 Credits </strong>
Blaxxun-bloop - for ServerSync

## <strong> 📜 Version History </strong>
Check the Changelog tab.
