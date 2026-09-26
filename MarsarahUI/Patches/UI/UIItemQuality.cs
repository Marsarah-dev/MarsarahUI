using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MarsarahUI.Managers.ConfigManager;

namespace MarsarahUI.Patches.UI
{
	internal class UIItemQuality : UIController
	{
		private static readonly LogManager log = new LogManager("UI Item Quality", LogManager.LogLevel.Info);

		private class OriginalStyle
		{
			public string Text;
			public float FontSize;
			public Color Color;
			public TextAlignmentOptions Alignment;
			public Vector2 Pivot;
			public float LineSpacing;
			public Vector2 SizeDelta;
			public Vector2 AnchoredPosition;
			public TextWrappingModes WrappingMode;
			public bool? OutlineEnabled;
		}

		private static readonly Dictionary<TMP_Text, OriginalStyle> originalStyles = new Dictionary<TMP_Text, OriginalStyle>();

		private static readonly FieldInfo inventoryField;
		private static readonly FieldInfo elementsField;
		private static readonly FieldInfo qualityField;

		static UIItemQuality()
		{
			inventoryField = AccessTools.Field(typeof(InventoryGrid), "m_inventory");
			elementsField = AccessTools.Field(typeof(InventoryGrid), "m_elements");

			if (elementsField != null)
			{
				System.Type elementType = elementsField.FieldType.GetGenericArguments()[0];
				qualityField = AccessTools.Field(elementType, "m_quality");
			}

			if (inventoryField == null || elementsField == null || qualityField == null)
			{
				log.Error("Failed to locate one or more fields required for item quality indicators.");
			}
		}

