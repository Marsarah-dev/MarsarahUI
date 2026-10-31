
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
		private static string lastDisplayName = string.Empty;

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
				string displayName = GetBiomeDisplayName(biome);

				if (!ConfigManager.EffectiveShowAlternateBiomeNames)
				{
					string vanillaName = Localization.instance.Localize("$biome_" + biome.ToLowerInvariant());

					if (biomeText.text != vanillaName)
					{
						biomeText.text = vanillaName;
						log.Info($"Alternate biome names disabled. Restored vanilla biome: {vanillaName}");
					}

					lastDisplayName = string.Empty;
					return;
				}

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
					}
					else if (alternateNames.Count > 1)
					{
						displayName += ": " + string.Join(", ", alternateNames);
					}
				}

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
	}
}
