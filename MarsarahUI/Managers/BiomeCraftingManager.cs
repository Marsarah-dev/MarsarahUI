using System.Collections.Generic;

namespace MarsarahUI.Managers
{
	internal static class BiomeCraftingManager
	{
		private static readonly LogManager log = new LogManager("Biome Crafting", LogManager.LogLevel.Info);

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
			Trinkets,
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

		private static readonly Dictionary<string, RecipeClassification> recipeClassifications = new Dictionary<string, RecipeClassification>();

		static BiomeCraftingManager()
		{
			// Meadows
			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Tools,
				"Recipe_Hammer",
				"Recipe_Hoe",
				"Recipe_PickaxeAntler");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Torches,
				"Recipe_Torch");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Armor,
				"Recipe_ArmorRagsChest",
				"Recipe_ArmorRagsLegs",
				"Recipe_HelmetLeather",
				"Recipe_ArmorLeatherChest",
				"Recipe_ArmorLeatherLegs");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Capes,
				"Recipe_CapeDeerHide");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.OneHandedWeapons,
				"Recipe_Club",
				"Recipe_AxeStone",
				"Recipe_AxeFlint",
				"Recipe_KnifeFlint",
				"Recipe_SpearFlint");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Shields,
				"Recipe_ShieldWood",
				"Recipe_ShieldWoodTower");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Bows,
				"Recipe_Bow");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Ammo,
				"Recipe_ArrowWood",
				"Recipe_ArrowFlint");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Food,
				"Recipe_FeastMeadows");


			// Black Forest
			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Tools,
				"Recipe_Cultivator");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Armor,
				"Recipe_HelmetTrollLeather",
				"Recipe_ArmorTrollLeatherChest",
				"Recipe_ArmorTrollLeatherLegs",
				"Recipe_HelmetBronze",
				"Recipe_ArmorBronzeChest",
				"Recipe_ArmorBronzeLegs");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Capes,
				"Recipe_CapeTrollHide");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Trinkets,
				"Recipe_TrinketBronzeHealth",
				"Recipe_TrinketBronzeStamina");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.OneHandedWeapons,
				"Recipe_KnifeButcher",
				"Recipe_KnifeCopper",
				"Recipe_SpearBronze",
				"Recipe_SwordBronze",
				"Recipe_MaceBronze",
				"Recipe_AxeBronze");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.TwoHandedWeapons,
				"Recipe_AtgeirBronze",
				"Recipe_SledgeStagbreaker",
				"Recipe_PickaxeBronze");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Shields,
				"Recipe_ShieldBronzeBuckler",
				"Recipe_ShieldBoneTower");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Bows,
				"Recipe_BowFineWood");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Ammo,
				"Recipe_ArrowFire",
				"Recipe_ArrowBronze");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Food,
				"Recipe_BoarJerky",
				"Recipe_CarrotSoup",
				"Recipe_DeerStew",
				"Recipe_MinceMeatSauce",
				"Recipe_QueensJam",
				"Recipe_FeastBlackforest");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Materials,
				"Recipe_Bronze",
				"Recipe_Bronze5",
				"Recipe_BronzeNails");


			// Swamp
			AddRecipes(CraftingBiome.Swamp, RecipeCategory.Armor,
				"Recipe_HelmetRoot",
				"Recipe_ArmorRootChest",
				"Recipe_ArmorRootLegs",
				"Recipe_HelmetIron",
				"Recipe_ArmorIronChest",
				"Recipe_ArmorIronLegs");

			AddRecipes(CraftingBiome.Swamp, RecipeCategory.Trinkets,
				"Recipe_TrinketIronHealth",
				"Recipe_TrinketIronStamina");

			AddRecipes(CraftingBiome.Swamp, RecipeCategory.OneHandedWeapons,
				"Recipe_SpearElderbark",
				"Recipe_SwordIron",
				"Recipe_MaceIron",
				"Recipe_AxeIron");

			AddRecipes(CraftingBiome.Swamp, RecipeCategory.TwoHandedWeapons,
				"Recipe_AtgeirIron",
				"Recipe_Battleaxe",
				"Recipe_SledgeIron",
				"Recipe_PickaxeIron");

			AddRecipes(CraftingBiome.Swamp, RecipeCategory.Shields,
				"Recipe_ShieldIronSquare",
				"Recipe_ShieldIronBuckler",
				"Recipe_ShieldBanded",
				"Recipe_ShieldIronTower");

			AddRecipes(CraftingBiome.Swamp, RecipeCategory.Bows,
				"Recipe_BowHuntsman");

			AddRecipes(CraftingBiome.Swamp, RecipeCategory.Ammo,
				"Recipe_ArrowIron");

			AddRecipes(CraftingBiome.Swamp, RecipeCategory.Food,
				"Recipe_Blacksoup",
				"Recipe_Sausages",
				"Recipe_ShocklateSmoothie",
				"Recipe_TurnipStew",
				"Recipe_FeastSwamps");

			AddRecipes(CraftingBiome.Swamp, RecipeCategory.Materials,
				"Recipe_IronNails");


			// Mountain
			AddRecipes(CraftingBiome.Mountain, RecipeCategory.Armor,
				"Recipe_HelmetFenrir",
				"Recipe_ArmorFenrirChest",
				"Recipe_ArmorFenrirLegs",
				"Recipe_HelmetDrake",
				"Recipe_ArmorWolfChest",
				"Recipe_ArmorWolfLegs");

			AddRecipes(CraftingBiome.Mountain, RecipeCategory.Capes,
				"Recipe_CapeWolf");

			AddRecipes(CraftingBiome.Mountain, RecipeCategory.Trinkets,
				"Recipe_TrinketSilverDamage",
				"Recipe_TrinketSilverResist");

			AddRecipes(CraftingBiome.Mountain, RecipeCategory.OneHandedWeapons,
				"Recipe_KnifeSilver",
				"Recipe_SpearWolfFang",
				"Recipe_SwordSilver",
				"Recipe_MaceSilver");

			AddRecipes(CraftingBiome.Mountain, RecipeCategory.TwoHandedWeapons,
				"Recipe_FistFenrirClaw",
				"Recipe_Battleaxe_Crystal");

			AddRecipes(CraftingBiome.Mountain, RecipeCategory.Shields,
				"Recipe_ShieldSilver");

			AddRecipes(CraftingBiome.Mountain, RecipeCategory.Bows,
				"Recipe_BowDraugrFang");

			AddRecipes(CraftingBiome.Mountain, RecipeCategory.Ammo,
				"Recipe_ArrowObsidian",
				"Recipe_ArrowPoison",
				"Recipe_ArrowFrost",
				"Recipe_ArrowSilver");

			AddRecipes(CraftingBiome.Mountain, RecipeCategory.Food,
				"Recipe_Eyescream",
				"Recipe_Onionsoup",
				"Recipe_WolfJerky",
				"Recipe_WolfSkewer",
				"Recipe_FeastMountains");


			// Plains
			AddRecipes(CraftingBiome.Plains, RecipeCategory.Armor,
				"Recipe_HelmetPadded",
				"Recipe_ArmorPaddedCuirass",
				"Recipe_ArmorPaddedGreaves");

			AddRecipes(CraftingBiome.Plains, RecipeCategory.Capes,
				"Recipe_CapeLox",
				"Recipe_CapeLinen");

			AddRecipes(CraftingBiome.Plains, RecipeCategory.Trinkets,
				"Recipe_TrinketBlackDamageHealth",
				"Recipe_TrinketBlackStamina");

			AddRecipes(CraftingBiome.Plains, RecipeCategory.OneHandedWeapons,
				"Recipe_KnifeBlackmetal",
				"Recipe_SwordBlackmetal",
				"Recipe_MaceNeedle",
				"Recipe_AxeBlackMetal");

			AddRecipes(CraftingBiome.Plains, RecipeCategory.TwoHandedWeapons,
				"Recipe_AtgeirBlackmetal",
				"Recipe_PickaxeBlackMetal");

			AddRecipes(CraftingBiome.Plains, RecipeCategory.Shields,
				"Recipe_ShieldBlackmetal",
				"Recipe_ShieldBlackmetalTower");

			AddRecipes(CraftingBiome.Plains, RecipeCategory.Ammo,
				"Recipe_ArrowNeedle");

			AddRecipes(CraftingBiome.Plains, RecipeCategory.Food,
				"Recipe_BloodPudding",
				"Recipe_FishWraps",
				"Recipe_Bread",
				"Recipe_FishAndBread",
				"Recipe_LoxPie",
				"Recipe_FeastPlains");


			// Ocean
			AddRecipes(CraftingBiome.Ocean, RecipeCategory.Trinkets,
				"Recipe_TrinketChitinSwim");

			AddRecipes(CraftingBiome.Ocean, RecipeCategory.OneHandedWeapons,
				"Recipe_KnifeChitin",
				"Recipe_SpearChitin");

			AddRecipes(CraftingBiome.Ocean, RecipeCategory.Shields,
				"Recipe_ShieldSerpentscale");

			AddRecipes(CraftingBiome.Ocean, RecipeCategory.Food,
				"Recipe_SerpentStew",
				"Recipe_FeastOceans");

			AddRecipes(CraftingBiome.Ocean, RecipeCategory.Ammo,
				"Recipe_FishingBaitOcean");

			log.Info($"Loaded {recipeClassifications.Count} biome crafting recipe classifications.");
		}

		internal static bool TryGetClassification(Recipe recipe, out RecipeClassification classification)
		{
			classification = null;

			if (recipe == null)
				return false;

			return recipeClassifications.TryGetValue(recipe.name, out classification);
		}
		private static void AddRecipes(CraftingBiome biome, RecipeCategory category, params string[] recipeNames)
		{
			for (int i = 0; i < recipeNames.Length; i++)
			{
				string recipeName = recipeNames[i];

				if (recipeClassifications.ContainsKey(recipeName))
				{
					log.Warn($"Duplicate biome crafting classification for recipe '{recipeName}'.");
					continue;
				}

				recipeClassifications.Add(recipeName, new RecipeClassification(biome, category, (i + 1) * 10));
			}
		}
	}
}