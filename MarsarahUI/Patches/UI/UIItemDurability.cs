using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIItemDurability : UIController
	{
		private static readonly LogManager log = new LogManager("UI Item Durability", LogManager.LogLevel.Info);

		private static readonly FieldInfo hotkeyItemsField;
		private static readonly FieldInfo hotkeyElementsField;
		private static readonly FieldInfo hotkeyDurabilityField;

		private static readonly FieldInfo inventoryField;
		private static readonly FieldInfo elementsField;
		private static readonly FieldInfo durabilityField;

		private static Sprite customSprite;
		private static readonly Dictionary<Image, Sprite> originalSprites = new Dictionary<Image, Sprite>();

		private static bool wasEnabled;

		static UIItemDurability()
		{
			hotkeyItemsField = AccessTools.Field(typeof(HotkeyBar), "m_items");
			hotkeyElementsField = AccessTools.Field(typeof(HotkeyBar), "m_elements");

			Type hotkeyElementType = typeof(HotkeyBar).GetNestedType("ElementData", BindingFlags.NonPublic);
			hotkeyDurabilityField = hotkeyElementType != null ? AccessTools.Field(hotkeyElementType, "m_durability") : null;

			inventoryField = AccessTools.Field(typeof(InventoryGrid), "m_inventory");
			elementsField = AccessTools.Field(typeof(InventoryGrid), "m_elements");

			if (elementsField != null)
			{
				Type elementType = elementsField.FieldType.GetGenericArguments()[0];
				durabilityField = AccessTools.Field(elementType, "m_durability");
			}

			if (hotkeyItemsField == null || hotkeyElementsField == null || hotkeyDurabilityField == null ||
				inventoryField == null || elementsField == null || durabilityField == null)
			{
				log.Error("Failed to locate one or more fields required for item durability bars.");
			}
		}

		[HarmonyPatch(typeof(HotkeyBar), "UpdateIcons")]
		private static class ItemDurability_HotkeyBarPatch
		{
			private static void Postfix(HotkeyBar __instance, Player player)
			{
				if (!ConfigManager.EffectiveColoredItemDurabilityBar)
				{
					if (wasEnabled)
					{
						RestoreOriginalSprites();
						wasEnabled = false;
					}

					return;
				}

				wasEnabled = true;

				if (!player || player.IsDead()) return;
				if (hotkeyItemsField == null || hotkeyElementsField == null || hotkeyDurabilityField == null) return;

				List<ItemDrop.ItemData> items = hotkeyItemsField.GetValue(__instance) as List<ItemDrop.ItemData>;
				IList elements = hotkeyElementsField.GetValue(__instance) as IList;

				if (items == null || elements == null) return;

				foreach (ItemDrop.ItemData item in items)
				{
					if (item == null || !item.m_shared.m_useDurability) continue;

					int index = item.m_gridPos.x;
					if (index < 0 || index >= elements.Count) continue;

					object elementData = elements[index];
					if (elementData == null) continue;

					GuiBar durabilityBar = hotkeyDurabilityField.GetValue(elementData) as GuiBar;
					if (durabilityBar == null) continue;

					ApplyDurabilityStyle(durabilityBar, item);
				}
			}
		}

		[HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
		private static class ItemDurability_InventoryGridPatch
		{
			private static void Postfix(InventoryGrid __instance)
			{
				if (!ConfigManager.EffectiveColoredItemDurabilityBar)
				{
					if (wasEnabled)
					{
						RestoreOriginalSprites();
						wasEnabled = false;
					}

					return;
				}

				wasEnabled = true;

				if (inventoryField == null || elementsField == null || durabilityField == null) return;

				Inventory inventory = inventoryField.GetValue(__instance) as Inventory;
				if (inventory == null) return;

				IList elements = elementsField.GetValue(__instance) as IList;
				if (elements == null) return;

				int width = inventory.GetWidth();

				foreach (ItemDrop.ItemData item in inventory.GetAllItems())
				{
					if (item == null || !item.m_shared.m_useDurability) continue;

					int index = item.m_gridPos.y * width + item.m_gridPos.x;
					if (index < 0 || index >= elements.Count) continue;

					object element = elements[index];
					if (element == null) continue;

					GuiBar durabilityBar = durabilityField.GetValue(element) as GuiBar;
					if (durabilityBar == null) continue;

					ApplyDurabilityStyle(durabilityBar, item);
				}
			}
		}

		private static void ApplyDurabilityStyle(GuiBar durabilityBar, ItemDrop.ItemData item)
		{
			Image barImage = durabilityBar.m_bar?.GetComponent<Image>();
			if (barImage == null) return;

			float durabilityPercent = item.GetDurabilityPercentage();
			durabilityBar.SetColor(GetDurabilityColor(durabilityPercent));

			Sprite sprite = GetCustomSprite();

			if (sprite != null)
			{
				if (!originalSprites.ContainsKey(barImage))
				{
					originalSprites[barImage] = barImage.sprite;
				}

				if (barImage.sprite != sprite)
				{
					barImage.sprite = sprite;
				}
			}

			FlashDurabilityBarAtZero(durabilityBar, item);
		}

		private static Color GetDurabilityColor(float percent)
		{
			if (percent > 0.5f)
			{
				return Color.Lerp(Color.yellow, Color.green, (percent - 0.5f) * 2f);
			}

			return Color.Lerp(Color.red, Color.yellow, percent * 2f);
		}

		private static void FlashDurabilityBarAtZero(GuiBar bar, ItemDrop.ItemData item)
		{
			if (bar == null || item == null || bar.m_bar == null) return;
			if (item.GetDurabilityPercentage() > 0f) return;

			Image barImage = bar.m_bar.GetComponent<Image>();
			if (barImage == null) return;

			Color color = barImage.color;
			color.a = 0.5f + 0.5f * Mathf.Sin(Time.time * 10f);
			barImage.color = color;
		}

		private static void RestoreOriginalSprites()
		{
			foreach (KeyValuePair<Image, Sprite> pair in originalSprites)
			{
				if (pair.Key != null)
				{
					pair.Key.sprite = pair.Value;
				}
			}

			originalSprites.Clear();

			log.Info("Restored vanilla durability bar sprites.");
		}

		private static Sprite GetCustomSprite()
		{
			if (customSprite != null) return customSprite;

			customSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == "bar_stagger");

			if (customSprite == null)
			{
				log.Warn("Could not find custom durability bar sprite.");
			}

			return customSprite;
		}
	}
}