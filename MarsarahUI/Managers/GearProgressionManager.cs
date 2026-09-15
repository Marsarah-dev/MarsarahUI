using System.Collections.Generic;
using UnityEngine;

namespace MarsarahUI.Managers
{
	internal static class GearProgressionManager
	{
		private static readonly LogManager log = new LogManager("Gear Progression", LogManager.LogLevel.Warning);

		internal enum ThreatLevel
		{
			Safe,
			Caution,
			Elevated,
			Dangerous,
			Extreme
		}

		private struct BiomeWeights
		{
			public BiomeWeights(int min, int max)
			{
				Min = min;
				Max = max;
			}

			public int Min;
			public int Max;
		}

		private static readonly Dictionary<string, BiomeWeights> defaultBiomeWeights = new Dictionary<string, BiomeWeights>()
		{
			{ "Meadows", new BiomeWeights(1, 11) },
			{ "BlackForest", new BiomeWeights(11, 24) },
			{ "Swamp", new BiomeWeights(24, 37) },
			{ "Mountain", new BiomeWeights(33, 46) },
			{ "Plains", new BiomeWeights(42, 55) },
			{ "Mistlands", new BiomeWeights(51, 60) },
			{ "AshLands", new BiomeWeights(60, 69) }
		};

		private static readonly Dictionary<string, BiomeWeights> unlockedBiomeWeights = new Dictionary<string, BiomeWeights>()
		{
			{ "Meadows", new BiomeWeights(1, 15) },
			{ "BlackForest", new BiomeWeights(11, 28) },
			{ "Swamp", new BiomeWeights(24, 37) },
			{ "Mountain", new BiomeWeights(33, 46) },
			{ "Plains", new BiomeWeights(42, 55) },
			{ "Mistlands", new BiomeWeights(51, 64) },
			{ "AshLands", new BiomeWeights(60, 73) }
		};

		private static readonly Dictionary<string, int> armorWeights = new Dictionary<string, int>()
		{
			// Capes
			{ "$item_cape_deerhide", 1 },
			{ "$item_cape_trollhide", 1 },
			{ "$item_cape_linen", 1 },
			{ "$item_cape_wolf", 1 },
			{ "$item_cape_lox", 1 },
			{ "$item_cape_feather", 1 },
			{ "$item_cape_ash", 1 },
			{ "$item_cape_asksvin", 1 },

			// Meadows
			{ "$item_chest_rags", 1 },
			{ "$item_legs_rags", 1 },
			{ "$item_helmet_leather", 2 },
			{ "$item_chest_leather", 2 },
			{ "$item_legs_leather", 2 },

			// Black Forest
			{ "$item_helmet_berserker", 5 },
			{ "$item_chest_berserker", 5 },
			{ "$item_legs_berserker", 5 },
			{ "$item_helmet_trollleather", 5 },
			{ "$item_chest_trollleather", 5 },
			{ "$item_legs_trollleather", 5 },
			{ "$item_helmet_bronze", 5 },
			{ "$item_chest_bronze", 5 },
			{ "$item_legs_bronze", 5 },

			// Swamp
			{ "$item_helmet_root", 8 },
			{ "$item_chest_root", 8 },
			{ "$item_legs_root", 8 },
			{ "$item_helmet_iron", 8 },
			{ "$item_chest_iron", 8 },
			{ "$item_legs_iron", 8 },

			// Mountain
			{ "$item_helmet_fenris", 11 },
			{ "$item_chest_fenris", 11 },
			{ "$item_legs_fenris", 11 },
			{ "$item_helmet_drake", 11 },
			{ "$item_chest_wolf", 11 },
			{ "$item_legs_wolf", 11 },

			// Plains
			{ "$item_helmet_berserker_undead", 14 },
			{ "$item_chest_berserker_undead", 14 },
			{ "$item_legs_berserker_undead", 14 },
			{ "$item_helmet_lox", 14 },
			{ "$item_chest_lox", 14 },
			{ "$item_legs_lox", 14 },
			{ "$item_helmet_padded", 14 },
			{ "$item_chest_pcuirass", 14 },
			{ "$item_legs_pgreaves", 14 },

			// Mistlands
			{ "$item_helmet_mage", 17 },
			{ "$item_chest_mage", 17 },
			{ "$item_legs_mage", 17 },
			{ "$item_helmet_carapace", 17 },
			{ "$item_chest_carapace", 17 },
			{ "$item_legs_carapace", 17 },

			// Ashlands
			{ "$item_helmet_mage_ashlands", 20 },
			{ "$item_chest_mage_ashlands", 20 },
			{ "$item_legs_mage_ashlands", 20 },
			{ "$item_helmet_medium_ashlands", 20 },
			{ "$item_chest_medium_ashlands", 20 },
			{ "$item_legs_medium_ashlands", 20 },
			{ "$item_helmet_flametal", 20 },
			{ "$item_chest_flametal", 20 },
			{ "$item_legs_flametal", 20 }
		};

		internal static int GetEquippedArmorWeight(Player player)
		{
			if (player == null) return 0;

			Inventory inventory = player.GetInventory();
			if (inventory == null) return 0;

			int armorWeight = 0;

			foreach (ItemDrop.ItemData equippedItem in inventory.GetEquippedItems())
			{
				if (equippedItem?.m_shared == null) continue;

				if (armorWeights.TryGetValue(equippedItem.m_shared.m_name, out int baseWeight))
				{
					armorWeight += baseWeight + equippedItem.m_quality - 1;
				}
			}

			return armorWeight;
		}

		internal static ThreatLevel GetThreatLevel(int armorWeight, string progressionBiome)
		{
			Dictionary<string, BiomeWeights> biomeWeights =
				CompatibilityManager.TweaksGearUpgradeUnlockEnabled
					? unlockedBiomeWeights
					: defaultBiomeWeights;

			if (!biomeWeights.TryGetValue(progressionBiome, out BiomeWeights weights))
			{
				log.Warn($"No gear progression weights found for '{progressionBiome}'.");
				return ThreatLevel.Safe;
			}

			float range = weights.Max - weights.Min;
			float startValue = armorWeight - weights.Min;

			float percent = range > 0f
				? startValue * 100f / range
				: 100f;

			if (percent < 0f) return ThreatLevel.Extreme;
			if (percent < 25f) return ThreatLevel.Dangerous;
			if (percent < 75f) return ThreatLevel.Elevated;
			if (percent < 100f) return ThreatLevel.Caution;

			return ThreatLevel.Safe;
		}

		internal static Color GetThreatColor(ThreatLevel threatLevel)
		{
			switch (threatLevel)
			{
				case ThreatLevel.Extreme:
					return new Color(0.298039f, 0f, 0.6f);

				case ThreatLevel.Dangerous:
					return Color.red;

				case ThreatLevel.Elevated:
					return new Color(1f, 0.549019f, 0f);

				case ThreatLevel.Caution:
					return Color.yellow;

				case ThreatLevel.Safe:
				default:
					return Color.green;
			}
		}

		internal static Color GetBiomeColor(int armorWeight, string biome)
		{
			if (string.IsNullOrEmpty(biome)) return Color.white;

			Dictionary<string, BiomeWeights> biomeWeights =
				CompatibilityManager.TweaksGearUpgradeUnlockEnabled
					? unlockedBiomeWeights
					: defaultBiomeWeights;

			if (!biomeWeights.ContainsKey(biome)) return Color.white;

			return GetThreatColor(GetThreatLevel(armorWeight, biome));
		}
	}
}