		[HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
		private static class ItemQuality_Patch
		{
			private static void Postfix(InventoryGrid __instance)
			{
				if (ConfigManager.EffectiveItemQualityIndicatorChoice == ItemQualityMode.Off)
				{
					if (originalStyles.Count > 0)
					{
						RestoreAllVanillaStyles();
					}

					return;
				}

				UpdateGrid(__instance);
			}
		}

		public static void UpdateSymbols()
		{
			if (ConfigManager.EffectiveItemQualityIndicatorChoice == ItemQualityMode.Off)
			{
				RestoreAllVanillaStyles();
				return;
			}

			foreach (InventoryGrid grid in Object.FindObjectsByType<InventoryGrid>(FindObjectsSortMode.None))
			{
				UpdateGrid(grid);
			}
		}

		private static void UpdateGrid(InventoryGrid grid)
		{
			if (grid == null || inventoryField == null || elementsField == null || qualityField == null) return;

			Inventory inventory = inventoryField.GetValue(grid) as Inventory;
			if (inventory == null) return;

			IList elements = elementsField.GetValue(grid) as IList;
			if (elements == null) return;

			int width = inventory.GetWidth();

			foreach (ItemDrop.ItemData item in inventory.GetAllItems())
			{
				if (item == null || item.m_shared.m_maxQuality <= 1) continue;

				int index = item.m_gridPos.y * width + item.m_gridPos.x;
				if (index < 0 || index >= elements.Count) continue;

				object element = elements[index];
				if (element == null) continue;

				TMP_Text qualityText = qualityField.GetValue(element) as TMP_Text;
				if (qualityText == null) continue;

				DrawSymbols(qualityText, item.m_quality);
			}
		}

		private static void DrawSymbols(TMP_Text textComponent, int quality)
		{
			if (textComponent == null) return;

			if (!originalStyles.ContainsKey(textComponent))
			{
				Outline originalOutline = textComponent.GetComponent<Outline>();

				originalStyles[textComponent] = new OriginalStyle
				{
					Text = textComponent.text,
					FontSize = textComponent.fontSize,
					Color = textComponent.color,
					Alignment = textComponent.alignment,
					Pivot = textComponent.rectTransform.pivot,
					LineSpacing = textComponent.lineSpacing,
					SizeDelta = textComponent.rectTransform.sizeDelta,
					AnchoredPosition = textComponent.rectTransform.anchoredPosition,
					WrappingMode = textComponent.textWrappingMode,
					OutlineEnabled = originalOutline != null ? originalOutline.enabled : (bool?)null
				};
			}

			bool vertical = ConfigManager.EffectiveItemQualityIndicatorChoice == ItemQualityMode.Vertical;

			char symbol = ConfigManager.EffectiveItemQualitySymbolChoice switch
			{
				ItemQualitySymbol.Star => '★',
				ItemQualitySymbol.Circle => '●',
				ItemQualitySymbol.Diamond => '◆',
				ItemQualitySymbol.EmptyDiamond => '◇',
				_ => '★'
			};

			string symbolText;

			if (quality >= 5)
			{
				symbolText = $"{quality}x {symbol}";
			}
			else
			{
				symbolText = vertical
					? string.Join("\n", new string(symbol, quality).ToCharArray())
					: new string(symbol, quality);
			}

			Color symbolColor = ConfigManager.EffectiveItemQualityColorChoice switch
			{
				ItemQualityColor.White => Color.white,
				ItemQualityColor.Yellow => Color.yellow,
				ItemQualityColor.Green => Color.green,
				ItemQualityColor.Red => Color.red,
				ItemQualityColor.Blue => Color.blue,
				ItemQualityColor.Cyan => Color.cyan,
				_ => Color.yellow
			};

			textComponent.text = symbolText;
			textComponent.color = symbolColor;
			textComponent.fontSize = 7f;
			textComponent.textWrappingMode = TextWrappingModes.PreserveWhitespaceNoWrap;
			textComponent.alignment = vertical ? TextAlignmentOptions.TopRight : TextAlignmentOptions.MidlineRight;
			textComponent.rectTransform.pivot = vertical ? new Vector2(1f, 1f) : new Vector2(1f, 0.5f);
			textComponent.lineSpacing = vertical ? -4f : 0f;

			float characterSize = textComponent.fontSize;
			float baseWidth = characterSize * 2f;
			float baseHeight = characterSize * 2f;

			float width = vertical
				? baseWidth
				: Mathf.Max(quality * characterSize, baseWidth);

			float height = vertical
				? Mathf.Max(quality * characterSize, baseHeight)
				: baseHeight;

			textComponent.rectTransform.sizeDelta = new Vector2(width, height);
			textComponent.rectTransform.anchoredPosition = new Vector2(-2f, vertical ? 0f : -9f);

			Outline outline = textComponent.GetComponent<Outline>();
			if (outline != null)
			{
				outline.enabled = false;
			}
		}

		private static void RestoreAllVanillaStyles()
		{
			foreach (KeyValuePair<TMP_Text, OriginalStyle> pair in originalStyles)
			{
				TMP_Text textComponent = pair.Key;
				OriginalStyle originalStyle = pair.Value;

				if (textComponent == null) continue;

				textComponent.text = originalStyle.Text;
				textComponent.fontSize = originalStyle.FontSize;
				textComponent.color = originalStyle.Color;
				textComponent.alignment = originalStyle.Alignment;
				textComponent.rectTransform.pivot = originalStyle.Pivot;
				textComponent.lineSpacing = originalStyle.LineSpacing;
				textComponent.rectTransform.sizeDelta = originalStyle.SizeDelta;
				textComponent.rectTransform.anchoredPosition = originalStyle.AnchoredPosition;
				textComponent.textWrappingMode = originalStyle.WrappingMode;

				Outline outline = textComponent.GetComponent<Outline>();

				if (outline != null && originalStyle.OutlineEnabled.HasValue)
				{
					outline.enabled = originalStyle.OutlineEnabled.Value;
				}
			}

			originalStyles.Clear();

			log.Info("Restored vanilla item quality indicators.");
		}
	}
}