using HarmonyLib;
using MarsarahUI.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIAlternateBiomeNames : UIController
	{
		private static readonly LogManager log = new LogManager("UI Alternate Biome Names", LogManager.LogLevel.Info);

		private static Text vanillaBiomeText;
		private static Vector2 vanillaSize;
		private static Vector2 vanillaPosition;
		private static bool vanillaLayoutSaved;
		private static TextAnchor vanillaAlignment;
		private static HorizontalWrapMode vanillaHorizontalOverflow;
		private static VerticalWrapMode vanillaVerticalOverflow;
		private static bool vanillaBestFit;
		private static int vanillaMinSize;
		private static int vanillaMaxSize;
		private const float BiomeAreaWidth = 290f;
		private const float BiomeAreaHeight = 52f;

		[HarmonyPatch(typeof(Minimap), "UpdateBiome")]
		private static class AlternateBiome_MinimapPatch
		{
			private static void Postfix(ref Text ___m_biomeNameSmall, Player player)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_biomeNameSmall == null) return;

				// Smart Biome owns the visible label when enabled.
				if (ConfigManager.EffectiveShowSmartBiome && ShowUI)
				{
					RestoreVanillaLayout(___m_biomeNameSmall);
					return;
				}

				if (!ConfigManager.EffectiveShowAlternateBiomeNames || player == null)
				{
					RestoreVanillaLayout(___m_biomeNameSmall);
					return;
				}

				string displayName = BiomeDisplayManager.GetDisplayName(player, out bool hasAlternates);
				if (!hasAlternates)
				{
					RestoreVanillaLayout(___m_biomeNameSmall);
					return;
				}

				UpdateVanillaLayout(___m_biomeNameSmall);
				___m_biomeNameSmall.text = BiomeDisplayManager.WrapDisplayName(___m_biomeNameSmall, displayName, BiomeAreaWidth);
			}
		}

		private static void UpdateVanillaLayout(Text text)
		{
			if (vanillaBiomeText != text)
			{
				vanillaBiomeText = text;
				vanillaLayoutSaved = false;
			}

			RectTransform rect = text.rectTransform;
			if (!vanillaLayoutSaved)
			{
				vanillaSize = rect.sizeDelta;
				vanillaPosition = rect.anchoredPosition;
				vanillaAlignment = text.alignment;
				vanillaHorizontalOverflow = text.horizontalOverflow;
				vanillaVerticalOverflow = text.verticalOverflow;
				vanillaBestFit = text.resizeTextForBestFit;
				vanillaMinSize = text.resizeTextMinSize;
				vanillaMaxSize = text.resizeTextMaxSize;
				vanillaLayoutSaved = true;
				log.Info("Captured vanilla biome label layout.");
			}

			Vector2 difference = new Vector2(BiomeAreaWidth - vanillaSize.x, BiomeAreaHeight - vanillaSize.y);
			rect.sizeDelta = new Vector2(BiomeAreaWidth, BiomeAreaHeight);
			rect.anchoredPosition = vanillaPosition - new Vector2(difference.x * (1f - rect.pivot.x), difference.y * (1f - rect.pivot.y));
			text.alignment = TextAnchor.UpperRight;
			text.horizontalOverflow = HorizontalWrapMode.Wrap;
			text.verticalOverflow = VerticalWrapMode.Truncate;
			text.resizeTextForBestFit = true;
			text.resizeTextMinSize = 12;
			text.resizeTextMaxSize = text.fontSize;
		}

		private static void RestoreVanillaLayout(Text text)
		{
			if (!vanillaLayoutSaved || vanillaBiomeText != text) return;
			text.rectTransform.sizeDelta = vanillaSize;
			text.rectTransform.anchoredPosition = vanillaPosition;
			text.alignment = vanillaAlignment;
			text.horizontalOverflow = vanillaHorizontalOverflow;
			text.verticalOverflow = vanillaVerticalOverflow;
			text.resizeTextForBestFit = vanillaBestFit;
			text.resizeTextMinSize = vanillaMinSize;
			text.resizeTextMaxSize = vanillaMaxSize;
			vanillaLayoutSaved = false;
		}
	}
}
