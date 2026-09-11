using BepInEx.Bootstrap;

namespace MarsarahUI.Managers
{
	internal static class CompatibilityManager
	{
		private static readonly LogManager log = new LogManager("Compatibility Manager", LogManager.LogLevel.Warning);

		private const string CraftFromContainersGUID = "aedenthorn.CraftFromContainers";
		private const string MinimalStatusEffectsGUID = "randyknapp.mods.minimalstatuseffects";

		internal static bool CraftFromContainersLoaded { get; private set; }
		internal static bool MinimalStatusEffectsLoaded { get; private set; }

		internal static void Initialize()
		{
			CraftFromContainersLoaded = Chainloader.PluginInfos.ContainsKey(CraftFromContainersGUID);
			MinimalStatusEffectsLoaded = Chainloader.PluginInfos.ContainsKey(MinimalStatusEffectsGUID);

			if (CraftFromContainersLoaded)
			{
				log.Warn("Craft From Containers detected. Show Owned Resources will be handled by Craft From Containers.");
			}

			if (MinimalStatusEffectsLoaded)
			{
				log.Warn("Minimal Status Effects detected.");
			}
		}
	}
}