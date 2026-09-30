using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;

namespace MarsarahUI.Patches.UI
{
	internal class UIBiomeCraftingTabs
	{
		private static readonly LogManager log = new LogManager("UI Biome Crafting Tabs", LogManager.LogLevel.Info);

		internal enum CraftingTab
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

		private static CraftingTab selectedTab = CraftingTab.All;

		[HarmonyPatch(typeof(InventoryGui), "UpdateRecipeList")]
		private static class UpdateRecipeList_Patch
		{
			private static void Prefix(InventoryGui __instance, List<Recipe> recipes)
			{
				if (!ConfigManager.EffectiveBiomeSortedCraftingTabs) return;
				if (__instance == null || recipes == null) return;
				if (!__instance.InCraftTab()) return;

				int originalCount = recipes.Count;

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

			return selectedTab.ToString() == classification.Biome.ToString();
		}
	}
}