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

		private static ConfigEntry<bool> tweaksGearUpgradeUnlock;
		private static MethodInfo tweaksIsContainerSealedMethod;

		internal static bool MarsarahTweaksLoaded { get; private set; }
		internal static bool CraftFromContainersLoaded { get; private set; }

		internal static bool TweaksGearUpgradeUnlockEnabled =>
			MarsarahTweaksLoaded && tweaksGearUpgradeUnlock?.Value == true;

		internal static void Initialize()
		{
			if (Chainloader.PluginInfos.TryGetValue(MarsarahTweaksGUID, out var pluginInfo) && pluginInfo.Instance != null)
			{
				MarsarahTweaksLoaded = true;

				ConfigFile tweaksConfig = pluginInfo.Instance.Config;

				tweaksGearUpgradeUnlock = GetBoolConfig(tweaksConfig, "3 - Balance (Synced with Server)", "04 - Gear Upgrade Unlock");

				Type progressionHaltType = pluginInfo.Instance.GetType().Assembly.GetType("MarsarahTweaks.Patches.Features.ProgressionHalt");
				tweaksIsContainerSealedMethod = progressionHaltType?.GetMethod("IsContainerSealed", BindingFlags.Static | BindingFlags.NonPublic);

				if (tweaksIsContainerSealedMethod == null)
				{
					log.Warn("Could not find MarsarahTweaks Progression Halt container compatibility method.");
				}

				log.Info("MarsarahTweaks detected. Smart Biome will account for Gear Upgrade Unlock.");
			}

			CraftFromContainersLoaded = Chainloader.PluginInfos.ContainsKey(CraftFromContainersGUID);

			if (CraftFromContainersLoaded)
			{
				log.Info("Craft From Containers detected. Show Owned Resources will be handled by Craft From Containers.");
			}
		}

		private static ConfigEntry<bool> GetBoolConfig(ConfigFile config, string section, string key)
		{
			ConfigDefinition definition = new ConfigDefinition(section, key);

			if (config.TryGetEntry(definition, out ConfigEntry<bool> entry))
			{
				return entry;
			}

			log.Warn($"Could not find MarsarahTweaks config '{section} / {key}'.");
			return null;
		}

		internal static bool TweaksIsContainerSealed(Container container)
		{
			if (!MarsarahTweaksLoaded || tweaksIsContainerSealedMethod == null || container == null) return false;

			try
			{
				return (bool)tweaksIsContainerSealedMethod.Invoke(null, new object[] { container });
			}
			catch (Exception ex)
			{
				log.Warn($"Failed to check MarsarahTweaks Progression Halt container state: {ex.Message}");
				return false;
			}
		}
	}
}