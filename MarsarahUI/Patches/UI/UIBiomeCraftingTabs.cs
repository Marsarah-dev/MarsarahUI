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
		private static RectTransform craftingContentRoot;
		private static bool craftingLayoutExpanded;

		private const float TabWidth = 85f;
		private const float TabSpacing = 2f;
		private const float TabRowHeight = 34f;

		[HarmonyPatch(typeof(InventoryGui), "UpdateRecipeList")]
		private static class UpdateRecipeList_Patch
		{
			private static void Prefix(InventoryGui __instance, List<Recipe> recipes)
			{
				if (__instance == null || recipes == null) return;

				if (!ConfigManager.EffectiveBiomeSortedCraftingTabs)
				{
					selectedTab = CraftingTab.All;
					SetBiomeTabsVisible(false);
					SetCraftingLayout(false, __instance);
					return;
				}

				if (!__instance.InCraftTab())
				{
					selectedTab = CraftingTab.All;
					SetBiomeTabsVisible(false);
					SetCraftingLayout(false, __instance);
					return;
				}

				SetCraftingLayout(true, __instance);

				int originalCount = recipes.Count;

				UpdateAvailableTabs(recipes);

				if (!availableTabs.Contains(selectedTab))
				{
					log.Info($"Selected crafting tab {selectedTab} is not available at this station. Resetting to All.");
					selectedTab = CraftingTab.All;
				}

				UpdateBiomeTabs(__instance);

				log.Info($"Available crafting tabs: {string.Join(", ", availableTabs)}");

				recipes.RemoveAll(recipe => !RecipeMatchesSelectedTab(recipe));

				log.Info($"{selectedTab} crafting filter: {originalCount} -> {recipes.Count} recipes");
			}
		}

		private static bool RecipeMatchesSelectedTab(Recipe recipe)
		{
			if (selectedTab == CraftingTab.All)
				return true;

			if (selectedTab == CraftingTab.Other)
				return !BiomeCraftingManager.TryGetClassification(recipe, out _);

			if (!BiomeCraftingManager.TryGetClassification(recipe, out BiomeCraftingManager.RecipeClassification classification))
				return false;

			if (!TryGetBiomeForTab(selectedTab, out BiomeCraftingManager.CraftingBiome biome))
				return false;

			return classification.Biome == biome;
		}

		private static bool TryGetBiomeForTab(CraftingTab tab, out BiomeCraftingManager.CraftingBiome biome)
		{
			switch (tab)
			{
				case CraftingTab.Meadows:
					biome = BiomeCraftingManager.CraftingBiome.Meadows;
					return true;

				case CraftingTab.BlackForest:
					biome = BiomeCraftingManager.CraftingBiome.BlackForest;
					return true;

				case CraftingTab.Swamp:
					biome = BiomeCraftingManager.CraftingBiome.Swamp;
					return true;

				case CraftingTab.Mountain:
					biome = BiomeCraftingManager.CraftingBiome.Mountain;
					return true;

				case CraftingTab.Plains:
					biome = BiomeCraftingManager.CraftingBiome.Plains;
					return true;

				case CraftingTab.Ocean:
					biome = BiomeCraftingManager.CraftingBiome.Ocean;
					return true;

				case CraftingTab.Mistlands:
					biome = BiomeCraftingManager.CraftingBiome.Mistlands;
					return true;

				case CraftingTab.Ashlands:
					biome = BiomeCraftingManager.CraftingBiome.Ashlands;
					return true;

				case CraftingTab.DeepNorth:
					biome = BiomeCraftingManager.CraftingBiome.DeepNorth;
					return true;

				default:
					biome = default;
					return false;
			}
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

				availableTabs.Add(CraftingTab.Meadows);
				availableTabs.Add(CraftingTab.BlackForest);
				availableTabs.Add(CraftingTab.Swamp);
				availableTabs.Add(CraftingTab.Mountain);
				availableTabs.Add(CraftingTab.Plains);
				availableTabs.Add(CraftingTab.Ocean);
				availableTabs.Add(CraftingTab.Mistlands);
				availableTabs.Add(CraftingTab.Ashlands);
				availableTabs.Add(CraftingTab.DeepNorth);
				availableTabs.Add(CraftingTab.Other);
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
					button.onClick = new Button.ButtonClickedEvent();

					CraftingTab capturedTab = tab;
					button.onClick.AddListener(() => OnBiomeTabClicked(capturedTab));

					Navigation navigation = button.navigation;
					navigation.mode = Navigation.Mode.None;
					button.navigation = navigation;
				}

				TMP_Text text = tabObject.GetComponentInChildren<TMP_Text>(true);

				if (text != null)
					text.text = GetTabLabel(tab);

				tabObjects[tab] = tabObject;
				tabObject.transform.SetAsLastSibling();
			}

			log.Info("Created biome crafting tab objects.");
		}

		private static void OnBiomeTabClicked(CraftingTab tab)
		{
			if (currentInventoryGui == null)
				return;

			if (!availableTabs.Contains(tab))
				return;

			if (selectedTab == tab)
				return;

			selectedTab = tab;
			UpdateTabSelectionVisuals();

			log.Info($"Selected crafting tab: {selectedTab}");

			RefreshCraftingPanel(currentInventoryGui);
		}

		private static void RefreshCraftingPanel(InventoryGui inventoryGui)
		{
			if (inventoryGui == null)
				return;

			var updateCraftingPanel = AccessTools.Method(typeof(InventoryGui), "UpdateCraftingPanel", new[] { typeof(bool) });

			if (updateCraftingPanel == null)
			{
				log.Error("Could not find InventoryGui.UpdateCraftingPanel.");
				return;
			}

			updateCraftingPanel.Invoke(inventoryGui, new object[] { false });

			EnsureSelectedRecipeVisible(inventoryGui);
		}

		private static void UpdateBiomeTabs(InventoryGui inventoryGui)
		{
			CreateBiomeTabs(inventoryGui);
			GameObject craftTab = GetInventoryGuiObject(inventoryGui, "m_tabCraft");

			if (craftTab == null)
				return;

			RectTransform craftRect = craftTab.GetComponent<RectTransform>();

			if (craftRect == null)
				return;

			foreach (GameObject tabObject in tabObjects.Values)
			{
				tabObject.SetActive(false);
			}

			float startX = craftRect.anchoredPosition.x;
			float currentX = startX;
			float currentY = craftRect.anchoredPosition.y - craftRect.rect.height - 4f;

			float maxWidth = 560f;

			int row = 0;

			foreach (CraftingTab tab in tabDisplayOrder)
			{
				if (!availableTabs.Contains(tab))
					continue;

				GameObject tabObject = tabObjects[tab];

				tabObject.SetActive(true);

				if (tab == CraftingTab.All)
				{
					PositionBiomeTab(tabObject, currentX, currentY);
					currentX += TabWidth + TabSpacing;
					continue;
				}

				if (currentX + TabWidth > startX + maxWidth)
				{
					row++;
					currentX = startX;
					currentY -= TabRowHeight;
				}

				PositionBiomeTab(tabObject, currentX, currentY);

				currentX += TabWidth + TabSpacing;
			}

			UpdateTabSelectionVisuals();
		}

		private static void CreateCraftingContentRoot(InventoryGui inventoryGui)
		{
			if (craftingContentRoot != null)
				return;

			RectTransform craftingRoot = AccessTools.Field(typeof(InventoryGui), "m_crafting")?.GetValue(inventoryGui) as RectTransform;

			if (craftingRoot == null)
			{
				log.Error("Could not find crafting root.");
				return;
			}

			GameObject contentObject = new GameObject("MarsarahBiomeCraftingContent", typeof(RectTransform));
			craftingContentRoot = contentObject.GetComponent<RectTransform>();
			craftingContentRoot.SetParent(craftingRoot, false);

			craftingContentRoot.anchorMin = Vector2.zero;
			craftingContentRoot.anchorMax = Vector2.one;
			craftingContentRoot.offsetMin = Vector2.zero;
			craftingContentRoot.offsetMax = Vector2.zero;

			string[] contentChildren =
			{
				"RecipeList",
				"Decription"
			};

			foreach (string childName in contentChildren)
			{
				Transform child = craftingRoot.Find(childName);

				if (child == null)
				{
					log.Info($"Crafting child '{childName}' was not found.");
					continue;
				}

				child.SetParent(craftingContentRoot, true);
			}

			craftingContentRoot.anchoredPosition = new Vector2(0f, -40f);

			log.Info("Created biome crafting content container and moved crafting content down.");
		}

		private static void MoveRectDown(RectTransform rect, float amount)
		{
			if (rect == null)
				return;

			rect.anchoredPosition -= new Vector2(0f, amount);
		}

		private static void ExtendRectDown(RectTransform rect, float amount)
		{
			if (rect == null)
				return;

			Vector2 originalPosition = rect.anchoredPosition;
			float originalHeight = rect.rect.height;
			float positionAdjustment = amount * (1f - rect.pivot.y);

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalHeight + amount);
			rect.anchoredPosition = originalPosition - new Vector2(0f, positionAdjustment);
		}

		private static void UpdateTabSelectionVisuals()
		{
			foreach (KeyValuePair<CraftingTab, GameObject> entry in tabObjects)
			{
				Button button = entry.Value.GetComponentInChildren<Button>(true);

				if (button != null)
					button.interactable = entry.Key != selectedTab;
			}
		}

		private static void SetBiomeTabsVisible(bool visible)
		{
			foreach (GameObject tabObject in tabObjects.Values)
			{
				if (tabObject != null)
					tabObject.SetActive(visible);
			}
		}

		private static void SetCraftingLayout(bool expanded, InventoryGui inventoryGui)
		{
			if (expanded == craftingLayoutExpanded)
				return;

			const float tabRowHeight = 40f;

			if (craftingContentRoot != null)
				craftingContentRoot.anchoredPosition = expanded ? new Vector2(0f, -tabRowHeight) : Vector2.zero;

			RectTransform craftingRoot = AccessTools.Field(typeof(InventoryGui), "m_crafting")?.GetValue(inventoryGui) as RectTransform;

			if (craftingRoot == null)
				return;

			RectTransform repairButton = GetInventoryGuiObject(inventoryGui, "m_repairButton")?.GetComponent<RectTransform>();
			RectTransform repairPanel = GetInventoryGuiObject(inventoryGui, "m_repairPanel")?.GetComponent<RectTransform>();

			if (expanded)
			{
				ExtendRectDown(craftingRoot.Find("Darken") as RectTransform, tabRowHeight);
				ExtendRectDown(craftingRoot.Find("selected_frame") as RectTransform, tabRowHeight);
				ExtendRectDown(craftingRoot.Find("Bkg") as RectTransform, tabRowHeight);

				MoveRectDown(repairButton, tabRowHeight);
				MoveRectDown(repairPanel, tabRowHeight);
			}
			else
			{
				ExtendRectDown(craftingRoot.Find("Darken") as RectTransform, -tabRowHeight);
				ExtendRectDown(craftingRoot.Find("selected_frame") as RectTransform, -tabRowHeight);
				ExtendRectDown(craftingRoot.Find("Bkg") as RectTransform, -tabRowHeight);

				MoveRectDown(repairButton, -tabRowHeight);
				MoveRectDown(repairPanel, -tabRowHeight);
			}

			craftingLayoutExpanded = expanded;
		}

		private static void EnsureSelectedRecipeVisible(InventoryGui inventoryGui)
		{
			if (inventoryGui == null)
				return;

			var getSelectedRecipeIndex = AccessTools.Method(typeof(InventoryGui), "GetSelectedRecipeIndex", new[] { typeof(bool) });
			var availableRecipesField = AccessTools.Field(typeof(InventoryGui), "m_availableRecipes");
			var ensureVisibleField = AccessTools.Field(typeof(InventoryGui), "m_recipeEnsureVisible");

			if (getSelectedRecipeIndex == null || availableRecipesField == null || ensureVisibleField == null)
				return;

			int selectedIndex = (int)getSelectedRecipeIndex.Invoke(inventoryGui, new object[] { true });

			if (!(availableRecipesField.GetValue(inventoryGui) is System.Collections.IList availableRecipes))
				return;

			if (selectedIndex < 0 || selectedIndex >= availableRecipes.Count)
				return;

			object recipeData = availableRecipes[selectedIndex];
			GameObject interfaceElement = AccessTools.Property(recipeData.GetType(), "InterfaceElement")?.GetValue(recipeData) as GameObject;

			if (interfaceElement == null)
				return;

			ScrollRectEnsureVisible ensureVisible = ensureVisibleField.GetValue(inventoryGui) as ScrollRectEnsureVisible;
			RectTransform recipeRect = interfaceElement.GetComponent<RectTransform>();

			if (ensureVisible != null && recipeRect != null)
				ensureVisible.CenterOnItem(recipeRect);
		}

		private static List<CraftingTab> GetAvailableBiomeTabs()
		{
			List<CraftingTab> tabs = new List<CraftingTab>();

			foreach (CraftingTab tab in tabDisplayOrder)
			{
				if (tab == CraftingTab.All || tab == CraftingTab.Other)
					continue;

				if (availableTabs.Contains(tab))
					tabs.Add(tab);
			}

			return tabs;
		}

		private static void PositionTab(GameObject tabObject, float x, float y, float width)
		{
			if (tabObject == null)
				return;

			RectTransform rect = tabObject.GetComponent<RectTransform>();

			if (rect == null)
				return;

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			rect.anchoredPosition = new Vector2(x, y);
		}

		private static void PositionBiomeTab(GameObject tabObject, float x, float y)
		{
			if (tabObject == null)
				return;

			RectTransform rect = tabObject.GetComponent<RectTransform>();

			if (rect == null)
				return;

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TabWidth);
			rect.anchoredPosition = new Vector2(x, y);
		}
	}
}