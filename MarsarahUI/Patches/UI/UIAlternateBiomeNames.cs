
using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using System.Linq;
using MarsarahUI.Patches.Logic;

namespace MarsarahUI.Patches.UI
{
	internal class UIAlternateBiomeNames
	{
		private static readonly LogManager log = new LogManager("UI Alternate Biome Names", LogManager.LogLevel.Warning);

		private static readonly FieldInfo biomeNameField = AccessTools.Field(typeof(Minimap), "m_biomeNameSmall");
		private static readonly MethodInfo updateBiomeMethod = AccessTools.Method(typeof(Minimap), "UpdateBiome", new[] { typeof(Player) });
		private static string lastDisplayName = string.Empty;
		private static string vanillaBiomeName = string.Empty;
		private static string lastBiome = string.Empty;

		private static readonly List<string> currentAlternateNames = new List<string>();
		private static string currentBaseBiome = string.Empty;
		internal static string CurrentAlternateSignature { get; private set; } = string.Empty;
		internal static bool HasAlternateBiomes => currentAlternateNames.Count > 0;

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

				if (ConfigManager.EffectiveAlternateBiomeDisplayMode == ConfigManager.AlternateBiomeDisplayMode.Off)
				{
					currentAlternateNames.Clear();
					CurrentAlternateSignature = string.Empty;
					return;
				}

				string baseBiome = GetBiomeDisplayName(biome);
				List<string> alternateNames = new List<string>();

				BiomeSector sector = WorldGenerator.instance.GetBiomeSector(player.transform.position, true);

				if (sector != null && sector.AltBiomes != null)
				{
					foreach (AltBiome alternate in sector.AltBiomes)
					{
						if (alternate == null || string.IsNullOrWhiteSpace(alternate.m_name)) continue;

						string shortName = GetShortAlternateName(alternate.m_name.Trim(), baseBiome);

						if (!alternateNames.Contains(shortName))
							alternateNames.Add(shortName);
					}
				}

				// Order by the same priority used by portal transition images.
				alternateNames = alternateNames.OrderBy(name => BiomePriority.GetAlternatePriority(biome, name)).ToList();

				currentAlternateNames.Clear();
				currentAlternateNames.AddRange(alternateNames);
				currentBaseBiome = baseBiome;
				CurrentAlternateSignature = baseBiome + "|" + string.Join("|", alternateNames);

				// Measure against the existing vanilla TextMeshPro component.
				float width = biomeText.rectTransform.rect.width;
				bool oneRow = ConfigManager.EffectiveAlternateBiomeDisplayMode == ConfigManager.AlternateBiomeDisplayMode.OneRow;
				string referenceText = oneRow ? "Mg" : "Mg\nMg";
				float maximumHeight = biomeText.GetPreferredValues(referenceText, width, 10000f).y;

				string displayName = GetLimitedDisplayName(baseBiome, alternateNames, candidate =>
					biomeText.GetPreferredValues(candidate, width, 10000f).y <= maximumHeight + 0.5f);

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

		private static string GetLimitedDisplayName(string biome, List<string> alternates, Func<string, bool> fits)
		{
			if (alternates.Count == 0)
				return biome;

			if (alternates.Count == 1)
			{
				string fullName = alternates[0] + " " + biome;

				if (fits(fullName))
					return fullName;

				return TruncateToFit(alternates[0], fits);
			}

			for (int visible = alternates.Count; visible >= 1; visible--)
			{
				string candidate = biome + ": " + string.Join(", ", alternates.GetRange(0, visible));

				int hidden = alternates.Count - visible;

				if (hidden > 0)
					candidate += " +" + hidden;

				if (fits(candidate))
					return candidate;
			}

			string compactName = alternates[0] + " +" + (alternates.Count - 1);

			if (fits(compactName))
				return compactName;

			return TruncateToFit(biome + " +" + alternates.Count, fits);
		}

		private static string TruncateToFit(string value, Func<string, bool> fits)
		{
			if (fits(value)) return value;

			for (int length = value.Length - 1; length > 0; length--)
			{
				string candidate = value.Substring(0, length).TrimEnd() + "…";

				if (fits(candidate))
					return candidate;
			}

			return fits("…") ? "…" : string.Empty;
		}

		internal static string GetSmartBiomeDisplayName(Func<string, bool> fits)
		{
			if (currentAlternateNames.Count == 0)
				return string.Empty;

			return GetLimitedDisplayName(currentBaseBiome, currentAlternateNames, fits);
		}

		internal static void RefreshFromConfig()
		{
			if (Minimap.instance == null || Player.m_localPlayer == null) return;

			if (ConfigManager.EffectiveAlternateBiomeDisplayMode == ConfigManager.AlternateBiomeDisplayMode.Off)
			{
				TMP_Text biomeText = biomeNameField?.GetValue(Minimap.instance) as TMP_Text;
				if (biomeText == null) return;

				biomeText.text = vanillaBiomeName;
				lastDisplayName = string.Empty;
				currentAlternateNames.Clear();
				CurrentAlternateSignature = string.Empty;

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
