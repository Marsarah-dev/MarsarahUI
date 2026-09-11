using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;
using ServerSync;

using MarsarahUI.Patches.UI;

namespace MarsarahUI.Managers
{
	public static class ConfigManager
	{
		private static readonly LogManager log = new LogManager("Config Manager", LogManager.LogLevel.Warning);

		private static ConfigFile Config;
		private static readonly ConfigSync configSync = new ConfigSync(MarsarahUI.ModGUID)
		{
			DisplayName = MarsarahUI.ModName,
			CurrentVersion = MarsarahUI.ModVersion,
			MinimumRequiredVersion = MarsarahUI.ModVersion
		};
		private static FileSystemWatcher watcher;

		private static string ConfigFileName => MarsarahUI.ModGUID + ".cfg";
		private static string ConfigFileFullPath => Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

		public static class ConfigSections
		{
			public const string UI = "1 - UI Settings (Local)";
			public const string ServerOverrides = "2 - Server Overrides (Synced)";
		}

		public struct ConfigMetadata
		{
			public string Name;
			public string Description;

			public ConfigMetadata(string name, string description)
			{
				Name = name;
				Description = description;
			}
		}

		public enum TimeMode
		{
			DigitalClock,
			DayPhases,
			Off
		}

		public enum OnlinePlayersMode
		{
			BottomRight,
			UnderMinimap,
			Off
		}

		public enum EnemyNameplateMode
		{
			BarsOnly,
			BarsWithHealth,
			BarsWithPercent,
			BarsWithBoth,
			Off
		}

		public enum ItemQualityMode
		{
			Horizontal,
			Vertical,
			Off
		}

		public enum ItemQualitySymbol
		{
			Star,
			Circle,
			Diamond,
			EmptyDiamond
		}

		public enum ItemQualityColor
		{
			White,
			Yellow,
			Green,
			Red,
			Blue,
			Cyan
		}

		public enum HoverInfoMode
		{
			ColoredText,
			WhiteText,
			Off
		}

		public enum ContainerHoverMode
		{
			CurrentPerMax,
			AmountOfFreeSlots,
			Percent
		}

		public enum BeeHoverMode
		{
			RemainingTime,
			Percent,
			PercentAndTime
		}

		public enum PlantHoverMode
		{
			RemainingTime,
			Percent,
			PercentAndTime
		}

		public enum FermenterHoverMode
		{
			RemainingTime,
			Percent,
			PercentAndTime
		}

		public enum CookingStationHoverMode
		{
			RemainingTime,
			Percent,
			PercentAndTime
		}

		public enum SmelterHoverMode
		{
			RemainingTime
		}

		public enum EggHoverMode
		{
			RemainingTime,
			Percent,
			PercentAndTime
		}

		public enum BoolOverride
		{
			UserChoice,
			ForceOn,
			ForceOff
		}

		public enum TimeModeOverride
		{
			UserChoice,
			DigitalClock,
			DayPhases,
			Off
		}

		public enum OnlinePlayersModeOverride
		{
			UserChoice,
			BottomRight,
			UnderMinimap,
			Off
		}

		public enum EnemyNameplateModeOverride
		{
			UserChoice,
			BarsOnly,
			BarsWithHealth,
			BarsWithPercent,
			BarsWithBoth,
			Off
		}

		public enum ItemQualityModeOverride
		{
			UserChoice,
			Horizontal,
			Vertical,
			Off
		}

		public enum ItemQualitySymbolOverride
		{
			UserChoice,
			Star,
			Circle,
			Diamond,
			EmptyDiamond
		}

		public enum ItemQualityColorOverride
		{
			UserChoice,
			White,
			Yellow,
			Green,
			Red,
			Blue,
			Cyan
		}

		public enum HoverInfoModeOverride
		{
			UserChoice,
			ColoredText,
			WhiteText,
			Off
		}

		public enum ContainerHoverModeOverride
		{
			UserChoice,
			CurrentPerMax,
			AmountOfFreeSlots,
			Percent
		}

		public enum BeeHoverModeOverride
		{
			UserChoice,
			RemainingTime,
			Percent,
			PercentAndTime
		}

		public enum PlantHoverModeOverride
		{
			UserChoice,
			RemainingTime,
			Percent,
			PercentAndTime
		}

		public enum FermenterHoverModeOverride
		{
			UserChoice,
			RemainingTime,
			Percent,
			PercentAndTime
		}

		public enum CookingStationHoverModeOverride
		{
			UserChoice,
			RemainingTime,
			Percent,
			PercentAndTime
		}

