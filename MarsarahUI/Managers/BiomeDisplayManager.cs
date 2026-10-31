using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Managers
{
	internal static class BiomeDisplayManager
	{
		private static readonly LogManager log = new LogManager("Biome Display", LogManager.LogLevel.Info);

		private const float BiomeUpdateInterval = 0.25f;
		private static float nextBiomeUpdateTime;
		private static string cachedBiome;
		private static Vector3 cachedPosition;
		private static string cachedText;
		private static bool cachedHasAlternates;

		internal static string GetBiomeDisplayName(string biome)
		{
			switch (biome)
			{
				case "BlackForest": return "Black Forest";
				case "AshLands": return "Ashlands";
				case "DeepNorth": return "Deep North";
				default: return biome ?? string.Empty;
			}
		}

		internal static string GetDisplayName(Player player, out bool hasAlternates)
		{
			hasAlternates = false;
			if (player == null) return string.Empty;

			string biome = player.GetCurrentBiome().ToString();
			if (!ConfigManager.EffectiveShowAlternateBiomeNames) return GetBiomeDisplayName(biome);

			Vector3 position = player.transform.position;
			if (cachedText != null && biome == cachedBiome && Time.unscaledTime < nextBiomeUpdateTime && (position - cachedPosition).sqrMagnitude < 16f)
			{
				hasAlternates = cachedHasAlternates;
				return cachedText;
			}

			cachedBiome = biome;
			cachedPosition = position;
			nextBiomeUpdateTime = Time.unscaledTime + BiomeUpdateInterval;
			cachedText = GetBiomeDisplayName(biome);
			cachedHasAlternates = false;

			try
			{
				WorldGenerator generator = WorldGenerator.instance;
				if (generator == null) return cachedText;

				BiomeSector sector = generator.GetBiomeSector(position, true);
				if (sector == null || sector.AltBiomes == null) return cachedText;

				List<string> names = new List<string>();
				HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (AltBiome alternate in sector.AltBiomes)
				{
					if (alternate == null || string.IsNullOrWhiteSpace(alternate.m_name)) continue;
					string name = alternate.m_name.Trim();
					if (seen.Add(name)) names.Add(name);
				}

				if (names.Count == 0) return cachedText;

				cachedHasAlternates = true;
				if (names.Count == 1)
				{
					string name = names[0];
					string suffix = " " + GetBiomeDisplayName(biome);
					cachedText = name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ? name : name + suffix;
				}
				else
				{
					List<string> shortNames = new List<string>();
					foreach (string name in names) shortNames.Add(StripBiomeSuffix(name, biome));
					cachedText = GetBiomeDisplayName(biome) + ": " + string.Join(", ", shortNames);
				}
			}
			catch (Exception ex)
			{
				log.Warn($"Could not determine alternate biome names: {ex.Message}");
				cachedHasAlternates = false;
				cachedText = GetBiomeDisplayName(biome);
			}

			hasAlternates = cachedHasAlternates;
			return cachedText;
		}

		private static string StripBiomeSuffix(string name, string biome)
		{
			string[] suffixes = { GetBiomeDisplayName(biome), biome };

			foreach (string suffix in suffixes)
			{
				if (name.EndsWith(" " + suffix, StringComparison.OrdinalIgnoreCase))
					return name.Substring(0, name.Length - suffix.Length - 1).Trim();
			}

			return name;
		}

		internal static string WrapDisplayName(Text text, string value, float maxWidth)
		{
			if (text == null || string.IsNullOrEmpty(value) || !value.Contains(", ")) return value;

			// Preserve the complete name; choose the comma boundary nearest the end of the available first line.
			TextGenerator generator = new TextGenerator();
			TextGenerationSettings settings = text.GetGenerationSettings(new Vector2(maxWidth, 0f));
			settings.horizontalOverflow = HorizontalWrapMode.Overflow;

			if (generator.GetPreferredWidth(value, settings) / text.pixelsPerUnit <= maxWidth) return value;

			int breakAt = -1;
			int cursor = value.IndexOf(", ", StringComparison.Ordinal);
			while (cursor >= 0)
			{
				string candidate = value.Substring(0, cursor + 1);
				if (generator.GetPreferredWidth(candidate, settings) / text.pixelsPerUnit <= maxWidth) breakAt = cursor + 1;
				cursor = value.IndexOf(", ", cursor + 2, StringComparison.Ordinal);
			}

			if (breakAt < 0) breakAt = value.IndexOf(", ", StringComparison.Ordinal) + 1;
			return value.Substring(0, breakAt).TrimEnd() + "\n" + value.Substring(breakAt).TrimStart();
		}
	}
}
