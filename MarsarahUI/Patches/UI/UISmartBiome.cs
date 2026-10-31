using HarmonyLib;
using MarsarahUI.Managers;
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

		private const float ArmorUpdateInterval = 0.5f;
		private static float nextArmorUpdateTime;
		private const float BiomeAreaWidth = 290f;

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

		[HarmonyPatch(typeof(Player), "Update")]
		private static class SmartBiome_PlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_localPlayer == null) return;
				if (!ConfigManager.EffectiveShowSmartBiome || !ShowUI) return;
				if (Time.unscaledTime < nextArmorUpdateTime) return;

				nextArmorUpdateTime = Time.unscaledTime + ArmorUpdateInterval;
				playerArmorWeight = GearProgressionManager.GetEquippedArmorWeight(___m_localPlayer);
			}
		}

		[HarmonyPatch(typeof(Minimap), "UpdateBiome")]
		private static class MoveBiomeMinimapText_Patch
		{
			private static void Prefix(ref Text ___m_biomeNameSmall, ref Player player)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (!ConfigManager.EffectiveShowSmartBiome)
				{
					if (___m_biomeNameSmall != null && !___m_biomeNameSmall.enabled)
					{
						___m_biomeNameSmall.enabled = true;
					}

					return;
				}

				if (___m_biomeNameSmall == null || player == null) return;

				___m_biomeNameSmall.enabled = !ShowUI;

				if (ShowUI)
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

				UIBiomeText.color = GearProgressionManager.GetBiomeColor(playerArmorWeight, currentBiome);

				Player player = Player.m_localPlayer;
				string displayName = BiomeDisplayManager.GetDisplayName(player, out bool hasAlternates);
				UIBiomeText.text = hasAlternates
					? BiomeDisplayManager.WrapDisplayName(UIBiomeText, displayName, BiomeAreaWidth)
					: GetBiomeDisplayName(currentBiome);
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UIBiomeText != null) return;

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			Vector2 UIBiomeAreaSize = new Vector2(BiomeAreaWidth, 52f);

			GameObject UIBiomeArea = new GameObject("BiomeArea");
			UIBiomeArea.layer = 5;
			UIBiomeArea.transform.SetParent(hud.m_rootObject.transform);

			RectTransform biomeAreaTransform = UIBiomeArea.AddComponent<RectTransform>();
			biomeAreaTransform.anchorMin = new Vector2(1f, 1f);
			biomeAreaTransform.anchorMax = new Vector2(1f, 1f);
			biomeAreaTransform.anchoredPosition = new Vector2(-195f, -66f);
			biomeAreaTransform.sizeDelta = UIBiomeAreaSize;

			UIBiomeArea.transform.localScale = Vector3.one;

			UIBiomeText = CreateTextObject("BiomeText", UIBiomeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, Vector2.zero, UIBiomeAreaSize);
			UIBiomeText.horizontalOverflow = HorizontalWrapMode.Wrap;
			UIBiomeText.resizeTextForBestFit = true;
			UIBiomeText.resizeTextMinSize = 12;
			UIBiomeText.resizeTextMaxSize = UITextFontSize;
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