		public enum SmelterHoverModeOverride
		{
			UserChoice,
			RemainingTime
		}

		public enum EggHoverModeOverride
		{
			UserChoice,
			RemainingTime,
			Percent,
			PercentAndTime
		}

		public static class Configs
		{
			public static readonly ConfigMetadata LockServerOverrides = new ConfigMetadata("01 - Lock Server Overrides", "If on, only server admins can change Server Override settings. Local UI settings are never affected.");
			public static readonly ConfigMetadata EnableServerOverrides = new ConfigMetadata("02 - Enable Server Overrides", "If on, the server can override selected UI settings for connected players. Settings left as UserChoice continue to use each player's local UI preference.");
			public static readonly ConfigMetadata OverrideMoreLoadingTips = new ConfigMetadata("03 - More Loading Tips Override", "Overrides the player's local More Loading Tips setting.");
			public static readonly ConfigMetadata OverrideInventoryWeightAndSlots = new ConfigMetadata("04 - Inventory Weight and Free Slots Override", "Overrides the player's local Inventory Weight and Free Slots setting.");
			public static readonly ConfigMetadata OverrideEnemyDetector = new ConfigMetadata("05 - Enemy Detector Override", "Overrides the player's local Enemy Detector setting.");
			public static readonly ConfigMetadata OverrideBoatSpeed = new ConfigMetadata("06 - Boat Speed Override", "Overrides the player's local Boat Speed setting.");
			public static readonly ConfigMetadata OverrideCurrentDay = new ConfigMetadata("07 - Current Day Override", "Overrides the player's local Current Day setting.");
			public static readonly ConfigMetadata OverrideCurrentTime = new ConfigMetadata("08 - Current Time Override", "Overrides the player's local Current Time setting.");
			public static readonly ConfigMetadata OverrideWeatherForecast = new ConfigMetadata("09 - Weather Forecast Override", "Overrides the player's local Weather Forecast setting.");
			public static readonly ConfigMetadata OverrideSmartBiome = new ConfigMetadata("10 - Smart Biome Override", "Overrides the player's local Smart Biome setting.");
			public static readonly ConfigMetadata OverrideSummonCounter = new ConfigMetadata("11 - Summon Counter Override", "Overrides the player's local Summon Counter setting.");
			public static readonly ConfigMetadata OverrideOnlinePlayers = new ConfigMetadata("12 - Online Players Override", "Overrides the player's local Online Players setting.");
			public static readonly ConfigMetadata OverrideOwnedResources = new ConfigMetadata("13 - Owned Resources Override", "Overrides the player's local Owned Resources setting.");
			public static readonly ConfigMetadata OverrideBossExpiration = new ConfigMetadata("14 - Boss Power Expiration Override", "Overrides the player's local Boss Power Expiration setting.");
			public static readonly ConfigMetadata OverridePlayerLogout = new ConfigMetadata("15 - Player Logout Announce Override", "Overrides the player's local Player Logout Announce setting.");
			public static readonly ConfigMetadata OverrideAshlandsHeat = new ConfigMetadata("16 - Ashlands Heat Meter Override", "Overrides the player's local Ashlands Heat Meter setting.");
			public static readonly ConfigMetadata OverrideEnemyNameplates = new ConfigMetadata("17 - Enemy Nameplate Mode Override", "Overrides the player's local Enemy Nameplate Mode setting.");
			public static readonly ConfigMetadata OverrideTamingProgress = new ConfigMetadata("18 - Taming Progress Override", "Overrides the player's local Taming Progress setting.");
			public static readonly ConfigMetadata OverrideItemQualityMode = new ConfigMetadata("19 - Item Quality Indicator Mode Override", "Overrides the player's local Item Quality Indicator Mode setting.");
			public static readonly ConfigMetadata OverrideItemQualitySymbol = new ConfigMetadata("20 - Item Quality Symbol Override", "Overrides the player's local Item Quality Symbol setting.");
			public static readonly ConfigMetadata OverrideItemQualityColor = new ConfigMetadata("21 - Item Quality Color Override", "Overrides the player's local Item Quality Color setting.");
			public static readonly ConfigMetadata OverrideItemDurability = new ConfigMetadata("22 - Item Durability Bar Override", "Overrides the player's local Item Durability Bar setting.");
			public static readonly ConfigMetadata OverrideDetailedHovers = new ConfigMetadata("23 - Detailed Hover Information Override", "Overrides the player's local Detailed Hover Information setting.");
			public static readonly ConfigMetadata OverrideContainerContents = new ConfigMetadata("24 - Container Contents Override", "Overrides the player's local Container Contents setting.");
			public static readonly ConfigMetadata OverrideContainerHover = new ConfigMetadata("25 - Container Hover Mode Override", "Overrides the player's local Container Hover Mode setting.");
			public static readonly ConfigMetadata OverrideBeeHover = new ConfigMetadata("26 - Beehive Hover Mode Override", "Overrides the player's local Beehive Hover Mode setting.");
			public static readonly ConfigMetadata OverridePlantHover = new ConfigMetadata("27 - Plant Hover Mode Override", "Overrides the player's local Plant Hover Mode setting.");
			public static readonly ConfigMetadata OverrideFermenterHover = new ConfigMetadata("28 - Fermenter Hover Mode Override", "Overrides the player's local Fermenter Hover Mode setting.");
			public static readonly ConfigMetadata OverrideCookingStationHover = new ConfigMetadata("29 - Cooking Station Hover Mode Override", "Overrides the player's local Cooking Station Hover Mode setting.");
			public static readonly ConfigMetadata OverrideSmelterHover = new ConfigMetadata("30 - Smelter Hover Mode Override", "Overrides the player's local Smelter Hover Mode setting.");
			public static readonly ConfigMetadata OverrideEggHover = new ConfigMetadata("31 - Egg Hover Mode Override", "Overrides the player's local Egg Hover Mode setting.");

