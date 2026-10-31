
using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace MarsarahUI.Patches.UI
{
	internal class UIAlternateBiomeNames
	{
		private static readonly LogManager log = new LogManager("UI Alternate Biome Names", LogManager.LogLevel.Info);

		private static readonly FieldInfo biomeNameField = AccessTools.Field(typeof(Minimap), "m_biomeNameSmall");
		private static readonly MethodInfo updateBiomeMethod = AccessTools.Method(typeof(Minimap), "UpdateBiome", new[] { typeof(Player) });
		private static string lastDisplayName = string.Empty;
		private static string vanillaBiomeName = string.Empty;
		private static string lastBiome = string.Empty;
		private static string currentAlternateDisplayName = string.Empty;
		internal static string CurrentAlternateDisplayName => currentAlternateDisplayName;

		[HarmonyPatch(typeof(Minimap), "UpdateBiome")]
		private static class AlternateBiome_MinimapPatch
		{
			private static void Postfix(Minimap __instance, Player player)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null || player == null || WorldGenerator.instance == null) return;

				// Read the existing vanilla text component without creating or replacing it.
				TMP_Text biomeText = biomeNameField?.GetValue(__instance) as TMP_Text;
				if (biomeText == null) return;

				string biome = player.GetCurrentBiome().ToString();
				if (lastBiome != biome)
				{
					vanillaBiomeName = biomeText.text;
					lastBiome = biome;
				}

				if (!ConfigManager.EffectiveShowAlternateBiomeNames)
				{
					currentAlternateDisplayName = string.Empty;
					return;
				}

				string displayName = GetBiomeDisplayName(biome);

				bool hasAlternates = false;

				BiomeSector sector = WorldGenerator.instance.GetBiomeSector(player.transform.position, true);

				if (sector != null && sector.AltBiomes != null)
				{
					List<string> alternateNames = new List<string>();

					foreach (AltBiome alternate in sector.AltBiomes)
					{
						if (alternate == null || string.IsNullOrWhiteSpace(alternate.m_name)) continue;

						string name = alternate.m_name.Trim();
						string shortName = GetShortAlternateName(name, displayName);

						if (!alternateNames.Contains(shortName))
							alternateNames.Add(shortName);
					}

					if (alternateNames.Count == 1)
					{
						displayName = alternateNames[0] + " " + displayName;
						hasAlternates = true;
					}
					else if (alternateNames.Count > 1)
					{
						displayName += ": " + string.Join(", ", alternateNames);
						hasAlternates = true;
					}
				}

				currentAlternateDisplayName = hasAlternates ? displayName : string.Empty;
				biomeText.text = displayName;

				if (lastDisplayName != displayName)
				{
					log.Info($"Minimap biome text updated: {displayName}");
					lastDisplayName = displayName;
				}
			}
		}

		private static string GetBiomeDisplayName(string biome)
		{
			switch (biome)
			{
				case "BlackForest": return "Black Forest";
				case "AshLands": return "Ashlands";
				case "DeepNorth": return "Deep North";
				default: return biome ?? string.Empty;
			}
		}

		private static string GetShortAlternateName(string name, string biome)
		{
			string suffix = " " + biome;

			if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
				return name.Substring(0, name.Length - suffix.Length).Trim();

			return name;
		}

		internal static void RefreshFromConfig()
		{
			if (Minimap.instance == null || Player.m_localPlayer == null) return;

			if (!ConfigManager.EffectiveShowAlternateBiomeNames)
			{
				TMP_Text biomeText = biomeNameField?.GetValue(Minimap.instance) as TMP_Text;
				if (biomeText == null) return;

				biomeText.text = vanillaBiomeName;
				lastDisplayName = string.Empty;
				currentAlternateDisplayName = string.Empty;

				log.Info("Alternate biome names disabled. Restored original vanilla biome text.");
			}
			else
			{
				if (updateBiomeMethod == null)
				{
					log.Warn("Could not find Minimap.UpdateBiome method.");
					return;
				}

				updateBiomeMethod.Invoke(Minimap.instance, new object[] { Player.m_localPlayer });

				log.Info("Alternate biome names enabled. Refreshed minimap.");
			}
		}
	}
}
