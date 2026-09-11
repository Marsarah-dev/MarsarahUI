using BepInEx.Bootstrap;
using BepInEx.Configuration;
using System;
using System.Reflection;

namespace MarsarahUI.Managers
{
	internal static class CompatibilityManager
	{
		private static readonly LogManager log = new LogManager("Compatibility Manager", LogManager.LogLevel.Warning);

		private const string MarsarahTweaksGUID = "Marsarah.MarsarahTweaks";
		private const string CraftFromContainersGUID = "aedenthorn.CraftFromContainers";
		private const string MinimalStatusEffectsGUID = "randyknapp.mods.minimalstatuseffects";

		internal static bool MarsarahTweaksLoaded { get; private set; }
		internal static bool CraftFromContainersLoaded { get; private set; }
		internal static bool MinimalStatusEffectsLoaded { get; private set; }

		internal static void Initialize()
		{
			MarsarahTweaksLoaded = Chainloader.PluginInfos.ContainsKey(MarsarahTweaksGUID);
			CraftFromContainersLoaded = Chainloader.PluginInfos.ContainsKey(CraftFromContainersGUID);
			MinimalStatusEffectsLoaded = Chainloader.PluginInfos.ContainsKey(MinimalStatusEffectsGUID);

			if (MarsarahTweaksLoaded)
			{
				log.Warn("MarsarahTweaks detected. Smart Biome will account for Gear Upgrade Unlock.");
			}

			if (CraftFromContainersLoaded)
			{
				log.Warn("Craft From Containers detected. Show Owned Resources will be handled by Craft From Containers.");
			}

			if (MinimalStatusEffectsLoaded)
			{
				log.Warn("Minimal Status Effects detected.");
			}
		}

		internal static bool IsTweaksGearUpgradeUnlockEnabled()
		{
			if (!MarsarahTweaksLoaded) return false;

			try
			{
				BepInEx.PluginInfo pluginInfo = Chainloader.PluginInfos[MarsarahTweaksGUID];
				Type configManagerType = pluginInfo.Instance.GetType().Assembly.GetType("MarsarahTweaks.Managers.ConfigManager");

				FieldInfo configField = configManagerType?.GetField("GearUpgradeUnlockEnabled", BindingFlags.Public | BindingFlags.Static);
				ConfigEntry<bool> configEntry = configField?.GetValue(null) as ConfigEntry<bool>;

				return configEntry?.Value ?? false;
			}
			catch (Exception ex)
			{
				log.Warn($"Could not read MarsarahTweaks Gear Upgrade Unlock setting: {ex.Message}");
				return false;
			}
		}
	}
}