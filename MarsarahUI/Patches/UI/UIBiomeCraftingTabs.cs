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
		private static float currentCraftingExtraHeight;
		private static GameObject biomeScrollViewportObject;
		private static RectTransform biomeScrollViewport;
		private static RectTransform biomeScrollContent;
		private static ScrollRect biomeScrollRect;
		private static Scrollbar biomeScrollBar;

		private const float TabWidth = 90f;
		private const float TabSpacing = 2f;
		private const float TabRowHeight = 32f;
		private const float TabRowWidth = 560f;
		private const float TabRowLeftOffset = 32f;
		private const float ScrollBarHeight = 10f;
		private const float ScrollBarSpacing = 4f;
		private const float ScrollRightInset = 10f;

		private static bool recipeDumpLogged;

		[HarmonyPatch(typeof(InventoryGui), "UpdateRecipeList")]
		private static class UpdateRecipeList_Patch
		{
			private static void Prefix(InventoryGui __instance, List<Recipe> recipes)
			{
				if (__instance == null || recipes == null) return;

				//DumpCurrentRecipes();

				if (!ConfigManager.EffectiveBiomeSortedCraftingTabs)
				{
					selectedTab = CraftingTab.All;
					SetBiomeTabsVisible(false);
					SetCraftingLayout(0, __instance);
					return;
				}

				if (!__instance.InCraftTab())
				{
					selectedTab = CraftingTab.All;
					SetBiomeTabsVisible(false);
					SetCraftingLayout(0, __instance);
					return;
				}

				int originalCount = recipes.Count;

				UpdateAvailableTabs(recipes);

				int tabRows = GetRequiredLayoutRows();
				SetCraftingLayout(tabRows, __instance);

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
			switch (ConfigManager.EffectiveBiomeCraftingTabsChoice)
			{
				case ConfigManager.BiomeCraftingTabsMode.MultipleRows:
					UpdateMultipleRowTabs(inventoryGui);
					break;

				case ConfigManager.BiomeCraftingTabsMode.Scrolling:
					UpdateScrollingTabs(inventoryGui);
					break;

				case ConfigManager.BiomeCraftingTabsMode.Off:
					SetBiomeTabsVisible(false);
					break;
			}
		}

		private static int GetRequiredLayoutRows()
		{
			switch (ConfigManager.EffectiveBiomeCraftingTabsChoice)
			{
				case ConfigManager.BiomeCraftingTabsMode.MultipleRows:
					return GetRequiredTabRows();

				case ConfigManager.BiomeCraftingTabsMode.Scrolling:
					return 1;

				default:
					return 0;
			}
		}

		private static void UpdateMultipleRowTabs(InventoryGui inventoryGui)
		{
			CreateBiomeTabs(inventoryGui);
			GameObject craftTab = GetInventoryGuiObject(inventoryGui, "m_tabCraft");

			if (craftTab == null)
				return;

			RectTransform craftRect = craftTab.GetComponent<RectTransform>();

			if (craftRect == null)
				return;

			if (biomeScrollViewportObject != null)
				biomeScrollViewportObject.SetActive(false);

			if (biomeScrollBar != null)
				biomeScrollBar.gameObject.SetActive(false);

			foreach (GameObject tabObject in tabObjects.Values)
			{
				tabObject.SetActive(false);
			}

			float startX = craftRect.anchoredPosition.x - TabRowLeftOffset;
			float currentX = startX;
			float currentY = craftRect.anchoredPosition.y - craftRect.rect.height - 4f;

			foreach (CraftingTab tab in tabDisplayOrder)
			{
				if (!availableTabs.Contains(tab))
					continue;

				GameObject tabObject = tabObjects[tab];

				RectTransform rect = tabObject.GetComponent<RectTransform>();

				tabObject.transform.SetParent(craftTab.transform.parent, false);

				rect.anchorMin = craftRect.anchorMin;
				rect.anchorMax = craftRect.anchorMax;
				rect.pivot = craftRect.pivot;
				rect.localScale = Vector3.one;
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, craftRect.rect.height);

				tabObject.SetActive(true);

				if (currentX + TabWidth > startX + TabRowWidth && currentX > startX)
				{
					currentX = startX;
					currentY -= TabRowHeight;
				}

				PositionBiomeTab(tabObject, currentX, currentY);

				currentX += TabWidth + TabSpacing;
			}

			UpdateTabSelectionVisuals();
		}

		private static void CreateBiomeScrollArea(InventoryGui inventoryGui, RectTransform craftRect)
		{
			if (biomeScrollViewportObject != null)
				return;

			GameObject craftTab = GetInventoryGuiObject(inventoryGui, "m_tabCraft");
			Scrollbar recipeScrollBar = AccessTools.Field(typeof(InventoryGui), "m_recipeListScroll")?.GetValue(inventoryGui) as Scrollbar;

			if (craftTab == null)
			{
				log.Error("Could not create biome tab scroll area because the Craft tab was not found.");
				return;
			}

			Transform parent = craftTab.transform.parent;

			biomeScrollViewportObject = new GameObject("MarsarahBiomeTabScrollViewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
			biomeScrollViewport = biomeScrollViewportObject.GetComponent<RectTransform>();
			biomeScrollViewport.SetParent(parent, false);

			biomeScrollViewport.anchorMin = craftRect.anchorMin;
			biomeScrollViewport.anchorMax = craftRect.anchorMax;
			biomeScrollViewport.pivot = new Vector2(0f, craftRect.pivot.y);
			biomeScrollViewport.localScale = Vector3.one;

			Image viewportImage = biomeScrollViewportObject.GetComponent<Image>();
			viewportImage.color = new Color(0f, 0f, 0f, 0f);
			viewportImage.raycastTarget = true;

			GameObject contentObject = new GameObject("Content", typeof(RectTransform));
			biomeScrollContent = contentObject.GetComponent<RectTransform>();
			biomeScrollContent.SetParent(biomeScrollViewport, false);
			biomeScrollContent.anchorMin = new Vector2(0f, 0.5f);
			biomeScrollContent.anchorMax = new Vector2(0f, 0.5f);
			biomeScrollContent.pivot = new Vector2(0f, 0.5f);
			biomeScrollContent.anchoredPosition = Vector2.zero;

			biomeScrollRect = biomeScrollViewportObject.GetComponent<ScrollRect>();
			biomeScrollRect.content = biomeScrollContent;
			biomeScrollRect.viewport = biomeScrollViewport;
			biomeScrollRect.horizontal = true;
			biomeScrollRect.vertical = false;
			biomeScrollRect.movementType = ScrollRect.MovementType.Clamped;
			biomeScrollRect.inertia = true;
			biomeScrollRect.scrollSensitivity = -(TabWidth + TabSpacing) * 4f;

			if (recipeScrollBar != null)
			{
				GameObject scrollBarObject = Object.Instantiate(recipeScrollBar.gameObject, parent);
				scrollBarObject.name = "MarsarahBiomeTabScrollBar";

				biomeScrollBar = scrollBarObject.GetComponent<Scrollbar>();

				if (biomeScrollBar != null)
				{
					biomeScrollBar.onValueChanged = new Scrollbar.ScrollEvent();
					biomeScrollBar.SetDirection(Scrollbar.Direction.LeftToRight, true);

					Navigation navigation = biomeScrollBar.navigation;
					navigation.mode = Navigation.Mode.None;
					biomeScrollBar.navigation = navigation;

					biomeScrollRect.horizontalScrollbar = biomeScrollBar;
					biomeScrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
				}
			}

			biomeScrollViewportObject.transform.SetAsLastSibling();

			if (biomeScrollBar != null)
				biomeScrollBar.transform.SetAsLastSibling();

			log.Info("Created horizontal biome crafting tab scroll area.");
		}

		private static void UpdateScrollingTabs(InventoryGui inventoryGui)
		{
			CreateBiomeTabs(inventoryGui);

			GameObject craftTab = GetInventoryGuiObject(inventoryGui, "m_tabCraft");

			if (craftTab == null)
				return;

			RectTransform craftRect = craftTab.GetComponent<RectTransform>();

			if (craftRect == null)
				return;

			CreateBiomeScrollArea(inventoryGui, craftRect);

			if (biomeScrollViewport == null || biomeScrollContent == null)
				return;

			float startX = craftRect.anchoredPosition.x - TabRowLeftOffset;
			float rowY = craftRect.anchoredPosition.y - craftRect.rect.height - 4f;
			float scrollStartX = startX + (TabWidth * 0.5f) + TabSpacing;
			float scrollWidth = TabRowWidth - TabWidth - TabSpacing - ScrollRightInset;

			GameObject allTab = tabObjects[CraftingTab.All];
			RectTransform allRect = allTab.GetComponent<RectTransform>();

			allTab.transform.SetParent(craftTab.transform.parent, false);

			allRect.anchorMin = craftRect.anchorMin;
			allRect.anchorMax = craftRect.anchorMax;
			allRect.pivot = craftRect.pivot;
			allRect.localScale = Vector3.one;

			PositionBiomeTab(allTab, startX, rowY);
			allTab.SetActive(true);

			biomeScrollViewportObject.SetActive(true);
			biomeScrollViewport.anchoredPosition = new Vector2(scrollStartX, rowY);
			biomeScrollViewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollWidth);
			biomeScrollViewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, craftRect.rect.height);

			float previousScrollPosition = biomeScrollRect.horizontalNormalizedPosition;
			float contentX = 0f;
			int visibleScrollableTabs = 0;

			foreach (CraftingTab tab in tabDisplayOrder)
			{
				if (tab == CraftingTab.All)
					continue;

				GameObject tabObject = tabObjects[tab];

				if (!availableTabs.Contains(tab))
				{
					tabObject.SetActive(false);
					continue;
				}

				RectTransform rect = tabObject.GetComponent<RectTransform>();

				tabObject.transform.SetParent(biomeScrollContent, false);

				rect.anchorMin = new Vector2(0f, 0.5f);
				rect.anchorMax = new Vector2(0f, 0.5f);
				rect.pivot = new Vector2(0f, 0.5f);
				rect.localScale = Vector3.one;

				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TabWidth);
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, craftRect.rect.height);
				rect.anchoredPosition = new Vector2(contentX, 0f);

				tabObject.SetActive(true);

				contentX += TabWidth + TabSpacing;
				visibleScrollableTabs++;
			}

			float contentWidth = visibleScrollableTabs > 0 ? contentX - TabSpacing : 0f;

			biomeScrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
			biomeScrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, craftRect.rect.height);

			bool needsScrolling = contentWidth > scrollWidth;

			if (biomeScrollBar != null)
			{
				RectTransform scrollBarRect = biomeScrollBar.GetComponent<RectTransform>();

				scrollBarRect.anchorMin = craftRect.anchorMin;
				scrollBarRect.anchorMax = craftRect.anchorMax;
				scrollBarRect.pivot = new Vector2(0f, 0.5f);

				float scrollBarY = rowY - (craftRect.rect.height * 0.5f) - (ScrollBarHeight * 0.5f) - 2f;

				scrollBarRect.anchoredPosition = new Vector2(scrollStartX, scrollBarY);
				scrollBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollWidth);
				scrollBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ScrollBarHeight);

				biomeScrollBar.gameObject.SetActive(needsScrolling);
			}

			Canvas.ForceUpdateCanvases();

			if (needsScrolling)
				biomeScrollRect.horizontalNormalizedPosition = previousScrollPosition;
			else
				biomeScrollRect.horizontalNormalizedPosition = 0f;

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

			craftingContentRoot.anchoredPosition = Vector2.zero;
			currentCraftingExtraHeight = 0f;

			log.Info("Created biome crafting content container.");
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

			if (!visible)
			{
				if (biomeScrollViewportObject != null)
					biomeScrollViewportObject.SetActive(false);

				if (biomeScrollBar != null)
					biomeScrollBar.gameObject.SetActive(false);
			}
		}

		private static void SetCraftingLayout(int tabRows, InventoryGui inventoryGui)
		{
			CreateCraftingContentRoot(inventoryGui);

			float targetExtraHeight = tabRows > 0 ? 40f + ((tabRows - 1) * TabRowHeight) : 0f;

			if (tabRows > 0 && ConfigManager.EffectiveBiomeCraftingTabsChoice == ConfigManager.BiomeCraftingTabsMode.Scrolling)
				targetExtraHeight += ScrollBarHeight + ScrollBarSpacing;

			if (Mathf.Approximately(targetExtraHeight, currentCraftingExtraHeight))
				return;

			RectTransform craftingRoot = AccessTools.Field(typeof(InventoryGui), "m_crafting")?.GetValue(inventoryGui) as RectTransform;

			if (craftingRoot == null)
				return;

			float difference = targetExtraHeight - currentCraftingExtraHeight;

			if (craftingContentRoot != null)
				craftingContentRoot.anchoredPosition = new Vector2(0f, -targetExtraHeight);

			ExtendRectDown(craftingRoot.Find("Darken") as RectTransform, difference);
			ExtendRectDown(craftingRoot.Find("selected_frame") as RectTransform, difference);
			ExtendRectDown(craftingRoot.Find("Bkg") as RectTransform, difference);

			RectTransform repairButton = GetInventoryGuiObject(inventoryGui, "m_repairButton")?.GetComponent<RectTransform>();
			RectTransform repairPanel = GetInventoryGuiObject(inventoryGui, "m_repairPanel")?.GetComponent<RectTransform>();

			MoveRectDown(repairButton, difference);
			MoveRectDown(repairPanel, difference);

			currentCraftingExtraHeight = targetExtraHeight;

			log.Info($"Crafting layout adjusted for {tabRows} biome tab row(s), extra height {targetExtraHeight}px.");
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

		private static int GetRequiredTabRows()
		{
			float currentWidth = 0f;
			int rows = 1;

			foreach (CraftingTab tab in tabDisplayOrder)
			{
				if (!availableTabs.Contains(tab))
					continue;

				float requiredWidth = currentWidth == 0f ? TabWidth : TabSpacing + TabWidth;

				if (currentWidth > 0f && currentWidth + requiredWidth > TabRowWidth)
				{
					rows++;
					currentWidth = TabWidth;
				}
				else
				{
					currentWidth += requiredWidth;
				}
			}

			return rows;
		}

		private static void DumpCurrentRecipes()
		{
			if (recipeDumpLogged || ObjectDB.instance == null || ObjectDB.instance.m_recipes == null)
				return;

			recipeDumpLogged = true;

			log.Info($"===== CURRENT RECIPE DUMP: {ObjectDB.instance.m_recipes.Count} recipes =====");

			foreach (Recipe recipe in ObjectDB.instance.m_recipes)
			{
				if (recipe == null)
					continue;

				string itemName = recipe.m_item != null ? recipe.m_item.name : "<none>";
				string stationName = recipe.m_craftingStation != null ? recipe.m_craftingStation.name : "<none>";

				ItemDrop itemDrop = recipe.m_item != null ? recipe.m_item.GetComponent<ItemDrop>() : null;
				string itemType = itemDrop != null ? itemDrop.m_itemData.m_shared.m_itemType.ToString() : "<none>";

				log.Info($"{recipe.name} | Item: {itemName} | Type: {itemType} | Station: {stationName}");
			}

			log.Info("===== END CURRENT RECIPE DUMP =====");
		}
	}
}