using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIBiomeCraftingTabs
	{
		private static readonly LogManager log = new LogManager("UI Biome Crafting Tabs", LogManager.LogLevel.Info);

		private enum CraftingTab
		{
			All,
			Meadows,
			BlackForest,
			Swamp,
			Mountain,
			Plains,
			Ocean,
			Mistlands,
			Ashlands,
			DeepNorth,
			Other
		}

		private static readonly CraftingTab[] tabDisplayOrder =
		{
			CraftingTab.All,
			CraftingTab.Meadows,
			CraftingTab.BlackForest,
			CraftingTab.Swamp,
			CraftingTab.Mountain,
			CraftingTab.Plains,
			CraftingTab.Ocean,
			CraftingTab.Mistlands,
			CraftingTab.Ashlands,
			CraftingTab.DeepNorth,
			CraftingTab.Other
		};

		private static readonly HashSet<CraftingTab> availableTabs = new HashSet<CraftingTab>();
		private static readonly Dictionary<CraftingTab, GameObject> tabObjects = new Dictionary<CraftingTab, GameObject>();

		private static CraftingTab selectedTab = CraftingTab.All;
		private static InventoryGui currentInventoryGui;

		private static bool RecipeMatchesSelectedTab(Recipe recipe)
		{
			if (selectedTab == CraftingTab.All)
				return true;

			if (selectedTab == CraftingTab.Other)
				return !BiomeCraftingManager.TryGetClassification(recipe, out _);

			if (!BiomeCraftingManager.TryGetClassification(recipe, out BiomeCraftingManager.RecipeClassification classification))
				return false;

			return selectedTab.ToString() == classification.Biome.ToString();
		}

		private static void UpdateAvailableTabs(List<Recipe> recipes)
		{
			availableTabs.Clear();
			availableTabs.Add(CraftingTab.All);

			bool hasOther = false;

			foreach (Recipe recipe in recipes)
			{
				if (!BiomeCraftingManager.TryGetClassification(recipe, out BiomeCraftingManager.RecipeClassification classification))
				{
					hasOther = true;
					continue;
				}

				switch (classification.Biome)
				{
					case BiomeCraftingManager.CraftingBiome.Meadows:
						availableTabs.Add(CraftingTab.Meadows);
						break;

					case BiomeCraftingManager.CraftingBiome.BlackForest:
						availableTabs.Add(CraftingTab.BlackForest);
						break;

					case BiomeCraftingManager.CraftingBiome.Swamp:
						availableTabs.Add(CraftingTab.Swamp);
						break;

					case BiomeCraftingManager.CraftingBiome.Mountain:
						availableTabs.Add(CraftingTab.Mountain);
						break;

					case BiomeCraftingManager.CraftingBiome.Plains:
						availableTabs.Add(CraftingTab.Plains);
						break;

					case BiomeCraftingManager.CraftingBiome.Ocean:
						availableTabs.Add(CraftingTab.Ocean);
						break;

					case BiomeCraftingManager.CraftingBiome.Mistlands:
						availableTabs.Add(CraftingTab.Mistlands);
						break;

					case BiomeCraftingManager.CraftingBiome.Ashlands:
						availableTabs.Add(CraftingTab.Ashlands);
						break;

					case BiomeCraftingManager.CraftingBiome.DeepNorth:
						availableTabs.Add(CraftingTab.DeepNorth);
						break;
				}
			}

			if (hasOther)
				availableTabs.Add(CraftingTab.Other);
		}

		private static GameObject GetInventoryGuiObject(InventoryGui inventoryGui, string fieldName)
		{
			object fieldValue = AccessTools.Field(typeof(InventoryGui), fieldName)?.GetValue(inventoryGui);

			if (fieldValue is GameObject gameObject)
				return gameObject;

			if (fieldValue is Component component)
				return component.gameObject;

			return null;
		}

		private static string GetTabLabel(CraftingTab tab)
		{
			switch (tab)
			{
				case CraftingTab.All:
					return "ALL";
				case CraftingTab.Meadows:
					return "MEADOWS";
				case CraftingTab.BlackForest:
					return "BLACK FOREST";
				case CraftingTab.Swamp:
					return "SWAMP";
				case CraftingTab.Mountain:
					return "MOUNTAIN";
				case CraftingTab.Plains:
					return "PLAINS";
				case CraftingTab.Ocean:
					return "OCEAN";
				case CraftingTab.Mistlands:
					return "MISTLANDS";
				case CraftingTab.Ashlands:
					return "ASHLANDS";
				case CraftingTab.DeepNorth:
					return "DEEP NORTH";
				case CraftingTab.Other:
					return "OTHER";
				default:
					return tab.ToString().ToUpperInvariant();
			}
		}

		private static void CreateBiomeTabs(InventoryGui inventoryGui)
		{
			GameObject craftTab = GetInventoryGuiObject(inventoryGui, "m_tabCraft");
			GameObject upgradeTab = GetInventoryGuiObject(inventoryGui, "m_tabUpgrade");

			if (craftTab == null || upgradeTab == null)
			{
				log.Error("Could not find vanilla Craft/Upgrade tab objects.");
				return;
			}

			if (currentInventoryGui == inventoryGui && tabObjects.Count > 0)
				return;

			currentInventoryGui = inventoryGui;
			tabObjects.Clear();

			RectTransform craftRect = craftTab.GetComponent<RectTransform>();

			if (craftRect == null)
			{
				log.Error("Could not find RectTransform on vanilla Craft tab.");
				return;
			}

			foreach (CraftingTab tab in tabDisplayOrder)
			{
				GameObject tabObject = Object.Instantiate(upgradeTab, upgradeTab.transform.parent);
				tabObject.name = $"MarsarahBiomeTab_{tab}";

				Button button = tabObject.GetComponentInChildren<Button>(true);

				if (button != null)
				{
					button.onClick.RemoveAllListeners();

					Navigation navigation = button.navigation;
					navigation.mode = Navigation.Mode.None;
					button.navigation = navigation;
				}

				TMP_Text text = tabObject.GetComponentInChildren<TMP_Text>(true);

				if (text != null)
					text.text = GetTabLabel(tab);

				tabObjects[tab] = tabObject;
			}

			log.Info("Created biome crafting tab objects.");
		}

		private static void UpdateBiomeTabs(InventoryGui inventoryGui)
		{
			CreateBiomeTabs(inventoryGui);

			if (tabObjects.Count == 0)
				return;

			GameObject craftTab = GetInventoryGuiObject(inventoryGui, "m_tabCraft");
			RectTransform craftRect = craftTab != null ? craftTab.GetComponent<RectTransform>() : null;

			if (craftRect == null)
				return;

			const float tabWidth = 100f;
			const float tabSpacing = 4f;
			float rowY = craftRect.anchoredPosition.y - craftRect.rect.height - 4f;
			int visibleIndex = 0;

			foreach (CraftingTab tab in tabDisplayOrder)
			{
				GameObject tabObject = tabObjects[tab];
				bool visible = availableTabs.Contains(tab);

				tabObject.SetActive(visible);

				if (!visible)
					continue;

				RectTransform tabRect = tabObject.GetComponent<RectTransform>();

				if (tabRect != null)
				{
					tabRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tabWidth);
					tabRect.anchoredPosition = new Vector2(craftRect.anchoredPosition.x + visibleIndex * (tabWidth + tabSpacing), rowY);
				}

				visibleIndex++;
			}
		}

		[HarmonyPatch(typeof(InventoryGui), "UpdateRecipeList")]
		private static class UpdateRecipeList_Patch
		{
			private static void Prefix(InventoryGui __instance, List<Recipe> recipes)
			{
				if (!ConfigManager.EffectiveBiomeSortedCraftingTabs) return;
				if (__instance == null || recipes == null) return;
				if (!__instance.InCraftTab()) return;

				int originalCount = recipes.Count;

				UpdateAvailableTabs(recipes);
				UpdateBiomeTabs(__instance);

				log.Info($"Available crafting tabs: {string.Join(", ", availableTabs)}");

				recipes.RemoveAll(recipe => !RecipeMatchesSelectedTab(recipe));

				log.Info($"{selectedTab} crafting filter: {originalCount} -> {recipes.Count} recipes");
			}
		}
	}
}