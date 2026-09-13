using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UISmartBiome : UIController
	{
		private static readonly LogManager log = new LogManager("UI Smart Biome", LogManager.LogLevel.Warning);

		private static int playerArmorWeight;
		private static string currentBiome;

		private static Text UIBiomeText;

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

		[HarmonyPatch(typeof(Player), "Update")]
		private static class SmartBiome_PlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_localPlayer == null) return;
				if (!ConfigManager.EffectiveShowSmartBiome || !ShowUI) return;

				Inventory inventory = ___m_localPlayer.GetInventory();
				if (inventory == null) return;

				playerArmorWeight = 0;

				foreach (ItemDrop.ItemData equippedItem in inventory.GetEquippedItems())
				{
					if (equippedItem?.m_shared == null) continue;

					if (armorWeights.TryGetValue(equippedItem.m_shared.m_name, out int baseWeight))
					{
						playerArmorWeight += baseWeight + equippedItem.m_quality - 1;
					}
				}
			}
		}

		[HarmonyPatch(typeof(Minimap), "UpdateBiome")]
		private static class MoveBiomeMinimapText_Patch
		{
			private static void Prefix(ref Text ___m_biomeNameSmall, ref Player player)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_biomeNameSmall == null || player == null) return;

				bool showSmartBiome = ConfigManager.EffectiveShowSmartBiome && ShowUI;

				___m_biomeNameSmall.enabled = !showSmartBiome;

				if (showSmartBiome)
				{
					currentBiome = player.GetCurrentBiome().ToString();
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class SmartBiome_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (!ConfigManager.EffectiveShowSmartBiome)
				{
					if (UIBiomeText != null)
					{
						UIBiomeText.enabled = false;
					}

					return;
				}

				CreateUI(__instance);

				bool minimapVisible =
					Minimap.instance != null &&
					Minimap.instance.m_mapSmall.activeInHierarchy;

				UIBiomeText.enabled = ShowUI && minimapVisible;

				if (!UIBiomeText.enabled) return;

				Dictionary<string, BiomeWeights> biomeWeights =	CompatibilityManager.TweaksGearUpgradeUnlockEnabled	? unlockedBiomeWeights : defaultBiomeWeights;

				if (currentBiome != null && biomeWeights.TryGetValue(currentBiome, out BiomeWeights weights))
				{
					float range = weights.Max - weights.Min;
					float startValue = playerArmorWeight - weights.Min;

					float playerArmorWeightPercent = range > 0f
						? startValue * 100f / range
						: 100f;

					UIBiomeText.color = GetColorFromPercent(playerArmorWeightPercent);
				}
				else
				{
					UIBiomeText.color = Color.white;
				}

				UIBiomeText.text = GetBiomeDisplayName(currentBiome);
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UIBiomeText != null) return;

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			Vector2 UIBiomeAreaSize = new Vector2(150f, 30f);

			GameObject UIBiomeArea = new GameObject("BiomeArea");
			UIBiomeArea.layer = 5;
			UIBiomeArea.transform.SetParent(hud.m_rootObject.transform);

			RectTransform biomeAreaTransform = UIBiomeArea.AddComponent<RectTransform>();
			biomeAreaTransform.anchorMin = new Vector2(1f, 1f);
			biomeAreaTransform.anchorMax = new Vector2(1f, 1f);
			biomeAreaTransform.anchoredPosition = new Vector2(-125f, -55f);
			biomeAreaTransform.sizeDelta = UIBiomeAreaSize;

			UIBiomeArea.transform.localScale = Vector3.one;

			UIBiomeText = CreateTextObject("BiomeText", UIBiomeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, Vector2.zero, UIBiomeAreaSize);
		}

		private static string GetBiomeDisplayName(string biome)
		{
			switch (biome)
			{
				case "BlackForest":
					return "Black forest";

				case "AshLands":
					return "Ashlands";

				default:
					return biome ?? "";
			}
		}

		private static Color GetColorFromPercent(float percent)
		{
			if (percent < 0f) return new Color(0.298039f, 0f, 0.6f);
			if (percent < 25f) return Color.red;
			if (percent < 75f) return new Color(1f, 0.549019f, 0f);
			if (percent < 100f) return Color.yellow;

			return Color.green;
		}
	}
}