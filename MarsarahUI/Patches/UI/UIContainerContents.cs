using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static MarsarahUI.Managers.ConfigManager;

namespace MarsarahUI.Patches.UI
{
	internal class UIContainerContents : UIController
	{
		private static readonly LogManager log = new LogManager("UI Container Contents", LogManager.LogLevel.Warning);

		private static GameObject containerContentsArea;
		private static readonly List<Image> itemIcons = new List<Image>();
		private static readonly List<Text> itemCounts = new List<Text>();
		private static Text othersText;

		private const int MaxDisplayedItems = 10;

		private const float IconSize = 32f;
		private const float IconSpacing = 3f;
		private const int CountFontSize = 12;
		private const int OthersFontSize = 11;

		private const int HorizontalColumns = 5;
		private const int VerticalRows = 5;

		private const float GridLeftOffset = -1f;
		private const float GridTopOffset = 20f;
		private const float OthersGap = 7f;

		private class ContainerItemEntry
		{
			internal ItemDrop.ItemData Item;
			internal string LocalizedName;
			internal int Count;

			internal ContainerItemEntry(ItemDrop.ItemData item, string localizedName)
			{
				Item = item;
				LocalizedName = localizedName;
				Count = 0;
			}
		}

		[HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
		private static class HudUpdateCrosshairPatch
		{
			private static void Prefix()
			{
				HideIcons();
			}
		}

		private static List<ContainerItemEntry> GetContainerItems(Inventory inventory)
		{
			List<ContainerItemEntry> items = new List<ContainerItemEntry>();
			Dictionary<string, ContainerItemEntry> itemLookup = new Dictionary<string, ContainerItemEntry>();

			foreach (ItemDrop.ItemData item in inventory.GetAllItems())
			{
				if (item?.m_shared == null) continue;

				string itemKey = item.m_shared.m_name;

				if (!itemLookup.TryGetValue(itemKey, out ContainerItemEntry entry))
				{
					string localizedName = Localization.instance.Localize(item.m_shared.m_name);

					entry = new ContainerItemEntry(item, localizedName);
					itemLookup.Add(itemKey, entry);
					items.Add(entry);
				}

				entry.Count += item.m_stack;
			}

			return items;
		}

		internal static string GetContainerInventoryList(Container container, Inventory inventory)
		{
			ContainerContentsMode mode = ConfigManager.EffectiveContainerContentsChoice;

			if (mode == ContainerContentsMode.Off) return "";
			if (CompatibilityManager.TweaksIsContainerSealed(container)) return "This chest is sealed.";

			List<ContainerItemEntry> items = GetContainerItems(inventory);

			if (items.Count == 0) return "";

			if (mode == ContainerContentsMode.IconsHorizontal || mode == ContainerContentsMode.IconsVertical)
			{
				ShowIcons(items, mode);
				return "";
			}

			if (mode != ContainerContentsMode.Text) return "";

			if (ConfigManager.EffectiveContainerContentsChoice != ContainerContentsMode.Text) return "";

			StringBuilder stringBuilder = new StringBuilder();
			int shown = 0;
			int total = items.Count;

			int maxCountLength = 2;

			for (int i = 0; i < Mathf.Min(items.Count, 10); i++)
			{
				maxCountLength = Mathf.Max(maxCountLength, items[i].Count.ToString().Length);
			}

			if (total > 10)
			{
				maxCountLength = Mathf.Max(maxCountLength, $"+{total - 10}".Length);
			}

			int namePosition = 25 + Mathf.Max(0, maxCountLength - 2) * 10;

			foreach (ContainerItemEntry item in items)
			{
				if (shown >= 10) break;

				string countColored = UIDetailedHovers.PaintTextIfEnabled(item.Count.ToString(), Color.yellow);
				string nameColored = UIDetailedHovers.PaintTextIfEnabled(item.LocalizedName, Color.gray);

				stringBuilder.AppendLine($"{countColored}<pos={namePosition}>{nameColored}");
				shown++;
			}

			if (total > 10)
			{
				string number = UIDetailedHovers.PaintTextIfEnabled($"+{total - 10}", Color.yellow);
				string others = UIDetailedHovers.PaintTextIfEnabled("Others", Color.gray);

				stringBuilder.AppendLine($"{number}<pos={namePosition}>{others}");
			}

			return stringBuilder.ToString().TrimEnd();
		}

		private static void ShowIcons(List<ContainerItemEntry> items, ContainerContentsMode mode)
		{
			if (Hud.instance == null || Hud.instance.m_hoverName == null) return;

			EnsureIconUI();

			if (containerContentsArea == null) return;

			ConfigureIconLayout(mode);

			containerContentsArea.SetActive(true);

			int shown = Mathf.Min(items.Count, MaxDisplayedItems);

			PositionIcons(shown, mode);

			for (int i = 0; i < MaxDisplayedItems; i++)
			{
				if (i < shown)
				{
					UpdateItemIcon(items[i], i);
					itemIcons[i].gameObject.SetActive(true);
				}
				else
				{
					itemIcons[i].gameObject.SetActive(false);
				}
			}

			int remaining = items.Count - MaxDisplayedItems;

			if (remaining > 0)
			{
				othersText.text = $"+{remaining} Others";
				othersText.gameObject.SetActive(true);
			}
			else
			{
				othersText.gameObject.SetActive(false);
			}
		}

		private static void EnsureIconUI()
		{
			if (containerContentsArea != null) return;
			if (Hud.instance == null || Hud.instance.m_hoverName == null) return;

			containerContentsArea = new GameObject("ContainerContentsArea");
			containerContentsArea.layer = 5;
			containerContentsArea.transform.SetParent(Hud.instance.m_hoverName.transform, false);

			RectTransform areaTransform = containerContentsArea.AddComponent<RectTransform>();
			areaTransform.anchorMin = new Vector2(0.5f, 0.5f);
			areaTransform.anchorMax = new Vector2(0.5f, 0.5f);
			areaTransform.pivot = new Vector2(0.5f, 1f);
			areaTransform.anchoredPosition = Vector2.zero;
			areaTransform.sizeDelta = Vector2.zero;

			itemIcons.Clear();
			itemCounts.Clear();

			for (int i = 0; i < MaxDisplayedItems; i++)
			{
				CreateIconSlot(i);
			}

			othersText = CreateTextObject("ContainerContentsOthers", containerContentsArea, Color.white, "AveriaSansLibre-Bold", OthersFontSize, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(220f, 25f));
			othersText.raycastTarget = false;

			RectTransform othersTransform = othersText.GetComponent<RectTransform>();
			othersTransform.anchorMin = new Vector2(0.5f, 1f);
			othersTransform.anchorMax = new Vector2(0.5f, 1f);
			othersTransform.pivot = new Vector2(0f, 1f);

			othersText.gameObject.SetActive(false);
		}

		private static void CreateIconSlot(int index)
		{
			Image icon = CreateUIImageObject($"ContainerItemIcon_{index}", containerContentsArea, Vector2.zero, new Vector2(IconSize, IconSize));

			RectTransform iconTransform = icon.GetComponent<RectTransform>();
			iconTransform.anchorMin = new Vector2(0.5f, 1f);
			iconTransform.anchorMax = new Vector2(0.5f, 1f);
			iconTransform.pivot = new Vector2(0.5f, 1f);

			icon.preserveAspect = true;
			icon.raycastTarget = false;

			Text countText = CreateTextObject($"ContainerItemCount_{index}", icon.gameObject, Color.white, "AveriaSansLibre-Bold", CountFontSize, TextAnchor.LowerRight, Vector2.zero, new Vector2(IconSize, IconSize));

			RectTransform countTransform = countText.GetComponent<RectTransform>();
			countTransform.anchorMin = Vector2.zero;
			countTransform.anchorMax = Vector2.one;
			countTransform.offsetMin = Vector2.zero;
			countTransform.offsetMax = Vector2.zero;

			countText.raycastTarget = false;

			itemIcons.Add(icon);
			itemCounts.Add(countText);

			icon.gameObject.SetActive(false);
		}

		private static void ConfigureIconLayout(ContainerContentsMode mode)
		{
			bool vertical = mode == ContainerContentsMode.IconsVertical;

			int columns = vertical ? 2 : HorizontalColumns;
			int rows = vertical ? VerticalRows : 2;

			float gridWidth = columns * IconSize + (columns - 1) * IconSpacing;
			float gridHeight = rows * IconSize + (rows - 1) * IconSpacing;

			RectTransform areaTransform = containerContentsArea.GetComponent<RectTransform>();
			RectTransform hoverTransform = Hud.instance.m_hoverName.GetComponent<RectTransform>();

			float hoverWidth = hoverTransform != null ? hoverTransform.rect.width : 350f;
			float xPosition = -hoverWidth / 2f + gridWidth / 2f + GridLeftOffset;

			areaTransform.anchoredPosition = new Vector2(xPosition, GridTopOffset);
			areaTransform.sizeDelta = new Vector2(gridWidth, gridHeight);

			for (int i = 0; i < itemIcons.Count; i++)
			{
				itemIcons[i].rectTransform.sizeDelta = new Vector2(IconSize, IconSize);
				itemCounts[i].fontSize = CountFontSize;
			}

			othersText.fontSize = OthersFontSize;

			RectTransform othersTransform = othersText.GetComponent<RectTransform>();
			othersTransform.anchoredPosition = new Vector2(-gridWidth / 2f, -(gridHeight + OthersGap));
		}

		private static void PositionIcons(int shown, ContainerContentsMode mode)
		{
			bool vertical = mode == ContainerContentsMode.IconsVertical;

			int columns = vertical ? 2 : HorizontalColumns;

			float gridWidth = columns * IconSize + (columns - 1) * IconSpacing;

			for (int i = 0; i < shown; i++)
			{
				int row;
				int column;

				if (vertical)
				{
					row = i % VerticalRows;
					column = i / VerticalRows;
				}
				else
				{
					row = i / HorizontalColumns;
					column = i % HorizontalColumns;
				}

				float x = -gridWidth / 2f + IconSize / 2f + column * (IconSize + IconSpacing);
				float y = -row * (IconSize + IconSpacing);

				itemIcons[i].rectTransform.anchoredPosition = new Vector2(x, y);
			}
		}

		private static void UpdateItemIcon(ContainerItemEntry entry, int index)
		{
			Sprite sprite = entry.Item.GetIcon();

			if (sprite == null)
			{
				log.Warn($"No icon found for container item '{entry.LocalizedName}'.");
				itemIcons[index].sprite = null;
				itemCounts[index].text = "";
				return;
			}

			itemIcons[index].sprite = sprite;
			itemCounts[index].text = entry.Count > 1 ? entry.Count.ToString() : "";
		}

		private static void HideIcons()
		{
			if (containerContentsArea == null) return;

			containerContentsArea.SetActive(false);
		}
	}
}