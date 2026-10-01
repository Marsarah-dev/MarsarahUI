using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UICraftingTabs
	{
		private static readonly LogManager log = new LogManager("UI Crafting Tabs", LogManager.LogLevel.Warning);

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
			Feasts,
			Recovery,
			Resist,
			Utility,
			Other
		}

		private enum CraftingTabProfile
		{
			Biomes,
			MeadKettle,
			FoodPreparation
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
			CraftingTab.Feasts,
			CraftingTab.Recovery,
			CraftingTab.Resist,
			CraftingTab.Utility,
			CraftingTab.Other
		};

		private sealed class RecipeSortEntry
		{
			internal object RecipeData { get; }
			internal Recipe Recipe { get; }
			internal RectTransform Rect { get; }
			internal int OriginalIndex { get; }

			internal RecipeSortEntry(object recipeData, Recipe recipe, RectTransform rect, int originalIndex)
			{
				RecipeData = recipeData;
				Recipe = recipe;
				Rect = rect;
				OriginalIndex = originalIndex;
			}
		}

		private static readonly HashSet<CraftingTab> availableTabs = new HashSet<CraftingTab>();
		private static readonly Dictionary<CraftingTab, GameObject> tabObjects = new Dictionary<CraftingTab, GameObject>();

		private static CraftingTab selectedTab = CraftingTab.All;
		private static CraftingTabProfile currentTabProfile = CraftingTabProfile.Biomes;
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
					SetCraftingTabsVisible(false);
					SetCraftingLayout(0, __instance);
					return;
				}

				if (!__instance.InCraftTab())
				{
					selectedTab = CraftingTab.All;
					SetCraftingTabsVisible(false);
					SetCraftingLayout(0, __instance);
					return;
				}

				int originalCount = recipes.Count;

				currentTabProfile = GetCraftingTabProfile(recipes);
				UpdateAvailableTabs(recipes);

				int tabRows = GetRequiredLayoutRows();
				SetCraftingLayout(tabRows, __instance);

				if (!availableTabs.Contains(selectedTab))
				{
					log.Info($"Selected crafting tab {selectedTab} is not available at this station. Resetting to All.");
					selectedTab = CraftingTab.All;
				}

				UpdateCraftingTabs(__instance);

				log.Info($"Available crafting tabs: {string.Join(", ", availableTabs)}");

				recipes.RemoveAll(recipe => !RecipeMatchesSelectedTab(recipe));

				log.Info($"{selectedTab} crafting filter: {originalCount} -> {recipes.Count} recipes");
			}

			private static void Postfix(InventoryGui __instance)
			{
				if (__instance == null)
					return;

				if (!ConfigManager.EffectiveBiomeSortedCraftingTabs)
					return;

				if (!__instance.InCraftTab())
					return;

				ApplyRecipeSorting(__instance);
			}
		}

		private static void ApplyRecipeSorting(InventoryGui inventoryGui)
		{
			if (selectedTab == CraftingTab.Other)
				return;

			if (currentTabProfile == CraftingTabProfile.MeadKettle && selectedTab != CraftingTab.All)
				return;

			var availableRecipesField = AccessTools.Field(typeof(InventoryGui), "m_availableRecipes");

			if (availableRecipesField == null)
			{
				log.Error("Could not find InventoryGui.m_availableRecipes for crafting recipe sorting.");
				return;
			}

			if (!(availableRecipesField.GetValue(inventoryGui) is System.Collections.IList availableRecipes))
				return;

			if (availableRecipes.Count < 2)
				return;

			List<RecipeSortEntry> entries = new List<RecipeSortEntry>();
			List<Vector2> slotPositions = new List<Vector2>();

			for (int i = 0; i < availableRecipes.Count; i++)
			{
				object recipeData = availableRecipes[i];
				Recipe recipe = GetRecipeFromRecipeData(recipeData);
				RectTransform rect = GetRecipeRectFromRecipeData(recipeData);

				if (recipe == null || rect == null)
				{
					log.Warn("Could not read recipe data while applying crafting recipe sorting.");
					return;
				}

				entries.Add(new RecipeSortEntry(recipeData, recipe, rect, i));
				slotPositions.Add(rect.anchoredPosition);
			}

			entries.Sort(CompareRecipeEntries);

			for (int i = 0; i < entries.Count; i++)
			{
				availableRecipes[i] = entries[i].RecipeData;
				entries[i].Rect.anchoredPosition = slotPositions[i];
			}

			log.Info($"Applied crafting recipe order for {currentTabProfile} / {selectedTab}: {entries.Count} recipes.");
		}

		private static Recipe GetRecipeFromRecipeData(object recipeData)
		{
			if (recipeData == null)
				return null;

			var property = AccessTools.Property(recipeData.GetType(), "Recipe");

			if (property != null)
				return property.GetValue(recipeData) as Recipe;

			var field = AccessTools.Field(recipeData.GetType(), "Recipe");
			return field?.GetValue(recipeData) as Recipe;
		}

		private static RectTransform GetRecipeRectFromRecipeData(object recipeData)
		{
			if (recipeData == null)
				return null;

			GameObject interfaceElement = null;

			var property = AccessTools.Property(recipeData.GetType(), "InterfaceElement");

			if (property != null)
				interfaceElement = property.GetValue(recipeData) as GameObject;
			else
				interfaceElement = AccessTools.Field(recipeData.GetType(), "InterfaceElement")?.GetValue(recipeData) as GameObject;

			return interfaceElement?.GetComponent<RectTransform>();
		}

		private static int CompareRecipeEntries(RecipeSortEntry a, RecipeSortEntry b)
		{
			switch (currentTabProfile)
			{
				case CraftingTabProfile.MeadKettle:
					return CompareMeadRecipes(a, b);

				case CraftingTabProfile.FoodPreparation:
					return CompareFoodPreparationRecipes(a, b);

				default:
					return CompareBiomeRecipes(a, b);
			}
		}

		private static int CompareBiomeRecipes(RecipeSortEntry a, RecipeSortEntry b)
		{
			bool hasA = BiomeCraftingManager.TryGetClassification(a.Recipe, out BiomeCraftingManager.RecipeClassification classificationA);
			bool hasB = BiomeCraftingManager.TryGetClassification(b.Recipe, out BiomeCraftingManager.RecipeClassification classificationB);

			if (selectedTab == CraftingTab.All)
			{
				if (hasA != hasB)
					return hasA ? -1 : 1;

				if (!hasA)
					return a.OriginalIndex.CompareTo(b.OriginalIndex);

				int comparison = ((int)classificationA.Biome).CompareTo((int)classificationB.Biome);

				if (comparison != 0)
					return comparison;
			}

			if (!hasA || !hasB)
				return a.OriginalIndex.CompareTo(b.OriginalIndex);

			int categoryComparison = ((int)classificationA.Category).CompareTo((int)classificationB.Category);

			if (categoryComparison != 0)
				return categoryComparison;

			int orderComparison = classificationA.Order.CompareTo(classificationB.Order);

			if (orderComparison != 0)
				return orderComparison;

			return a.OriginalIndex.CompareTo(b.OriginalIndex);
		}

		private static int CompareFoodPreparationRecipes(RecipeSortEntry a, RecipeSortEntry b)
		{
			if (selectedTab == CraftingTab.Feasts)
				return CompareFeastRecipes(a, b);

			if (selectedTab != CraftingTab.All)
				return CompareBiomeRecipes(a, b);

			int groupA = GetFoodPreparationSortGroup(a.Recipe);
			int groupB = GetFoodPreparationSortGroup(b.Recipe);

			int groupComparison = groupA.CompareTo(groupB);

			if (groupComparison != 0)
				return groupComparison;

			if (groupA == 4)
				return CompareFeastRecipes(a, b);

			if (groupA == 5)
				return a.OriginalIndex.CompareTo(b.OriginalIndex);

			if (BiomeCraftingManager.TryGetClassification(a.Recipe, out BiomeCraftingManager.RecipeClassification classificationA) &&
				BiomeCraftingManager.TryGetClassification(b.Recipe, out BiomeCraftingManager.RecipeClassification classificationB))
			{
				int categoryComparison = ((int)classificationA.Category).CompareTo((int)classificationB.Category);

				if (categoryComparison != 0)
					return categoryComparison;

				int orderComparison = classificationA.Order.CompareTo(classificationB.Order);

				if (orderComparison != 0)
					return orderComparison;
			}

			return a.OriginalIndex.CompareTo(b.OriginalIndex);
		}

		private static int GetFoodPreparationSortGroup(Recipe recipe)
		{
			if (BiomeCraftingManager.TryGetSpecialGroup(recipe, out BiomeCraftingManager.SpecialRecipeGroup group) &&
				group == BiomeCraftingManager.SpecialRecipeGroup.Feast)
			{
				return 4;
			}

			if (!BiomeCraftingManager.TryGetClassification(recipe, out BiomeCraftingManager.RecipeClassification classification))
				return 5;

			switch (classification.Biome)
			{
				case BiomeCraftingManager.CraftingBiome.Plains:
					return 0;

				case BiomeCraftingManager.CraftingBiome.Mistlands:
					return 1;

				case BiomeCraftingManager.CraftingBiome.Ashlands:
					return 2;

				case BiomeCraftingManager.CraftingBiome.DeepNorth:
					return 3;

				default:
					return 5;
			}
		}

		private static int CompareFeastRecipes(RecipeSortEntry a, RecipeSortEntry b)
		{
			bool hasA = BiomeCraftingManager.TryGetClassification(a.Recipe, out BiomeCraftingManager.RecipeClassification classificationA);
			bool hasB = BiomeCraftingManager.TryGetClassification(b.Recipe, out BiomeCraftingManager.RecipeClassification classificationB);

			if (hasA != hasB)
				return hasA ? -1 : 1;

			if (!hasA)
				return a.OriginalIndex.CompareTo(b.OriginalIndex);

			int biomeComparison = ((int)classificationA.Biome).CompareTo((int)classificationB.Biome);

			if (biomeComparison != 0)
				return biomeComparison;

			return a.OriginalIndex.CompareTo(b.OriginalIndex);
		}

		private static int CompareMeadRecipes(RecipeSortEntry a, RecipeSortEntry b)
		{
			if (selectedTab != CraftingTab.All)
				return a.OriginalIndex.CompareTo(b.OriginalIndex);

			int groupA = GetMeadSortGroup(a.Recipe);
			int groupB = GetMeadSortGroup(b.Recipe);

			int comparison = groupA.CompareTo(groupB);

			if (comparison != 0)
				return comparison;

			return a.OriginalIndex.CompareTo(b.OriginalIndex);
		}

		private static int GetMeadSortGroup(Recipe recipe)
		{
			if (!BiomeCraftingManager.TryGetSpecialGroup(recipe, out BiomeCraftingManager.SpecialRecipeGroup group))
				return 3;

			switch (group)
			{
				case BiomeCraftingManager.SpecialRecipeGroup.MeadRecovery:
					return 0;

				case BiomeCraftingManager.SpecialRecipeGroup.MeadResistance:
					return 1;

				case BiomeCraftingManager.SpecialRecipeGroup.MeadUtility:
					return 2;

				default:
					return 3;
			}
		}

		private static bool RecipeMatchesSelectedTab(Recipe recipe)
		{
			if (selectedTab == CraftingTab.All)
				return true;

			switch (currentTabProfile)
			{
				case CraftingTabProfile.MeadKettle:
					return RecipeMatchesMeadKettleTab(recipe);

				case CraftingTabProfile.FoodPreparation:
					return RecipeMatchesFoodPreparationTab(recipe);

				default:
					return RecipeMatchesBiomeTab(recipe);
			}
		}

		private static bool RecipeMatchesBiomeTab(Recipe recipe)
		{
			if (selectedTab == CraftingTab.Other)
				return !BiomeCraftingManager.TryGetClassification(recipe, out _);

			if (!BiomeCraftingManager.TryGetClassification(recipe, out BiomeCraftingManager.RecipeClassification classification))
				return false;

			if (!TryGetBiomeForTab(selectedTab, out BiomeCraftingManager.CraftingBiome biome))
				return false;

			return classification.Biome == biome;
		}

		private static bool RecipeMatchesMeadKettleTab(Recipe recipe)
		{
			if (!BiomeCraftingManager.TryGetSpecialGroup(recipe, out BiomeCraftingManager.SpecialRecipeGroup group))
				return selectedTab == CraftingTab.Other;

			switch (selectedTab)
			{
				case CraftingTab.Recovery:
					return group == BiomeCraftingManager.SpecialRecipeGroup.MeadRecovery;

				case CraftingTab.Resist:
					return group == BiomeCraftingManager.SpecialRecipeGroup.MeadResistance;

				case CraftingTab.Utility:
					return group == BiomeCraftingManager.SpecialRecipeGroup.MeadUtility;

				case CraftingTab.Other:
					return group != BiomeCraftingManager.SpecialRecipeGroup.MeadRecovery &&
						group != BiomeCraftingManager.SpecialRecipeGroup.MeadResistance &&
						group != BiomeCraftingManager.SpecialRecipeGroup.MeadUtility;

				default:
					return false;
			}
		}

		private static bool RecipeMatchesFoodPreparationTab(Recipe recipe)
		{
			if (BiomeCraftingManager.TryGetSpecialGroup(recipe, out BiomeCraftingManager.SpecialRecipeGroup group) &&
				group == BiomeCraftingManager.SpecialRecipeGroup.Feast)
			{
				return selectedTab == CraftingTab.Feasts;
			}

			if (BiomeCraftingManager.TryGetClassification(recipe, out BiomeCraftingManager.RecipeClassification classification) &&
				TryGetTabForBiome(classification.Biome, out CraftingTab tab) &&
				IsFoodPreparationBiomeTab(tab))
			{
				return selectedTab == tab;
			}

			return selectedTab == CraftingTab.Other;
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

		private static bool TryGetTabForBiome(BiomeCraftingManager.CraftingBiome biome, out CraftingTab tab)
		{
			switch (biome)
			{
				case BiomeCraftingManager.CraftingBiome.Meadows:
					tab = CraftingTab.Meadows;
					return true;

				case BiomeCraftingManager.CraftingBiome.BlackForest:
					tab = CraftingTab.BlackForest;
					return true;

				case BiomeCraftingManager.CraftingBiome.Swamp:
					tab = CraftingTab.Swamp;
					return true;

				case BiomeCraftingManager.CraftingBiome.Mountain:
					tab = CraftingTab.Mountain;
					return true;

				case BiomeCraftingManager.CraftingBiome.Plains:
					tab = CraftingTab.Plains;
					return true;

				case BiomeCraftingManager.CraftingBiome.Ocean:
					tab = CraftingTab.Ocean;
					return true;

				case BiomeCraftingManager.CraftingBiome.Mistlands:
					tab = CraftingTab.Mistlands;
					return true;

				case BiomeCraftingManager.CraftingBiome.Ashlands:
					tab = CraftingTab.Ashlands;
					return true;

				case BiomeCraftingManager.CraftingBiome.DeepNorth:
					tab = CraftingTab.DeepNorth;
					return true;

				default:
					tab = default;
					return false;
			}
		}

		private static bool IsFoodPreparationBiomeTab(CraftingTab tab)
		{
			return tab == CraftingTab.Plains ||
				tab == CraftingTab.Mistlands ||
				tab == CraftingTab.Ashlands ||
				tab == CraftingTab.DeepNorth;
		}

		private static void UpdateAvailableTabs(List<Recipe> recipes)
		{
			availableTabs.Clear();
			availableTabs.Add(CraftingTab.All);

			switch (currentTabProfile)
			{
				case CraftingTabProfile.MeadKettle:
					UpdateMeadKettleTabs(recipes);
					break;

				case CraftingTabProfile.FoodPreparation:
					UpdateFoodPreparationTabs(recipes);
					break;

				default:
					UpdateBiomeAvailableTabs(recipes);
					break;
			}
		}

		private static void UpdateBiomeAvailableTabs(List<Recipe> recipes)
		{
			bool hasOther = false;

			foreach (Recipe recipe in recipes)
			{
				if (BiomeCraftingManager.TryGetClassification(recipe, out BiomeCraftingManager.RecipeClassification classification) &&
					TryGetTabForBiome(classification.Biome, out CraftingTab tab))
				{
					availableTabs.Add(tab);
				}
				else
				{
					hasOther = true;
				}
			}

			if (hasOther)
				availableTabs.Add(CraftingTab.Other);
		}

		private static void UpdateMeadKettleTabs(List<Recipe> recipes)
		{
			bool hasOther = false;

			foreach (Recipe recipe in recipes)
			{
				if (!BiomeCraftingManager.TryGetSpecialGroup(recipe, out BiomeCraftingManager.SpecialRecipeGroup group))
				{
					hasOther = true;
					continue;
				}

				switch (group)
				{
					case BiomeCraftingManager.SpecialRecipeGroup.MeadRecovery:
						availableTabs.Add(CraftingTab.Recovery);
						break;

					case BiomeCraftingManager.SpecialRecipeGroup.MeadResistance:
						availableTabs.Add(CraftingTab.Resist);
						break;

					case BiomeCraftingManager.SpecialRecipeGroup.MeadUtility:
						availableTabs.Add(CraftingTab.Utility);
						break;

					default:
						hasOther = true;
						break;
				}
			}

			if (hasOther)
				availableTabs.Add(CraftingTab.Other);
		}

		private static void UpdateFoodPreparationTabs(List<Recipe> recipes)
		{
			bool hasOther = false;

			foreach (Recipe recipe in recipes)
			{
				if (BiomeCraftingManager.TryGetSpecialGroup(recipe, out BiomeCraftingManager.SpecialRecipeGroup group) &&
					group == BiomeCraftingManager.SpecialRecipeGroup.Feast)
				{
					availableTabs.Add(CraftingTab.Feasts);
					continue;
				}

				if (BiomeCraftingManager.TryGetClassification(recipe, out BiomeCraftingManager.RecipeClassification classification) &&
					TryGetTabForBiome(classification.Biome, out CraftingTab tab) &&
					IsFoodPreparationBiomeTab(tab))
				{
					availableTabs.Add(tab);
					continue;
				}

				hasOther = true;
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
				case CraftingTab.Feasts:
					return "FEASTS";
				case CraftingTab.Recovery:
					return "RECOVERY";
				case CraftingTab.Resist:
					return "RESIST";
				case CraftingTab.Utility:
					return "UTILITY";
				default:
					return tab.ToString().ToUpperInvariant();
			}
		}

		private static void CreateCraftingTabs(InventoryGui inventoryGui)
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

		internal static void RefreshFromConfig()
		{
			if (currentInventoryGui == null) return;

			RefreshCraftingPanel(currentInventoryGui);

			log.Info($"Refreshed crafting tabs using effective mode {ConfigManager.EffectiveBiomeCraftingTabsChoice}.");
		}

		private static void UpdateCraftingTabs(InventoryGui inventoryGui)
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
					SetCraftingTabsVisible(false);
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
			CreateCraftingTabs(inventoryGui);
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
					biomeScrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
				}
			}

			biomeScrollViewportObject.transform.SetAsLastSibling();

			if (biomeScrollBar != null)
				biomeScrollBar.transform.SetAsLastSibling();

			log.Info("Created horizontal biome crafting tab scroll area.");
		}

		private static void UpdateScrollingTabs(InventoryGui inventoryGui)
		{
			CreateCraftingTabs(inventoryGui);

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

			bool needsScrolling = NeedsHorizontalTabScrolling();

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

		private static void SetCraftingTabsVisible(bool visible)
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

			if (tabRows > 0 && ConfigManager.EffectiveBiomeCraftingTabsChoice == ConfigManager.BiomeCraftingTabsMode.Scrolling && NeedsHorizontalTabScrolling())
			{
				targetExtraHeight += ScrollBarHeight + ScrollBarSpacing;
			}

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

		private static CraftingTabProfile GetCraftingTabProfile(List<Recipe> recipes)
		{
			foreach (Recipe recipe in recipes)
			{
				if (recipe?.m_craftingStation == null)
					continue;

				switch (recipe.m_craftingStation.name)
				{
					case "piece_MeadCauldron":
						return CraftingTabProfile.MeadKettle;

					case "piece_preptable":
						return CraftingTabProfile.FoodPreparation;
				}
			}

			return CraftingTabProfile.Biomes;
		}

		private static bool NeedsHorizontalTabScrolling()
		{
			int visibleScrollableTabs = 0;

			foreach (CraftingTab tab in tabDisplayOrder)
			{
				if (tab == CraftingTab.All || !availableTabs.Contains(tab))
					continue;

				visibleScrollableTabs++;
			}

			if (visibleScrollableTabs == 0)
				return false;

			float contentWidth = (visibleScrollableTabs * TabWidth) + ((visibleScrollableTabs - 1) * TabSpacing);
			float scrollWidth = TabRowWidth - TabWidth - TabSpacing - ScrollRightInset;

			return contentWidth > scrollWidth;
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