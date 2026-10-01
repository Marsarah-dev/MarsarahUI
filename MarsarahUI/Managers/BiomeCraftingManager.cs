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

			// Mistlands
			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Tools, 
				"Recipe_Demister", 
				"Recipe_GrapplingHook");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Torches, 
				"Recipe_TorchMist");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Armor, 
				"Recipe_HelmetMage", 
				"Recipe_ArmorMageChest", 
				"Recipe_ArmorMageLegs", 
				"Recipe_HelmetCarapace", 
				"Recipe_ArmorCarapaceChest", 
				"Recipe_ArmorCarapaceLegs");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Capes, 
				"Recipe_CapeFeather");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Trinkets, 
				"Recipe_TrinketCarapaceEitr", 
				"Recipe_TrinketScaleStaminaDamage");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.OneHandedWeapons, 
				"Recipe_SpearCarapace", 
				"Recipe_SwordMistwalker", 
				"Recipe_AxeJotunBane");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.TwoHandedWeapons, 
				"Recipe_KnifeSkollAndHati", 
				"Recipe_AtgeirHimminAfl", 
				"Recipe_SwordKrom", 
				"Recipe_SledgeDemolisher");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Shields, 
				"Recipe_ShieldCarapaceBuckler", 
				"Recipe_ShieldCarapace");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Bows, 
				"Recipe_CrossbowArbalest", 
				"Recipe_BowSpineSnap");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Magic, 
				"Recipe_StaffFireball", 
				"Recipe_StaffIceShards", 
				"Recipe_StaffShield", 
				"Recipe_StaffSkeleton");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Ammo, 
				"Recipe_ArrowCarapace", 
				"Recipe_BoltBone", 
				"Recipe_BoltIron", 
				"Recipe_BoltBlackmetal", 
				"Recipe_BoltCarapace", 
				"Recipe_TurretBoltWood", 
				"Recipe_TurretBolt", 
				"Recipe_FishingBaitMistlands");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Food, 
				"Recipe_CookedEgg", 
				"Recipe_MushroomOmelette", 
				"Recipe_Salad", 
				"Recipe_SeekerAspic", 
				"Recipe_YggdrasilPorridge", 
				"Recipe_HoneyGlazedChicken", 
				"Recipe_MagicallyStuffedShroom", 
				"Recipe_MeatPlatter", 
				"Recipe_MisthareSupreme", 
				"Recipe_FeastMistlands");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Materials, 
				"Recipe_CeramicPlate", 
				"Recipe_MechanicalSpring", 
				"Recipe_ShieldCore");

			AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Misc, 
				"Recipe_DvergrKey");


			// Ashlands
			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.Armor, 
				"Recipe_HelmetMage_Ashlands", 
				"Recipe_ArmorMageChest_Ashlands", 
				"Recipe_ArmorMageLegs_Ashlands", 
				"Recipe_HelmetMedium_Ashlands", 
				"Recipe_ArmorMediumChest_Ashlands", 
				"Recipe_ArmorMediumLegs_Ashlands", 
				"Recipe_HelmetFlametal", 
				"Recipe_ArmorFlametalChest", 
				"Recipe_ArmorFlametalLegs");

			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.Capes, 
				"Recipe_CapeAsh", 
				"Recipe_CapeAsksvin");

			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.Trinkets, 
				"Recipe_TrinketFlametalEitr", 
				"Recipe_TrinketFlametalStaminaHealth");

			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.OneHandedWeapons, 
				"Recipe_SpearSplitner", 
				"Recipe_SpearSplitner_Blood", 
				"Recipe_SpearSplitner_Lightning", 
				"Recipe_SpearSplitner_Nature", 
				"Recipe_SwordFire", 
				"Recipe_SwordNiedhogg", 
				"Recipe_SwordNiedhogg_Blood", 
				"Recipe_SwordNiedhogg_Lightning", 
				"Recipe_SwordNiedhogg_Nature", 
				"Recipe_MaceEldner", 
				"Recipe_MaceEldner_Blood", 
				"Recipe_MaceEldner_Lightning", 
				"Recipe_MaceEldner_Nature");

			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.TwoHandedWeapons, 
				"Recipe_AxeBerzerkr", 
				"Recipe_AxeBerzerkr_Blood", 
				"Recipe_AxeBerzerkr_Lightning", 
				"Recipe_AxeBerzerkr_Nature", 
				"Recipe_SwordSlayer", 
				"Recipe_SwordSlayer_Blood", 
				"Recipe_SwordSlayer_Lightning", 
				"Recipe_SwordSlayer_Nature");

			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.Shields, 
				"Recipe_ShieldFlametal", 
				"Recipe_ShieldFlametalTower");

			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.Bows, 
				"Recipe_BowAshlands", 
				"Recipe_BowAshlands_Blood", 
				"Recipe_BowAshlands_Lightning", 
				"Recipe_BowAshlands_Nature", 
				"Recipe_CrossbowRipper", 
				"Recipe_CrossbowRipper_Blood", 
				"Recipe_CrossbowRipper_Lightning", 
				"Recipe_CrossbowRipper_Nature");

			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.Magic, 
				"Recipe_StaffClusterbomb", 
				"Recipe_StaffLightning", 
				"Recipe_StaffGreenRoots", 
				"Recipe_StaffRedTroll");

			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.Ammo, 
				"Recipe_ArrowCharred", 
				"Recipe_BoltCharred", 
				"Recipe_TurretBoltFlametal", 
				"Recipe_CatapultPayload_Grausten", 
				"Recipe_FishingBaitAshlands");

			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.Food, 
				"Recipe_MashedMeat", 
				"Recipe_FierySvinstew", 
				"Recipe_ScorchingMedley", 
				"Recipe_SpiceInducedMarmalade", 
				"Recipe_MarinatedGreens", 
				"Recipe_SizzlingBerryBroth", 
				"Recipe_SparklingShroomshake", 
				"Recipe_PiquantPie", 
				"Recipe_RoastedCrustPie", 
				"Recipe_FeastAshlands");

			AddRecipes(CraftingBiome.Ashlands, RecipeCategory.Misc, 
				"Recipe_SaddleAsksvin");


			// Deep North
			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Tools, 
				"Recipe_SnowShovel");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Armor, 
				"Recipe_HelmetGoldMage", 
				"Recipe_ArmorGoldMageChest", 
				"Recipe_ArmorGoldMageLegs", 
				"Recipe_HelmetGoldMedium", 
				"Recipe_ArmorGoldMediumChest", 
				"Recipe_ArmorGoldMediumLegs", 
				"Recipe_HelmetGold", 
				"Recipe_ArmorGoldChest", 
				"Recipe_ArmorGoldLegs");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Capes, 
				"Recipe_CapeDeepNorth", 
				"Recipe_CapeDeepNorthMage");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Trinkets, 
				"Recipe_TrinketBloodGoldHealth", 
				"Recipe_TrinketBloodGoldStamina");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.OneHandedWeapons, 
				"Recipe_KnifeGold", 
				"Recipe_KnifeGold_BloodLightning", 
				"Recipe_KnifeGold_FrostFire", 
				"Recipe_SpearGold", 
				"Recipe_SpearGold_BloodLightning", 
				"Recipe_SpearGold_FrostFire", 
				"Recipe_SwordGold", 
				"Recipe_SwordGold_BloodLightning", 
				"Recipe_SwordGold_FrostFire", 
				"Recipe_MaceGold", 
				"Recipe_MaceGold_BloodLightning", 
				"Recipe_MaceGold_FrostFire", 
				"Recipe_AxeGold", 
				"Recipe_AxeGold_BloodLightning", 
				"Recipe_AxeGold_FrostFire");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.TwoHandedWeapons, 
				"Recipe_AtgeirGold", 
				"Recipe_AtgeirGold_BloodLightning", 
				"Recipe_AtgeirGold_FrostFire", 
				"Recipe_BattleaxeGold", 
				"Recipe_BattleaxeGold_BloodLightning", 
				"Recipe_BattleaxeGold_FrostFire", 
				"Recipe_FistweaponGold", 
				"Recipe_FistGold_BloodLightning", 
				"Recipe_FistGold_FrostFire", 
				"Recipe_SledgeGold", 
				"Recipe_SledgeGold_BloodLightning", 
				"Recipe_SledgeGold_FrostFire", 
				"Recipe_THSwordGold", 
				"Recipe_THSwordGold_BloodLightning", 
				"Recipe_THSwordGold_FrostFire");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Shields, 
				"Recipe_ShieldBucklerGold", 
				"Recipe_ShieldRoundGold", 
				"Recipe_ShieldTowerGold");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Bows, 
				"Recipe_BowGold", 
				"Recipe_BowGold_BloodLightning", 
				"Recipe_BowGold_FrostFire", 
				"Recipe_CrossbowGold", 
				"Recipe_CrossbowGold_BloodLightning", 
				"Recipe_CrossbowGold_FrostFire");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Magic, 
				"Recipe_StaffFrostOrbs_Upgrade", 
				"Recipe_StaffOrbOfAhri_Upgrade", 
				"Recipe_StaffSpiritCaller_Upgrade", 
				"Recipe_StaffThunderBlood_Upgrade");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Ammo, 
				"Recipe_ArrowBloodGold", 
				"Recipe_BoltBloodGold", 
				"Recipe_Catapult_Ammo_BloodGold", 
				"Recipe_TurretBoltBloodGold", 
				"Recipe_FishingBaitDeepNorth");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Food, 
				"Recipe_BakedPoteitr", 
				"Recipe_KaleChips", 
				"Recipe_LingonDricka", 
				"Recipe_MeatballsMashedPoteitr", 
				"Recipe_MooseKebab", 
				"Recipe_OatmealLingonberryJam", 
				"Recipe_OatMilk", 
				"Recipe_OvenPancake", 
				"Recipe_Pancakes", 
				"Recipe_SealSoup", 
				"Recipe_SmokedFish", 
				"Recipe_SmokedMooseMeat", 
				"Recipe_FeastDeepNorth");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Materials, 
				"Recipe_ArmorGoldChestUncooked", 
				"Recipe_ArmorGoldLegsUncooked", 
				"Recipe_HelmetGoldUncooked", 
				"Recipe_ArmorGoldMediumChestUncooked", 
				"Recipe_ArmorGoldMediumLegsUncooked", 
				"Recipe_HelmetGoldMediumUncooked", 
				"Recipe_ArmorGoldMageChestUncooked", 
				"Recipe_ArmorGoldMageLegsUncooked", 
				"Recipe_HelmetGoldMageUncooked", 
				"Recipe_KnifeGoldUncooked", 
				"Recipe_SpearGoldUncooked", 
				"Recipe_SwordGoldUncooked", 
				"Recipe_MaceGoldUncooked", 
				"Recipe_AxeGoldUncooked", 
				"Recipe_AtgeirGoldUncooked", 
				"Recipe_BattleaxeGoldUncooked", 
				"Recipe_FistweaponGoldUncooked", 
				"Recipe_SledgeGoldUncooked", 
				"Recipe_THSwordGoldUncooked", 
				"Recipe_ShieldBucklerGoldUncooked", 
				"Recipe_ShieldRoundGoldUncooked", 
				"Recipe_ShieldTowerGoldUncooked", 
				"Recipe_BowGoldUncooked", 
				"Recipe_CrossbowGoldUncooked", 
				"Recipe_StaffFrostOrbs", 
				"Recipe_StaffOrbOfAhri", 
				"Recipe_StaffSpiritCaller", 
				"Recipe_StaffThunderBlood", 
				"Recipe_BloodGoldKeyUncooked");

			AddRecipes(CraftingBiome.DeepNorth, RecipeCategory.Misc, 
				"Recipe_SaddleMoose");

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