			public static readonly ConfigMetadata UIMoreLoadingTips = new ConfigMetadata("01 - More Loading Tips", "More loading screen tips");
			public static readonly ConfigMetadata UIInventoryWeightAndSlots = new ConfigMetadata("02 - Show Inventory Weight and Free Slots", "Shows inventory weight and free slots on the bottom left of the screen");
			public static readonly ConfigMetadata UIEnemyDetector = new ConfigMetadata("03 - Show Enemy Detector", "Shows enemy detector on the bottom left of the screen");
			public static readonly ConfigMetadata UIBoatSpeed = new ConfigMetadata("04 - Show Boat Speed", "Shows boat speed when using a boat next to the sail indicator");
			public static readonly ConfigMetadata UICurrentDay = new ConfigMetadata("05 - Show Current Day", "Shows the current day above the minimap.");
			public static readonly ConfigMetadata UITimeMode = new ConfigMetadata("06 - Show Current Time", "Shows the current time above the minimap. Can choose between digital clock and day sections");
			public static readonly ConfigMetadata UIWeatherForecast = new ConfigMetadata("07 - Show Weather Forecast Indicator", "Shows the next scheduled weather as an icon at the bottom-right of the minimap and the remaining time to that weather.");
			public static readonly ConfigMetadata UISmartBiome = new ConfigMetadata("08 - Smart Biome Indicator", "Shows smart biome text on the minimap (colored according to worn armor relative to current biome)");
			public static readonly ConfigMetadata UISummonCounter = new ConfigMetadata("09 - Show Summon Counter", "Shows number of summoned skeletons from the Dead Raiser");
			public static readonly ConfigMetadata UIOnlinePlayersMode = new ConfigMetadata("10 - Show Online Players", "Displays a list of online players. The player names can be toggled with the Home key. (Not displayed if only one player is online)");
			public static readonly ConfigMetadata UIShowOwnedResources = new ConfigMetadata("11 - Show Owned Resources In Build Menu", "Displays the total amount of resources in the player's inventory in addition to the required resource amount for the selected piece or recipe in the build or crafting menu");
			public static readonly ConfigMetadata UIShowPowerExpiration = new ConfigMetadata("12 - Show Boss Power Expiration Message", "Displays a message in the center of the screen when any Forsaken Power expires");
			public static readonly ConfigMetadata UIPlayerLogoutAnnounce = new ConfigMetadata("13 - Player Logout Announce", "Displays a message when a player logs out in the top-left corner of the screen and in the chat window");
			public static readonly ConfigMetadata UIAshlandsHeatLevel = new ConfigMetadata("14 - Show Heat Meter in Ashlands", "Shows a heat meter at the top-center of the screen when in Ashlands water or lava");
			public static readonly ConfigMetadata UIEnemyNameplateMode = new ConfigMetadata("15 - Enemy Nameplate Mode", "Changes the way enemy nameplates are displayed by changing bar style and colors and alerted/aggravated status. Has different ways of showing HP");
			public static readonly ConfigMetadata UITamingProgress = new ConfigMetadata("16 - Show Taming Progress", "Displays current taming percentage of animals that are acclamatizing under the HP bar. This is independent of Enemy Nameplate Mode");
			public static readonly ConfigMetadata UIItemQualityIndicatorMode = new ConfigMetadata("17 - Item Quality Indicator Mode", "Changes the way item quality is displayed by converting the vanilla number to symbols.");
			public static readonly ConfigMetadata UIItemQualitySymbol = new ConfigMetadata("18 - Symbol For Item Quality", "Choose the symbol used for the item quality indicator. Requires Item Quality Indicator Mode to be enabled");
			public static readonly ConfigMetadata UIItemQualityColor = new ConfigMetadata("19 - Color For Item Quality", "Choose the color used for the item quality indicator. Requires Item Quality Indicator Mode to be enabled");
			public static readonly ConfigMetadata UIItemDurabilityColor = new ConfigMetadata("20 - Better Item Durability Bar", "Colors the item durability bar according to current durability and modifies the sprite texture");
			public static readonly ConfigMetadata UIHoverInfoMode = new ConfigMetadata("21 - Detailed Hover Information", "Adds more information when hovering over objects. Master toggle for the configs below (23-30)");
			public static readonly ConfigMetadata UIContainerContents = new ConfigMetadata("22 - Show Container Contents", "Show the contents of a chest or container when hovering. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UIContainerHoverMode = new ConfigMetadata("23 - Container Hover Mode", "Choose the method of displaying Container hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UIBeeHoverMode = new ConfigMetadata("24 - Beehive Hover Mode", "Choose the method of displaying Beehive hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UIPlantHoverMode = new ConfigMetadata("25 - Plant Hover Mode", "Choose the method of displaying Plant hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UIFermenterHoverMode = new ConfigMetadata("26 - Fermenter Hover Mode", "Choose the method of displaying Fermenter hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UICookingStationHoverMode = new ConfigMetadata("27 - CookingStation Hover Mode", "Choose the method of displaying Cooking Station hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UISmelterHoverMode = new ConfigMetadata("28 - Smelter Hover Mode", "Choose the method of displaying Smelter hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UIEggHoverMode = new ConfigMetadata("29 - Egg Hover Mode", "Choose the method of displaying Egg hatching hover info. Requires Detailed Hover Information");
		}

		public static ConfigEntry<bool> ServerOverridesLocked;
		public static ConfigEntry<bool> ServerOverridesEnabled;
		public static ConfigEntry<BoolOverride> MoreLoadingTipsOverride;
		public static ConfigEntry<BoolOverride> InventoryWeightAndSlotsOverride;
		public static ConfigEntry<BoolOverride> EnemyDetectorOverride;
		public static ConfigEntry<BoolOverride> BoatSpeedOverride;
		public static ConfigEntry<BoolOverride> CurrentDayOverride;
		public static ConfigEntry<TimeModeOverride> CurrentTimeOverride;
		public static ConfigEntry<BoolOverride> WeatherForecastOverride;
		public static ConfigEntry<BoolOverride> SmartBiomeOverride;
		public static ConfigEntry<BoolOverride> SummonCounterOverride;
		public static ConfigEntry<OnlinePlayersModeOverride> OnlinePlayersOverride;
		public static ConfigEntry<BoolOverride> OwnedResourcesOverride;
		public static ConfigEntry<BoolOverride> BossExpirationOverride;
		public static ConfigEntry<BoolOverride> PlayerLogoutOverride;
		public static ConfigEntry<BoolOverride> AshlandsHeatOverride;
		public static ConfigEntry<EnemyNameplateModeOverride> EnemyNameplatesOverride;
		public static ConfigEntry<BoolOverride> TamingProgressOverride;
		public static ConfigEntry<ItemQualityModeOverride> ItemQualityModeOverrideChoice;
		public static ConfigEntry<ItemQualitySymbolOverride> ItemQualitySymbolOverrideChoice;
		public static ConfigEntry<ItemQualityColorOverride> ItemQualityColorOverrideChoice;
		public static ConfigEntry<BoolOverride> ItemDurabilityOverride;
		public static ConfigEntry<HoverInfoModeOverride> DetailedHoversOverride;
		public static ConfigEntry<BoolOverride> ContainerContentsOverride;
		public static ConfigEntry<ContainerHoverModeOverride> ContainerHoverOverride;
		public static ConfigEntry<BeeHoverModeOverride> BeeHoverOverride;
		public static ConfigEntry<PlantHoverModeOverride> PlantHoverOverride;
		public static ConfigEntry<FermenterHoverModeOverride> FermenterHoverOverride;
		public static ConfigEntry<CookingStationHoverModeOverride> CookingStationHoverOverride;
		public static ConfigEntry<SmelterHoverModeOverride> SmelterHoverOverride;
		public static ConfigEntry<EggHoverModeOverride> EggHoverOverride;

		public static ConfigEntry<bool> MoreLoadingTipsEnabled;
		public static ConfigEntry<bool> ShowInventoryWeightAndSlots;
		public static ConfigEntry<bool> ShowEnemyDetector;
		public static ConfigEntry<bool> ShowBoatSpeed;
		public static ConfigEntry<bool> ShowCurrentDay;
		public static ConfigEntry<TimeMode> TimeChoice;
		public static ConfigEntry<bool> ShowWeatherForecast;
		public static ConfigEntry<bool> ShowSmartBiome;
		public static ConfigEntry<bool> ShowSummonCounter;
		public static ConfigEntry<OnlinePlayersMode> OnlinePlayersChoice;
		public static ConfigEntry<bool> ShowOwnedResources;
		public static ConfigEntry<bool> ShowBossExpirationMessage;
		public static ConfigEntry<bool> AnnouncePlayerLogout;
		public static ConfigEntry<bool> ShowHeatLevelInAshlands;
		public static ConfigEntry<EnemyNameplateMode> EnemyNameplateChoice;
		public static ConfigEntry<bool> ShowTamingProgress;
		public static ConfigEntry<ItemQualityMode> ItemQualityIndicatorChoice;
		public static ConfigEntry<ItemQualitySymbol> ItemQualitySymbolChoice;
		public static ConfigEntry<ItemQualityColor> ItemQualityColorChoice;
		public static ConfigEntry<bool> ColoredItemDurabilityBar;
		public static ConfigEntry<HoverInfoMode> DetailedHoverInfoChoice;
		public static ConfigEntry<bool> ShowContainerContents;
		public static ConfigEntry<ContainerHoverMode> ContainerHoverModeChoice;
		public static ConfigEntry<BeeHoverMode> BeehiveHoverModeChoice;
		public static ConfigEntry<PlantHoverMode> PlantHoverModeChoice;
		public static ConfigEntry<FermenterHoverMode> FermenterHoverModeChoice;
		public static ConfigEntry<CookingStationHoverMode> CookingStationHoverModeChoice;
		public static ConfigEntry<SmelterHoverMode> SmelterHoverModeChoice;
		public static ConfigEntry<EggHoverMode> EggHoverModeChoice;

		// Effective UI settings
		public static bool EffectiveMoreLoadingTipsEnabled => ResolveBool(MoreLoadingTipsEnabled, MoreLoadingTipsOverride);
		public static bool EffectiveShowInventoryWeightAndSlots => ResolveBool(ShowInventoryWeightAndSlots, InventoryWeightAndSlotsOverride);
		public static bool EffectiveShowEnemyDetector => ResolveBool(ShowEnemyDetector, EnemyDetectorOverride);
		public static bool EffectiveShowBoatSpeed => ResolveBool(ShowBoatSpeed, BoatSpeedOverride);
		public static bool EffectiveShowCurrentDay => ResolveBool(ShowCurrentDay, CurrentDayOverride);
		public static TimeMode EffectiveTimeChoice => ResolveEnum(TimeChoice, CurrentTimeOverride, TimeModeOverride.UserChoice);
		public static bool EffectiveShowWeatherForecast => ResolveBool(ShowWeatherForecast, WeatherForecastOverride);
		public static bool EffectiveShowSmartBiome => ResolveBool(ShowSmartBiome, SmartBiomeOverride);
		public static bool EffectiveShowSummonCounter => ResolveBool(ShowSummonCounter, SummonCounterOverride);
		public static OnlinePlayersMode EffectiveOnlinePlayersChoice => ResolveEnum(OnlinePlayersChoice, OnlinePlayersOverride, OnlinePlayersModeOverride.UserChoice);
		public static bool EffectiveShowOwnedResources => ResolveBool(ShowOwnedResources, OwnedResourcesOverride);
		public static bool EffectiveShowBossExpirationMessage => ResolveBool(ShowBossExpirationMessage, BossExpirationOverride);
		public static bool EffectiveAnnouncePlayerLogout => ResolveBool(AnnouncePlayerLogout, PlayerLogoutOverride);
		public static bool EffectiveShowHeatLevelInAshlands => ResolveBool(ShowHeatLevelInAshlands, AshlandsHeatOverride);
		public static EnemyNameplateMode EffectiveEnemyNameplateChoice => ResolveEnum(EnemyNameplateChoice, EnemyNameplatesOverride, EnemyNameplateModeOverride.UserChoice);
		public static bool EffectiveShowTamingProgress => ResolveBool(ShowTamingProgress, TamingProgressOverride);
		public static ItemQualityMode EffectiveItemQualityIndicatorChoice => ResolveEnum(ItemQualityIndicatorChoice, ItemQualityModeOverrideChoice, ItemQualityModeOverride.UserChoice);
		public static ItemQualitySymbol EffectiveItemQualitySymbolChoice => ResolveEnum(ItemQualitySymbolChoice, ItemQualitySymbolOverrideChoice, ItemQualitySymbolOverride.UserChoice);
		public static ItemQualityColor EffectiveItemQualityColorChoice => ResolveEnum(ItemQualityColorChoice, ItemQualityColorOverrideChoice, ItemQualityColorOverride.UserChoice);
		public static bool EffectiveColoredItemDurabilityBar => ResolveBool(ColoredItemDurabilityBar, ItemDurabilityOverride);
		public static HoverInfoMode EffectiveDetailedHoverInfoChoice => ResolveEnum(DetailedHoverInfoChoice, DetailedHoversOverride, HoverInfoModeOverride.UserChoice);
		public static bool EffectiveShowContainerContents => ResolveBool(ShowContainerContents, ContainerContentsOverride);
		public static ContainerHoverMode EffectiveContainerHoverModeChoice => ResolveEnum(ContainerHoverModeChoice, ContainerHoverOverride, ContainerHoverModeOverride.UserChoice);
		public static BeeHoverMode EffectiveBeehiveHoverModeChoice => ResolveEnum(BeehiveHoverModeChoice, BeeHoverOverride, BeeHoverModeOverride.UserChoice);
		public static PlantHoverMode EffectivePlantHoverModeChoice => ResolveEnum(PlantHoverModeChoice, PlantHoverOverride, PlantHoverModeOverride.UserChoice);
		public static FermenterHoverMode EffectiveFermenterHoverModeChoice => ResolveEnum(FermenterHoverModeChoice, FermenterHoverOverride, FermenterHoverModeOverride.UserChoice);
		public static CookingStationHoverMode EffectiveCookingStationHoverModeChoice => ResolveEnum(CookingStationHoverModeChoice, CookingStationHoverOverride, CookingStationHoverModeOverride.UserChoice);
		public static SmelterHoverMode EffectiveSmelterHoverModeChoice => ResolveEnum(SmelterHoverModeChoice, SmelterHoverOverride, SmelterHoverModeOverride.UserChoice);
		public static EggHoverMode EffectiveEggHoverModeChoice => ResolveEnum(EggHoverModeChoice, EggHoverOverride, EggHoverModeOverride.UserChoice);

		public static void Init(ConfigFile configFile)
		{
			Config = configFile;

			// ===== Local UI Settings
			MoreLoadingTipsEnabled = CreateConfig(Configs.UIMoreLoadingTips, true);
			ShowInventoryWeightAndSlots = CreateConfig(Configs.UIInventoryWeightAndSlots, true);
			ShowEnemyDetector = CreateConfig(Configs.UIEnemyDetector, true);
			ShowBoatSpeed = CreateConfig(Configs.UIBoatSpeed, true);
			ShowCurrentDay = CreateConfig(Configs.UICurrentDay, true);
			TimeChoice = CreateConfig(Configs.UITimeMode, TimeMode.DigitalClock);
			ShowWeatherForecast = CreateConfig(Configs.UIWeatherForecast, true);
			ShowSmartBiome = CreateConfig(Configs.UISmartBiome, true);
			ShowSummonCounter = CreateConfig(Configs.UISummonCounter, true);
			OnlinePlayersChoice = CreateConfig(Configs.UIOnlinePlayersMode, OnlinePlayersMode.BottomRight);
			ShowOwnedResources = CreateConfig(Configs.UIShowOwnedResources, true);
			ShowBossExpirationMessage = CreateConfig(Configs.UIShowPowerExpiration, true);
			AnnouncePlayerLogout = CreateConfig(Configs.UIPlayerLogoutAnnounce, true);
			ShowHeatLevelInAshlands = CreateConfig(Configs.UIAshlandsHeatLevel, true);
			EnemyNameplateChoice = CreateConfig(Configs.UIEnemyNameplateMode, EnemyNameplateMode.BarsWithHealth);
			ShowTamingProgress = CreateConfig(Configs.UITamingProgress, true);
			ItemQualityIndicatorChoice = CreateConfig(Configs.UIItemQualityIndicatorMode, ItemQualityMode.Horizontal);
			ItemQualitySymbolChoice = CreateConfig(Configs.UIItemQualitySymbol, ItemQualitySymbol.Star);
			ItemQualityColorChoice = CreateConfig(Configs.UIItemQualityColor, ItemQualityColor.Yellow);
			ColoredItemDurabilityBar = CreateConfig(Configs.UIItemDurabilityColor, true);
			DetailedHoverInfoChoice = CreateConfig(Configs.UIHoverInfoMode, HoverInfoMode.ColoredText);
			ShowContainerContents = CreateConfig(Configs.UIContainerContents, false);
			ContainerHoverModeChoice = CreateConfig(Configs.UIContainerHoverMode, ContainerHoverMode.CurrentPerMax);
			BeehiveHoverModeChoice = CreateConfig(Configs.UIBeeHoverMode, BeeHoverMode.RemainingTime);
			PlantHoverModeChoice = CreateConfig(Configs.UIPlantHoverMode, PlantHoverMode.RemainingTime);
			FermenterHoverModeChoice = CreateConfig(Configs.UIFermenterHoverMode, FermenterHoverMode.RemainingTime);
			CookingStationHoverModeChoice = CreateConfig(Configs.UICookingStationHoverMode, CookingStationHoverMode.RemainingTime);
			SmelterHoverModeChoice = CreateConfig(Configs.UISmelterHoverMode, SmelterHoverMode.RemainingTime);
			EggHoverModeChoice = CreateConfig(Configs.UIEggHoverMode, EggHoverMode.RemainingTime);

			// ===== Server Overrides
			ServerOverridesLocked = CreateServerOverride(Configs.LockServerOverrides, true);
			_ = configSync.AddLockingConfigEntry(ServerOverridesLocked);
			ServerOverridesEnabled = CreateServerOverride(Configs.EnableServerOverrides, true);
			MoreLoadingTipsOverride = CreateServerOverride(Configs.OverrideMoreLoadingTips, BoolOverride.UserChoice);
			InventoryWeightAndSlotsOverride = CreateServerOverride(Configs.OverrideInventoryWeightAndSlots, BoolOverride.UserChoice);
			EnemyDetectorOverride = CreateServerOverride(Configs.OverrideEnemyDetector, BoolOverride.UserChoice);
			BoatSpeedOverride = CreateServerOverride(Configs.OverrideBoatSpeed, BoolOverride.UserChoice);
			CurrentDayOverride = CreateServerOverride(Configs.OverrideCurrentDay, BoolOverride.UserChoice);
			CurrentTimeOverride = CreateServerOverride(Configs.OverrideCurrentTime, TimeModeOverride.UserChoice);
			WeatherForecastOverride = CreateServerOverride(Configs.OverrideWeatherForecast, BoolOverride.UserChoice);
			SmartBiomeOverride = CreateServerOverride(Configs.OverrideSmartBiome, BoolOverride.UserChoice);
			SummonCounterOverride = CreateServerOverride(Configs.OverrideSummonCounter, BoolOverride.UserChoice);
			OnlinePlayersOverride = CreateServerOverride(Configs.OverrideOnlinePlayers, OnlinePlayersModeOverride.UserChoice);
			OwnedResourcesOverride = CreateServerOverride(Configs.OverrideOwnedResources, BoolOverride.UserChoice);
			BossExpirationOverride = CreateServerOverride(Configs.OverrideBossExpiration, BoolOverride.UserChoice);
			PlayerLogoutOverride = CreateServerOverride(Configs.OverridePlayerLogout, BoolOverride.UserChoice);
			AshlandsHeatOverride = CreateServerOverride(Configs.OverrideAshlandsHeat, BoolOverride.UserChoice);
			EnemyNameplatesOverride = CreateServerOverride(Configs.OverrideEnemyNameplates, EnemyNameplateModeOverride.UserChoice);
			TamingProgressOverride = CreateServerOverride(Configs.OverrideTamingProgress, BoolOverride.UserChoice);
			ItemQualityModeOverrideChoice = CreateServerOverride(Configs.OverrideItemQualityMode, ItemQualityModeOverride.UserChoice);
			ItemQualitySymbolOverrideChoice = CreateServerOverride(Configs.OverrideItemQualitySymbol, ItemQualitySymbolOverride.UserChoice);
			ItemQualityColorOverrideChoice = CreateServerOverride(Configs.OverrideItemQualityColor, ItemQualityColorOverride.UserChoice);
			ItemDurabilityOverride = CreateServerOverride(Configs.OverrideItemDurability, BoolOverride.UserChoice);
			DetailedHoversOverride = CreateServerOverride(Configs.OverrideDetailedHovers, HoverInfoModeOverride.UserChoice);
			ContainerContentsOverride = CreateServerOverride(Configs.OverrideContainerContents, BoolOverride.UserChoice);
			ContainerHoverOverride = CreateServerOverride(Configs.OverrideContainerHover, ContainerHoverModeOverride.UserChoice);
			BeeHoverOverride = CreateServerOverride(Configs.OverrideBeeHover, BeeHoverModeOverride.UserChoice);
			PlantHoverOverride = CreateServerOverride(Configs.OverridePlantHover, PlantHoverModeOverride.UserChoice);
			FermenterHoverOverride = CreateServerOverride(Configs.OverrideFermenterHover, FermenterHoverModeOverride.UserChoice);
			CookingStationHoverOverride = CreateServerOverride(Configs.OverrideCookingStationHover, CookingStationHoverModeOverride.UserChoice);
			SmelterHoverOverride = CreateServerOverride(Configs.OverrideSmelterHover, SmelterHoverModeOverride.UserChoice);
			EggHoverOverride = CreateServerOverride(Configs.OverrideEggHover, EggHoverModeOverride.UserChoice);

			SetupWatcher();
		}

		private static ConfigEntry<T> CreateConfig<T>(ConfigMetadata metadata, T defaultValue)
		{
			ConfigEntry<T> configEntry = Config.Bind(ConfigSections.UI, metadata.Name, defaultValue, new ConfigDescription(metadata.Description));
			configEntry.SettingChanged += (_, __) => OnConfigChanged(metadata.Name);
			return configEntry;
		}

		private static ConfigEntry<T> CreateServerOverride<T>(ConfigMetadata metadata, T defaultValue)
		{
			ConfigEntry<T> configEntry = Config.Bind(ConfigSections.ServerOverrides, metadata.Name, defaultValue, new ConfigDescription(metadata.Description));

			SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = true;

			configEntry.SettingChanged += (_, __) => OnConfigChanged(metadata.Name);

			return configEntry;
		}

		private static bool ResolveBool(ConfigEntry<bool> localEntry, ConfigEntry<BoolOverride> overrideEntry)
		{
			if (!ServerOverridesEnabled.Value || overrideEntry.Value == BoolOverride.UserChoice)
				return localEntry.Value;

			switch (overrideEntry.Value)
			{
				case BoolOverride.ForceOn:
					return true;

				case BoolOverride.ForceOff:
					return false;

				default:
					return localEntry.Value;
			}
		}

		private static TLocal ResolveEnum<TLocal, TOverride>(ConfigEntry<TLocal> localEntry, ConfigEntry<TOverride> overrideEntry, TOverride userChoice) where TLocal : struct where TOverride : struct
		{
			if (!ServerOverridesEnabled.Value || overrideEntry.Value.Equals(userChoice))
				return localEntry.Value;

			if (Enum.TryParse(overrideEntry.Value.ToString(), out TLocal resolvedValue))
				return resolvedValue;

			log.Warn($"Could not resolve server override '{overrideEntry.Definition.Key}' value '{overrideEntry.Value}'. Falling back to local value.");
			return localEntry.Value;
		}

		private static void SetupWatcher()
		{
			watcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName)
			{
				IncludeSubdirectories = true,
				SynchronizingObject = ThreadingHelper.SynchronizingObject,
				EnableRaisingEvents = true
			};

			watcher.Changed += ReadConfigValues;
			watcher.Created += ReadConfigValues;
			watcher.Renamed += ReadConfigValues;
		}

		private static void ReadConfigValues(object sender, FileSystemEventArgs e)
		{
			if (!File.Exists(ConfigFileFullPath)) return;

			try
			{
				Config.Reload();
			}
			catch
			{
				log.Error($"There was an issue loading {ConfigFileName}");
			}
		}

		private static void OnConfigChanged(string configName)
		{
			log.Info($"Config setting '{configName}' changed!");
			Config.Save();

			if (configName == Configs.UICurrentDay.Name ||
				configName == Configs.UITimeMode.Name ||
				configName == Configs.OverrideCurrentDay.Name ||
				configName == Configs.OverrideCurrentTime.Name ||
				configName == Configs.EnableServerOverrides.Name)
			{
				UITimeAndDay.UpdatePositions();
			}

			if (configName == Configs.UIInventoryWeightAndSlots.Name ||
				configName == Configs.UIEnemyDetector.Name ||
				configName == Configs.OverrideInventoryWeightAndSlots.Name ||
				configName == Configs.OverrideEnemyDetector.Name ||
				configName == Configs.EnableServerOverrides.Name)
			{
				UIController.UpdateUIPositions();
			}
		}
	}
}