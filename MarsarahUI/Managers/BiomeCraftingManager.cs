using System.Collections.Generic;

namespace MarsarahUI.Managers
{
	internal static class BiomeCraftingManager
	{
		private static readonly LogManager log = new LogManager("Biome Crafting", LogManager.LogLevel.Warning);

		internal enum CraftingBiome
		{
			Meadows,
			BlackForest,
			Swamp,
			Mountain,
			Plains,
			Ocean,
			Mistlands,
			Ashlands,
			DeepNorth
		}

		internal enum RecipeCategory
		{
			Tools,
			Torches,
			Armor,
			Capes,
			OneHandedWeapons,
			TwoHandedWeapons,
			Shields,
			Bows,
			Magic,
			Ammo,
			Food,
			Mead,
			Materials,
			Misc
		}

		internal sealed class RecipeClassification
		{
			public CraftingBiome Biome { get; }
			public RecipeCategory Category { get; }
			public int Order { get; }

			public RecipeClassification(CraftingBiome biome, RecipeCategory category, int order)
			{
				Biome = biome;
				Category = category;
				Order = order;
			}
		}

		private static readonly Dictionary<string, RecipeClassification> recipeClassifications = new Dictionary<string, RecipeClassification>
		{
			// Black Forest
			{ "Recipe_SwordBronze", new RecipeClassification(CraftingBiome.BlackForest, RecipeCategory.OneHandedWeapons, 10) },
			{ "Recipe_MaceBronze", new RecipeClassification(CraftingBiome.BlackForest, RecipeCategory.OneHandedWeapons, 20) },
			{ "Recipe_AxeBronze", new RecipeClassification(CraftingBiome.BlackForest, RecipeCategory.OneHandedWeapons, 30) },

			// Swamp
			{ "Recipe_SwordIron", new RecipeClassification(CraftingBiome.Swamp, RecipeCategory.OneHandedWeapons, 10) },
			{ "Recipe_MaceIron", new RecipeClassification(CraftingBiome.Swamp, RecipeCategory.OneHandedWeapons, 20) },
			{ "Recipe_PickaxeIron", new RecipeClassification(CraftingBiome.Swamp, RecipeCategory.Tools, 10) },

			// Mountain
			{ "Recipe_SwordSilver", new RecipeClassification(CraftingBiome.Mountain, RecipeCategory.OneHandedWeapons, 10) },
			{ "Recipe_MaceSilver", new RecipeClassification(CraftingBiome.Mountain, RecipeCategory.OneHandedWeapons, 20) }
		};

		internal static bool TryGetClassification(Recipe recipe, out RecipeClassification classification)
		{
			classification = null;

			if (recipe == null)
				return false;

			return recipeClassifications.TryGetValue(recipe.name, out classification);
		}
	}
}