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

		public enum InfoRailStyle
		{
			Style1,
			Style2,
			Style3
		}

		public enum InfoRailDisplayMode
		{
			Icons,
			Text
		}

		public enum TimeMode
		{
			DigitalClock,
			DayPhases,
			Off
		}

		public enum InventoryDisplayMode
		{
			WeightAndFreeSlots,
			WeightOnly,
			FreeSlotsOnly,
			Off
		}

		public enum EnemyDetectorMode
		{
			Consolidated,
			Split,
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

		public enum ContainerContentsMode
		{
			IconsHorizontal,
			IconsVertical,
			Text,
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

		public enum EnemyNameplateModeOverride
		{
			UserChoice,
			BarsOnly,
			BarsWithHealth,
			BarsWithPercent,
			BarsWithBoth,
			Off
		}

		public enum HoverInfoModeOverride
		{
			UserChoice,
			ColoredText,
			WhiteText,
			Off
		}

		public enum SkillProgressBarColor
		{
			Gold,
			White,
			Green,
			Blue,
			Cyan,
			Red,
			Purple,
			Off
		}

		public static class Configs
		{
			public static readonly ConfigMetadata UIInfoRailStyle = new ConfigMetadata("00 - Information Rail Style", "Choose the visual style used by the information rail and summon counter.");
			public static readonly ConfigMetadata UIInfoRailDisplayMode = new ConfigMetadata("00a - Information Rail Display Mode", "Choose whether the information rail and summon counter use icons or text labels.");
			public static readonly ConfigMetadata UIBetterLoadingTips = new ConfigMetadata("01 - Better Loading Tips", "Replaces the vanilla loading tips with a larger selection of more useful gameplay tips.");
			public static readonly ConfigMetadata UIInventoryWeightAndSlots = new ConfigMetadata("02 - Inventory Weight and Free Slots", "Choose whether the bottom-left information rail displays inventory weight, free slots, both, or neither.");
			public static readonly ConfigMetadata UIEnemyDetector = new ConfigMetadata("03 - Enemy Detector", "Choose whether nearby hostile enemies are shown in one consolidated counter, split into normal, tough, and boss counters, or enemy detection is disabled.");
			public static readonly ConfigMetadata UIBoatSpeed = new ConfigMetadata("04 - Show Boat Speed", "Shows boat speed when using a boat next to the sail indicator");
			public static readonly ConfigMetadata UICurrentDay = new ConfigMetadata("05 - Show Current Day", "Shows the current day above the minimap.");
			public static readonly ConfigMetadata UITimeMode = new ConfigMetadata("06 - Show Current Time", "Shows the current time above the minimap. Can choose between digital clock and day sections");
			public static readonly ConfigMetadata UIWeatherForecast = new ConfigMetadata("07 - Show Weather Forecast Indicator", "Shows the next scheduled weather as an icon at the bottom-right of the minimap and the remaining time to that weather.");
			public static readonly ConfigMetadata UISmartBiome = new ConfigMetadata("08 - Smart Biome Indicator", "Shows smart biome text on the minimap (colored according to worn armor relative to current biome)");
			public static readonly ConfigMetadata UISummonCounter = new ConfigMetadata("09 - Show Summon Counter", "Shows number of summoned skeletons from the Dead Raiser");
			public static readonly ConfigMetadata UIOnlinePlayers = new ConfigMetadata("10 - Show Online Players", "Displays the number of online players in the bottom-right corner. Player names can be toggled with the Home key. Not displayed if only one player is online.");
			public static readonly ConfigMetadata UIShowOwnedResources = new ConfigMetadata("11 - Show Owned Resources In Build Menu", "Displays the total amount of resources in the player's inventory in addition to the required resource amount for the selected piece or recipe in the build or crafting menu");
			public static readonly ConfigMetadata UIShowPowerExpiration = new ConfigMetadata("12 - Show Boss Power Expiration Message", "Displays a message in the center of the screen when any Forsaken Power expires");
			public static readonly ConfigMetadata UIAshlandsHeatLevel = new ConfigMetadata("13 - Show Heat Meter in Ashlands", "Shows a heat meter at the top-center of the screen when in Ashlands water or lava");
			public static readonly ConfigMetadata UIEnemyNameplateMode = new ConfigMetadata("14 - Enemy Nameplate Mode", "Changes the way enemy nameplates are displayed by changing bar style and colors and alerted/aggravated status. Has different ways of showing HP");
			public static readonly ConfigMetadata UITamingProgress = new ConfigMetadata("15 - Show Taming Progress", "Displays current taming percentage of animals that are acclamatizing under the HP bar. This is independent of Enemy Nameplate Mode");
			public static readonly ConfigMetadata UIItemQualityIndicatorMode = new ConfigMetadata("16 - Item Quality Indicator Mode", "Changes the way item quality is displayed by converting the vanilla number to symbols.");
			public static readonly ConfigMetadata UIItemQualitySymbol = new ConfigMetadata("17 - Symbol For Item Quality", "Choose the symbol used for the item quality indicator. Requires Item Quality Indicator Mode to be enabled");
			public static readonly ConfigMetadata UIItemQualityColor = new ConfigMetadata("18 - Color For Item Quality", "Choose the color used for the item quality indicator. Requires Item Quality Indicator Mode to be enabled");
			public static readonly ConfigMetadata UIItemDurabilityColor = new ConfigMetadata("19 - Better Item Durability Bar", "Colors the item durability bar according to current durability and modifies the sprite texture");
			public static readonly ConfigMetadata UIHoverInfoMode = new ConfigMetadata("20 - Detailed Hover Information", "Adds more information when hovering over objects. Master toggle for the hover information configs below");
			public static readonly ConfigMetadata UIContainerContents = new ConfigMetadata("21 - Container Contents Mode", "Choose how container contents are displayed when hovering: horizontal icons, vertical icons, text, or off. Requires Detailed Hover Information.");
			public static readonly ConfigMetadata UIContainerHoverMode = new ConfigMetadata("22 - Container Hover Mode", "Choose the method of displaying Container hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UIBeeHoverMode = new ConfigMetadata("23 - Beehive Hover Mode", "Choose the method of displaying Beehive hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UIPlantHoverMode = new ConfigMetadata("24 - Plant Hover Mode", "Choose the method of displaying Plant hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UIFermenterHoverMode = new ConfigMetadata("25 - Fermenter Hover Mode", "Choose the method of displaying Fermenter hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UICookingStationHoverMode = new ConfigMetadata("26 - CookingStation Hover Mode", "Choose the method of displaying Cooking Station hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UISmelterHoverMode = new ConfigMetadata("27 - Smelter Hover Mode", "Choose the method of displaying Smelter hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UIEggHoverMode = new ConfigMetadata("28 - Egg Hover Mode", "Choose the method of displaying Egg hatching hover info. Requires Detailed Hover Information");
			public static readonly ConfigMetadata UIStatusEffectsUnderMinimap = new ConfigMetadata("29 - Status Effects Under Minimap", "Moves status effects below the minimap and displays them in a more compact layout.");
			public static readonly ConfigMetadata UIGlobalChatByDefault = new ConfigMetadata("30 - Global Chat By Default", "Makes regular chat messages visible to all players regardless of distance.");
			public static readonly ConfigMetadata UISkillProgressBar = new ConfigMetadata("31 - Skill Progress Bar", "Displays skill progress when advancing toward the next skill level. Choose the bar color or Off to disable.");
			public static readonly ConfigMetadata UICharacterStatistics = new ConfigMetadata("32 - Logon Screen Character Statistics", "Displays statistics and notable facts for the selected character on the character selection screen.");

			public static readonly ConfigMetadata LockServerOverrides = new ConfigMetadata("01 - Lock Server Overrides", "If on, only server admins can change Server Override settings. Local UI settings are never affected.");
			public static readonly ConfigMetadata EnableServerOverrides = new ConfigMetadata("02 - Enable Server Overrides", "If on, the server can override selected UI settings for connected players. Settings left as UserChoice continue to use each player's local UI preference.");
			public static readonly ConfigMetadata OverrideEnemyDetector = new ConfigMetadata("03 - Enemy Detector Override", "Overrides the player's local Enemy Detector setting.");
			public static readonly ConfigMetadata OverrideCurrentDay = new ConfigMetadata("04 - Current Day Override", "Overrides the player's local Current Day setting.");
			public static readonly ConfigMetadata OverrideCurrentTime = new ConfigMetadata("05 - Current Time Override", "Overrides the player's local Current Time setting.");
			public static readonly ConfigMetadata OverrideWeatherForecast = new ConfigMetadata("06 - Weather Forecast Override", "Overrides the player's local Weather Forecast setting.");
			public static readonly ConfigMetadata OverrideSmartBiome = new ConfigMetadata("07 - Smart Biome Override", "Overrides the player's local Smart Biome setting.");
			public static readonly ConfigMetadata OverrideAshlandsHeat = new ConfigMetadata("08 - Ashlands Heat Meter Override", "Overrides the player's local Ashlands Heat Meter setting.");
			public static readonly ConfigMetadata OverrideEnemyNameplates = new ConfigMetadata("09 - Enemy Nameplate Mode Override", "Overrides the player's local Enemy Nameplate Mode setting.");
			public static readonly ConfigMetadata OverrideTamingProgress = new ConfigMetadata("10 - Taming Progress Override", "Overrides the player's local Taming Progress setting.");
			public static readonly ConfigMetadata OverrideDetailedHovers = new ConfigMetadata("11 - Detailed Hover Information Override", "Overrides the player's local Detailed Hover Information setting.");
			public static readonly ConfigMetadata OverrideContainerContents = new ConfigMetadata("12 - Container Contents Override", "Overrides the player's local Container Contents setting.");
			public static readonly ConfigMetadata OverrideGlobalChatByDefault = new ConfigMetadata("13 - Global Chat By Default Override", "Overrides the player's local Global Chat By Default setting.");
		}

		public static ConfigEntry<InfoRailStyle> InfoRailStyleChoice;
		public static ConfigEntry<InfoRailDisplayMode> InfoRailDisplayModeChoice;
		public static ConfigEntry<bool> BetterLoadingTipsEnabled;
		public static ConfigEntry<InventoryDisplayMode> InventoryDisplayChoice;
		public static ConfigEntry<EnemyDetectorMode> EnemyDetectorChoice;
		public static ConfigEntry<bool> ShowBoatSpeed;
		public static ConfigEntry<bool> ShowCurrentDay;
		public static ConfigEntry<TimeMode> TimeChoice;
		public static ConfigEntry<bool> ShowWeatherForecast;
		public static ConfigEntry<bool> ShowSmartBiome;
		public static ConfigEntry<bool> ShowSummonCounter;
		public static ConfigEntry<bool> ShowOnlinePlayers;
		public static ConfigEntry<bool> ShowOwnedResources;
		public static ConfigEntry<bool> ShowBossExpirationMessage;
		public static ConfigEntry<bool> ShowHeatLevelInAshlands;
		public static ConfigEntry<EnemyNameplateMode> EnemyNameplateChoice;
		public static ConfigEntry<bool> ShowTamingProgress;
		public static ConfigEntry<ItemQualityMode> ItemQualityIndicatorChoice;
		public static ConfigEntry<ItemQualitySymbol> ItemQualitySymbolChoice;
		public static ConfigEntry<ItemQualityColor> ItemQualityColorChoice;
		public static ConfigEntry<bool> ColoredItemDurabilityBar;
		public static ConfigEntry<HoverInfoMode> DetailedHoverInfoChoice;
		public static ConfigEntry<ContainerContentsMode> ContainerContentsChoice;
		public static ConfigEntry<ContainerHoverMode> ContainerHoverModeChoice;
		public static ConfigEntry<BeeHoverMode> BeehiveHoverModeChoice;
		public static ConfigEntry<PlantHoverMode> PlantHoverModeChoice;
		public static ConfigEntry<FermenterHoverMode> FermenterHoverModeChoice;
		public static ConfigEntry<CookingStationHoverMode> CookingStationHoverModeChoice;
		public static ConfigEntry<SmelterHoverMode> SmelterHoverModeChoice;
		public static ConfigEntry<EggHoverMode> EggHoverModeChoice;
		public static ConfigEntry<bool> StatusEffectsUnderMinimap;
		public static ConfigEntry<bool> GlobalChatByDefault;
		public static ConfigEntry<SkillProgressBarColor> SkillProgressBarChoice;
		public static ConfigEntry<bool> ShowCharacterStatistics;

		public static ConfigEntry<bool> ServerOverridesLocked;
		public static ConfigEntry<bool> ServerOverridesEnabled;

		public static ConfigEntry<BoolOverride> EnemyDetectorOverride;
		public static ConfigEntry<BoolOverride> CurrentDayOverride;
		public static ConfigEntry<TimeModeOverride> CurrentTimeOverride;
		public static ConfigEntry<BoolOverride> WeatherForecastOverride;
		public static ConfigEntry<BoolOverride> SmartBiomeOverride;
		public static ConfigEntry<BoolOverride> AshlandsHeatOverride;
		public static ConfigEntry<EnemyNameplateModeOverride> EnemyNameplatesOverride;
		public static ConfigEntry<BoolOverride> TamingProgressOverride;
		public static ConfigEntry<HoverInfoModeOverride> DetailedHoversOverride;
		public static ConfigEntry<BoolOverride> ContainerContentsOverride;
		public static ConfigEntry<BoolOverride> GlobalChatByDefaultOverride;

		// Effective UI settings
		public static InfoRailDisplayMode EffectiveInfoRailDisplayModeChoice => InfoRailDisplayModeChoice.Value;
		public static bool EffectiveBetterLoadingTipsEnabled => BetterLoadingTipsEnabled.Value;
		public static InventoryDisplayMode EffectiveInventoryDisplayChoice => InventoryDisplayChoice.Value;

		public static bool EffectiveShowInventoryWeightAndSlots => EffectiveInventoryDisplayChoice != InventoryDisplayMode.Off;

		public static EnemyDetectorMode EffectiveEnemyDetectorChoice
		{
			get
			{
				if (!ServerOverridesEnabled.Value || EnemyDetectorOverride.Value == BoolOverride.UserChoice)
					return EnemyDetectorChoice.Value;

				switch (EnemyDetectorOverride.Value)
				{
					case BoolOverride.ForceOn:
						return EnemyDetectorChoice.Value == EnemyDetectorMode.Off
							? EnemyDetectorMode.Consolidated
							: EnemyDetectorChoice.Value;

					case BoolOverride.ForceOff:
						return EnemyDetectorMode.Off;

					default:
						return EnemyDetectorChoice.Value;
				}
			}
		}

		public static bool EffectiveShowEnemyDetector => EffectiveEnemyDetectorChoice != EnemyDetectorMode.Off;
		public static bool EffectiveShowBoatSpeed => ShowBoatSpeed.Value;
		public static bool EffectiveShowCurrentDay => ResolveBool(ShowCurrentDay, CurrentDayOverride);
		public static TimeMode EffectiveTimeChoice => ResolveEnum(TimeChoice, CurrentTimeOverride, TimeModeOverride.UserChoice);
		public static bool EffectiveShowWeatherForecast => ResolveBool(ShowWeatherForecast, WeatherForecastOverride);
		public static bool EffectiveShowSmartBiome => ResolveBool(ShowSmartBiome, SmartBiomeOverride);
		public static bool EffectiveShowSummonCounter => ShowSummonCounter.Value;
		public static bool EffectiveShowOnlinePlayers => ShowOnlinePlayers.Value;
		public static bool EffectiveShowOwnedResources => ShowOwnedResources.Value;
		public static bool EffectiveShowBossExpirationMessage => ShowBossExpirationMessage.Value;
		public static bool EffectiveShowHeatLevelInAshlands => ResolveBool(ShowHeatLevelInAshlands, AshlandsHeatOverride);
		public static EnemyNameplateMode EffectiveEnemyNameplateChoice => ResolveEnum(EnemyNameplateChoice, EnemyNameplatesOverride, EnemyNameplateModeOverride.UserChoice);
		public static bool EffectiveShowTamingProgress => ResolveBool(ShowTamingProgress, TamingProgressOverride);
		public static ItemQualityMode EffectiveItemQualityIndicatorChoice => ItemQualityIndicatorChoice.Value;
		public static ItemQualitySymbol EffectiveItemQualitySymbolChoice => ItemQualitySymbolChoice.Value;
		public static ItemQualityColor EffectiveItemQualityColorChoice => ItemQualityColorChoice.Value;
		public static bool EffectiveColoredItemDurabilityBar => ColoredItemDurabilityBar.Value;
		public static HoverInfoMode EffectiveDetailedHoverInfoChoice => ResolveEnum(DetailedHoverInfoChoice, DetailedHoversOverride, HoverInfoModeOverride.UserChoice);
		public static ContainerContentsMode EffectiveContainerContentsChoice
		{
			get
			{
				if (!ServerOverridesEnabled.Value || ContainerContentsOverride.Value == BoolOverride.UserChoice)
					return ContainerContentsChoice.Value;

				switch (ContainerContentsOverride.Value)
				{
					case BoolOverride.ForceOn:
						return ContainerContentsChoice.Value == ContainerContentsMode.Off
							? ContainerContentsMode.IconsHorizontal
							: ContainerContentsChoice.Value;

					case BoolOverride.ForceOff:
						return ContainerContentsMode.Off;

					default:
						return ContainerContentsChoice.Value;
				}
			}
		}
		public static ContainerHoverMode EffectiveContainerHoverModeChoice => ContainerHoverModeChoice.Value;
		public static BeeHoverMode EffectiveBeehiveHoverModeChoice => BeehiveHoverModeChoice.Value;
		public static PlantHoverMode EffectivePlantHoverModeChoice => PlantHoverModeChoice.Value;
		public static FermenterHoverMode EffectiveFermenterHoverModeChoice => FermenterHoverModeChoice.Value;
		public static CookingStationHoverMode EffectiveCookingStationHoverModeChoice => CookingStationHoverModeChoice.Value;
		public static SmelterHoverMode EffectiveSmelterHoverModeChoice => SmelterHoverModeChoice.Value;
		public static EggHoverMode EffectiveEggHoverModeChoice => EggHoverModeChoice.Value;
		public static bool EffectiveStatusEffectsUnderMinimap => StatusEffectsUnderMinimap.Value;
		public static bool EffectiveGlobalChatByDefault => ResolveBool(GlobalChatByDefault, GlobalChatByDefaultOverride);
		public static SkillProgressBarColor EffectiveSkillProgressBarChoice => SkillProgressBarChoice.Value;
		public static bool EffectiveShowCharacterStatistics => ShowCharacterStatistics.Value;

		public static void Init(ConfigFile configFile)
		{
			Config = configFile;

			// ===== Local UI Settings
			InfoRailStyleChoice = CreateConfig(Configs.UIInfoRailStyle, InfoRailStyle.Style1);
			UIStyleManager.Initialize(InfoRailStyleChoice);
			InfoRailDisplayModeChoice = CreateConfig(Configs.UIInfoRailDisplayMode, InfoRailDisplayMode.Icons);
			BetterLoadingTipsEnabled = CreateConfig(Configs.UIBetterLoadingTips, true);
			InventoryDisplayChoice = CreateConfig(Configs.UIInventoryWeightAndSlots, InventoryDisplayMode.WeightAndFreeSlots);
			EnemyDetectorChoice = CreateConfig(Configs.UIEnemyDetector, EnemyDetectorMode.Consolidated);
			ShowBoatSpeed = CreateConfig(Configs.UIBoatSpeed, true);
			ShowCurrentDay = CreateConfig(Configs.UICurrentDay, true);
			TimeChoice = CreateConfig(Configs.UITimeMode, TimeMode.DigitalClock);
			ShowWeatherForecast = CreateConfig(Configs.UIWeatherForecast, true);
			ShowSmartBiome = CreateConfig(Configs.UISmartBiome, true);
			ShowSummonCounter = CreateConfig(Configs.UISummonCounter, true);
			ShowOnlinePlayers = CreateConfig(Configs.UIOnlinePlayers, true);
			ShowOwnedResources = CreateConfig(Configs.UIShowOwnedResources, true);
			ShowBossExpirationMessage = CreateConfig(Configs.UIShowPowerExpiration, true);
			ShowHeatLevelInAshlands = CreateConfig(Configs.UIAshlandsHeatLevel, true);
			EnemyNameplateChoice = CreateConfig(Configs.UIEnemyNameplateMode, EnemyNameplateMode.BarsWithHealth);
			ShowTamingProgress = CreateConfig(Configs.UITamingProgress, true);
			ItemQualityIndicatorChoice = CreateConfig(Configs.UIItemQualityIndicatorMode, ItemQualityMode.Horizontal);
			ItemQualitySymbolChoice = CreateConfig(Configs.UIItemQualitySymbol, ItemQualitySymbol.Star);
			ItemQualityColorChoice = CreateConfig(Configs.UIItemQualityColor, ItemQualityColor.Yellow);
			ColoredItemDurabilityBar = CreateConfig(Configs.UIItemDurabilityColor, true);
			DetailedHoverInfoChoice = CreateConfig(Configs.UIHoverInfoMode, HoverInfoMode.ColoredText);
			ContainerContentsChoice = CreateConfig(Configs.UIContainerContents, ContainerContentsMode.IconsHorizontal);
			ContainerHoverModeChoice = CreateConfig(Configs.UIContainerHoverMode, ContainerHoverMode.CurrentPerMax);
			BeehiveHoverModeChoice = CreateConfig(Configs.UIBeeHoverMode, BeeHoverMode.RemainingTime);
			PlantHoverModeChoice = CreateConfig(Configs.UIPlantHoverMode, PlantHoverMode.RemainingTime);
			FermenterHoverModeChoice = CreateConfig(Configs.UIFermenterHoverMode, FermenterHoverMode.RemainingTime);
			CookingStationHoverModeChoice = CreateConfig(Configs.UICookingStationHoverMode, CookingStationHoverMode.RemainingTime);
			SmelterHoverModeChoice = CreateConfig(Configs.UISmelterHoverMode, SmelterHoverMode.RemainingTime);
			EggHoverModeChoice = CreateConfig(Configs.UIEggHoverMode, EggHoverMode.RemainingTime);
			StatusEffectsUnderMinimap = CreateConfig(Configs.UIStatusEffectsUnderMinimap, true);
			GlobalChatByDefault = CreateConfig(Configs.UIGlobalChatByDefault, true);
			SkillProgressBarChoice = CreateConfig(Configs.UISkillProgressBar, SkillProgressBarColor.Gold);
			ShowCharacterStatistics = CreateConfig(Configs.UICharacterStatistics, true);

			// ===== Server Overrides
			ServerOverridesLocked = CreateServerOverride(Configs.LockServerOverrides, true);
			_ = configSync.AddLockingConfigEntry(ServerOverridesLocked);

			ServerOverridesEnabled = CreateServerOverride(Configs.EnableServerOverrides, true);

			EnemyDetectorOverride = CreateServerOverride(Configs.OverrideEnemyDetector, BoolOverride.UserChoice);
			CurrentDayOverride = CreateServerOverride(Configs.OverrideCurrentDay, BoolOverride.UserChoice);
			CurrentTimeOverride = CreateServerOverride(Configs.OverrideCurrentTime, TimeModeOverride.UserChoice);
			WeatherForecastOverride = CreateServerOverride(Configs.OverrideWeatherForecast, BoolOverride.UserChoice);
			SmartBiomeOverride = CreateServerOverride(Configs.OverrideSmartBiome, BoolOverride.UserChoice);
			AshlandsHeatOverride = CreateServerOverride(Configs.OverrideAshlandsHeat, BoolOverride.UserChoice);
			EnemyNameplatesOverride = CreateServerOverride(Configs.OverrideEnemyNameplates, EnemyNameplateModeOverride.UserChoice);
			TamingProgressOverride = CreateServerOverride(Configs.OverrideTamingProgress, BoolOverride.UserChoice);
			DetailedHoversOverride = CreateServerOverride(Configs.OverrideDetailedHovers, HoverInfoModeOverride.UserChoice);
			ContainerContentsOverride = CreateServerOverride(Configs.OverrideContainerContents, BoolOverride.UserChoice);
			GlobalChatByDefaultOverride = CreateServerOverride(Configs.OverrideGlobalChatByDefault, BoolOverride.UserChoice);

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

			if (configName == Configs.UIItemQualityIndicatorMode.Name ||
				configName == Configs.UIItemQualitySymbol.Name ||
				configName == Configs.UIItemQualityColor.Name)
			{
				UIItemQuality.UpdateSymbols();
			}
		}
	}
}