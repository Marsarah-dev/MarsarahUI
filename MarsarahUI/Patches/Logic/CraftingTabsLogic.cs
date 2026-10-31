
using System.Collections.Generic;
using MarsarahUI.Managers;

namespace MarsarahUI.Patches.Logic
{
	internal enum CraftingTab
	{
		All,
		Meadows,
		BlackForest,
		Swamp,
		Mountain,
		Ocean,
		Plains,
		Mistlands,
		Ashlands,
		DeepNorth,
		Feasts,
		Recovery,
		Resist,
		Utility,
		Other
	}

	internal enum CraftingTabProfile
	{
		Biomes,
		MeadKettle,
		FoodPreparation,
		NoCost
	}

	internal static class CraftingTabsLogic
	{
		private static readonly LogManager log = new LogManager("Biome Crafting", LogManager.LogLevel.Warning);

		internal enum CraftingBiome
		{
			Meadows,
			BlackForest,
			Swamp,
			Mountain,
			Ocean,
			Plains,
			Mistlands,
			Ashlands,
			DeepNorth
		}

		internal enum RecipeCategory
		{
			Materials,
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
			Misc
		}

		internal enum SpecialRecipeGroup
		{
			Feast,
			MeadRecovery,
			MeadResistance,
			MeadUtility
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
		private static readonly Dictionary<string, SpecialRecipeGroup> specialRecipeGroups = new Dictionary<string, SpecialRecipeGroup>();

		static CraftingTabsLogic()
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

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.TwoHandedWeapons,
				"Recipe_AxeEarly");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Shields,
				"Recipe_ShieldWood",
				"Recipe_ShieldWoodTower");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Bows,
				"Recipe_Bow");

			AddRecipes(CraftingBiome.Meadows, RecipeCategory.Ammo,
				"Recipe_ArrowWood",
				"Recipe_ArrowFire",
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
				"Recipe_HelmetBerserker",
				"Recipe_ArmorBerserkerChest",
				"Recipe_ArmorBerserkerLegs",
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
				"Recipe_FistBjornClaw",
				"Recipe_AtgeirBronze",
				"Recipe_SledgeStagbreaker",
				"Recipe_PickaxeBronze");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Shields,
				"Recipe_ShieldBronzeBuckler",
				"Recipe_ShieldBoneTower");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Bows,
				"Recipe_BowFineWood");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Ammo,
				"Recipe_ArrowBronze");

			AddRecipes(CraftingBiome.BlackForest, RecipeCategory.Food,
				"Recipe_BoarJerky",
				"Recipe_CarrotSoup",
				"Recipe_DeerStew",
				"Recipe_MinceMeatSauce",
				"Recipe_QueensJam",
				"Recipe_PulledBear",
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
				"Recipe_ShieldIronTower",
				"Recipe_ShieldRoots");

			AddRecipes(CraftingBiome.Swamp, RecipeCategory.Bows,
				"Recipe_BowHuntsman");

			AddRecipes(CraftingBiome.Swamp, RecipeCategory.Ammo,
				"Recipe_ArrowIron",
				"Recipe_BombOoze",
				"Recipe_BombBlob_Poison",
				"Recipe_BombBlob_PoisonElite");

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
				"Recipe_ArrowSilver",
				"Recipe_BombBlob_Frost");

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
				"Recipe_ArmorPaddedGreaves",
				"Recipe_HelmetLox",
				"Recipe_ArmorLoxChest",
				"Recipe_ArmorLoxLegs",
				"Recipe_HelmetBerserkerUndead",
				"Recipe_ArmorBerserkerUndeadChest",
				"Recipe_ArmorBerserkerUndeadLegs");

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
				"Recipe_Battleaxe_Blackmetal",
				"Recipe_FistUnbjornClaw",
				"Recipe_PickaxeBlackMetal");

			AddRecipes(CraftingBiome.Plains, RecipeCategory.Shields,
				"Recipe_ShieldBlackmetal",
				"Recipe_ShieldBlackmetalTower");

			AddRecipes(CraftingBiome.Plains, RecipeCategory.Ammo,
				"Recipe_ArrowNeedle",
				"Recipe_BombBlob_Tar");

			AddRecipes(CraftingBiome.Plains, RecipeCategory.Food,
				"Recipe_BloodPudding",
				"Recipe_FishWraps",
				"Recipe_Bread",
				"Recipe_FishAndBread",
				"Recipe_LoxPie",
				"Recipe_VikingCupcake",
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
				"Recipe_Battleaxe_SkullSplittur",
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
				"Recipe_BombBile");

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

			// MC: Keeping this here commented. 
			//AddRecipes(CraftingBiome.Mistlands, RecipeCategory.Misc, 
			//	"Recipe_DvergrKey");


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
				"Recipe_BombBlob_Lava",
				"Recipe_BombLava",
				"Recipe_BombSmoke",
				"Recipe_BombSiege",
				"Recipe_CatapultPayload_Grausten");

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
				"Recipe_ArmorGoldLegs",
				"Recipe_HelmetCrownOfValheim");

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
				"Recipe_BombBlob_Morkhalla",
				"Recipe_BombDynamite");

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
				"Recipe_FishSoup",
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

			AddSpecialRecipes(SpecialRecipeGroup.Feast,
				"Recipe_FeastMeadows",
				"Recipe_FeastBlackforest",
				"Recipe_FeastSwamps",
				"Recipe_FeastMountains",
				"Recipe_FeastOceans",
				"Recipe_FeastPlains",
				"Recipe_FeastMistlands",
				"Recipe_FeastAshlands",
				"Recipe_FeastDeepNorth");

			AddSpecialRecipes(SpecialRecipeGroup.MeadRecovery,
				"Recipe_MeadBaseHealthMinor",
				"Recipe_MeadBaseHealthMedium",
				"Recipe_MeadBaseHealthMajor",
				"Recipe_MeadBaseHealthLingering",
				"Recipe_MeadBaseStaminaMinor",
				"Recipe_MeadBaseStaminaMedium",
				"Recipe_MeadBaseStaminaLingering",
				"Recipe_MeadBaseEitrMinor",
				"Recipe_MeadBaseEitrLingering");

			AddSpecialRecipes(SpecialRecipeGroup.MeadResistance,
				"Recipe_BarleyWineBase",
				"Recipe_MeadBaseFrostResist",
				"Recipe_MeadBasePoisonResist",
				"Recipe_MeadBaseBugRepellent");

			AddSpecialRecipes(SpecialRecipeGroup.MeadUtility,
				"Recipe_MeadBaseBzerker",
				"Recipe_MeadBaseHasty",
				"Recipe_MeadBaseLightFoot",
				"Recipe_MeadBaseStrength",
				"Recipe_MeadBaseSwimmer",
				"Recipe_MeadBaseTamer",
				"Recipe_MeadBaseTasty");

			log.Info($"Loaded {recipeClassifications.Count} biome crafting recipe classifications.");
		}

		internal static bool TryGetClassification(Recipe recipe, out RecipeClassification classification)
		{
			classification = null;

			if (recipe == null)
				return false;

			if (recipe.name == "Recipe_CapeLinen" && CompatibilityManager.TweaksEarlyLinenCapeEnabled)
			{
				classification = new RecipeClassification(CraftingBiome.Swamp, RecipeCategory.Capes, 10);
				return true;
			}

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

		private static void AddSpecialRecipes(SpecialRecipeGroup group, params string[] recipeNames)
		{
			foreach (string recipeName in recipeNames)
			{
				if (specialRecipeGroups.ContainsKey(recipeName))
				{
					log.Warn($"Duplicate special crafting classification for recipe '{recipeName}'.");
					continue;
				}

				specialRecipeGroups.Add(recipeName, group);
			}
		}

		// Tab groups
		internal static bool TryGetSpecialGroup(Recipe recipe, out SpecialRecipeGroup group)
		{
			group = default;

			if (recipe == null)
				return false;

			return specialRecipeGroups.TryGetValue(recipe.name, out group);
		}

		internal static bool TryGetBiomeForTab(CraftingTab tab, out CraftingBiome biome)
		{
			switch (tab)
			{
				case CraftingTab.Meadows:
					biome = CraftingBiome.Meadows;
					return true;

				case CraftingTab.BlackForest:
					biome = CraftingBiome.BlackForest;
					return true;

				case CraftingTab.Swamp:
					biome = CraftingBiome.Swamp;
					return true;

				case CraftingTab.Mountain:
					biome = CraftingBiome.Mountain;
					return true;

				case CraftingTab.Plains:
					biome = CraftingBiome.Plains;
					return true;

				case CraftingTab.Ocean:
					biome = CraftingBiome.Ocean;
					return true;

				case CraftingTab.Mistlands:
					biome = CraftingBiome.Mistlands;
					return true;

				case CraftingTab.Ashlands:
					biome = CraftingBiome.Ashlands;
					return true;

				case CraftingTab.DeepNorth:
					biome = CraftingBiome.DeepNorth;
					return true;

				default:
					biome = default;
					return false;
			}
		}

		internal static bool TryGetTabForBiome(CraftingBiome biome, out CraftingTab tab)
		{
			switch (biome)
			{
				case CraftingBiome.Meadows:
					tab = CraftingTab.Meadows;
					return true;

				case CraftingBiome.BlackForest:
					tab = CraftingTab.BlackForest;
					return true;

				case CraftingBiome.Swamp:
					tab = CraftingTab.Swamp;
					return true;

				case CraftingBiome.Mountain:
					tab = CraftingTab.Mountain;
					return true;

				case CraftingBiome.Plains:
					tab = CraftingTab.Plains;
					return true;

				case CraftingBiome.Ocean:
					tab = CraftingTab.Ocean;
					return true;

				case CraftingBiome.Mistlands:
					tab = CraftingTab.Mistlands;
					return true;

				case CraftingBiome.Ashlands:
					tab = CraftingTab.Ashlands;
					return true;

				case CraftingBiome.DeepNorth:
					tab = CraftingTab.DeepNorth;
					return true;

				default:
					tab = default;
					return false;
			}
		}

		internal static bool IsFoodPreparationBiomeTab(CraftingTab tab)
		{
			return tab == CraftingTab.Plains ||
				tab == CraftingTab.Mistlands ||
				tab == CraftingTab.Ashlands ||
				tab == CraftingTab.DeepNorth;
		}

		// Filtering
		internal static bool RecipeMatchesSelectedTab(Recipe recipe, CraftingTab selectedTab, CraftingTabProfile profile)
		{
			if (selectedTab == CraftingTab.All)
				return true;

			switch (profile)
			{
				case CraftingTabProfile.MeadKettle:
					return RecipeMatchesMeadKettleTab(recipe, selectedTab);

				case CraftingTabProfile.FoodPreparation:
					return RecipeMatchesFoodPreparationTab(recipe, selectedTab);

				case CraftingTabProfile.NoCost:
					return RecipeMatchesNoCostTab(recipe, selectedTab);

				default:
					return RecipeMatchesBiomeTab(recipe, selectedTab);
			}
		}

		private static bool RecipeMatchesBiomeTab(Recipe recipe, CraftingTab selectedTab)
		{
			if (selectedTab == CraftingTab.Other)
				return !TryGetClassification(recipe, out _);

			if (!TryGetClassification(recipe, out RecipeClassification classification))
				return false;

			if (!TryGetBiomeForTab(selectedTab, out CraftingBiome biome))
				return false;

			return classification.Biome == biome;
		}

		private static bool RecipeMatchesMeadKettleTab(Recipe recipe, CraftingTab selectedTab)
		{
			if (!TryGetSpecialGroup(recipe, out SpecialRecipeGroup group))
				return selectedTab == CraftingTab.Other;

			switch (selectedTab)
			{
				case CraftingTab.Recovery:
					return group == SpecialRecipeGroup.MeadRecovery;

				case CraftingTab.Resist:
					return group == SpecialRecipeGroup.MeadResistance;

				case CraftingTab.Utility:
					return group == SpecialRecipeGroup.MeadUtility;

				case CraftingTab.Other:
					return group != SpecialRecipeGroup.MeadRecovery &&
						group != SpecialRecipeGroup.MeadResistance &&
						group != SpecialRecipeGroup.MeadUtility;

				default:
					return false;
			}
		}

		private static bool RecipeMatchesFoodPreparationTab(Recipe recipe, CraftingTab selectedTab)
		{
			if (TryGetSpecialGroup(recipe, out SpecialRecipeGroup group) &&
				group == SpecialRecipeGroup.Feast)
			{
				return selectedTab == CraftingTab.Feasts;
			}

			if (TryGetClassification(recipe, out RecipeClassification classification) &&
				TryGetTabForBiome(classification.Biome, out CraftingTab tab) &&
				IsFoodPreparationBiomeTab(tab))
			{
				return selectedTab == tab;
			}

			return selectedTab == CraftingTab.Other;
		}

		private static bool RecipeMatchesNoCostTab(Recipe recipe, CraftingTab selectedTab)
		{
			if (selectedTab == CraftingTab.Other)
			{
				return !TryGetClassification(recipe, out _) &&
					!TryGetSpecialGroup(recipe, out _);
			}

			if (TryGetBiomeForTab(selectedTab, out CraftingBiome biome) &&
				TryGetClassification(recipe, out RecipeClassification classification))
			{
				return classification.Biome == biome;
			}

			if (!TryGetSpecialGroup(recipe, out SpecialRecipeGroup group))
				return false;

			switch (selectedTab)
			{
				case CraftingTab.Feasts:
					return group == SpecialRecipeGroup.Feast;

				case CraftingTab.Recovery:
					return group == SpecialRecipeGroup.MeadRecovery;

				case CraftingTab.Resist:
					return group == SpecialRecipeGroup.MeadResistance;

				case CraftingTab.Utility:
					return group == SpecialRecipeGroup.MeadUtility;

				default:
					return false;
			}
		}


		// Available crafting tabs
		internal static void UpdateAvailableTabs(List<Recipe> recipes, CraftingTabProfile profile, HashSet<CraftingTab> availableTabs)
		{
			availableTabs.Clear();
			availableTabs.Add(CraftingTab.All);

			switch (profile)
			{
				case CraftingTabProfile.MeadKettle:
					UpdateMeadKettleTabs(recipes, availableTabs);
					break;

				case CraftingTabProfile.FoodPreparation:
					UpdateFoodPreparationTabs(recipes, availableTabs);
					break;

				case CraftingTabProfile.NoCost:
					UpdateNoCostTabs(recipes, availableTabs);
					break;

				default:
					UpdateBiomeAvailableTabs(recipes, availableTabs);
					break;
			}
		}

		private static void UpdateBiomeAvailableTabs(List<Recipe> recipes, HashSet<CraftingTab> availableTabs)
		{
			bool hasOther = false;

			foreach (Recipe recipe in recipes)
			{
				if (TryGetClassification(recipe, out RecipeClassification classification) &&
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

		private static void UpdateMeadKettleTabs(List<Recipe> recipes, HashSet<CraftingTab> availableTabs)
		{
			bool hasOther = false;

			foreach (Recipe recipe in recipes)
			{
				if (!TryGetSpecialGroup(recipe, out SpecialRecipeGroup group))
				{
					hasOther = true;
					continue;
				}

				switch (group)
				{
					case SpecialRecipeGroup.MeadRecovery:
						availableTabs.Add(CraftingTab.Recovery);
						break;

					case SpecialRecipeGroup.MeadResistance:
						availableTabs.Add(CraftingTab.Resist);
						break;

					case SpecialRecipeGroup.MeadUtility:
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

		private static void UpdateFoodPreparationTabs(List<Recipe> recipes, HashSet<CraftingTab> availableTabs)
		{
			bool hasOther = false;

			foreach (Recipe recipe in recipes)
			{
				if (TryGetSpecialGroup(recipe, out SpecialRecipeGroup group) &&
					group == SpecialRecipeGroup.Feast)
				{
					availableTabs.Add(CraftingTab.Feasts);
					continue;
				}

				if (TryGetClassification(recipe, out RecipeClassification classification) &&
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

		private static void UpdateNoCostTabs(List<Recipe> recipes, HashSet<CraftingTab> availableTabs)
		{
			bool hasOther = false;

			foreach (Recipe recipe in recipes)
			{
				bool hasClassification = TryGetClassification(recipe, out RecipeClassification classification);
				bool hasSpecialGroup = TryGetSpecialGroup(recipe, out SpecialRecipeGroup specialGroup);

				if (hasClassification && TryGetTabForBiome(classification.Biome, out CraftingTab biomeTab))
					availableTabs.Add(biomeTab);

				if (hasSpecialGroup)
				{
					switch (specialGroup)
					{
						case SpecialRecipeGroup.Feast:
							availableTabs.Add(CraftingTab.Feasts);
							break;

						case SpecialRecipeGroup.MeadRecovery:
							availableTabs.Add(CraftingTab.Recovery);
							break;

						case SpecialRecipeGroup.MeadResistance:
							availableTabs.Add(CraftingTab.Resist);
							break;

						case SpecialRecipeGroup.MeadUtility:
							availableTabs.Add(CraftingTab.Utility);
							break;
					}
				}

				if (!hasClassification && !hasSpecialGroup)
					hasOther = true;
			}

			if (hasOther)
				availableTabs.Add(CraftingTab.Other);
		}


		// Recipe sorting
		internal readonly struct RecipeSortData
		{
			internal Recipe Recipe { get; }
			internal int OriginalIndex { get; }
			internal bool CanCraft { get; }

			internal RecipeSortData(Recipe recipe, int originalIndex, bool canCraft)
			{
				Recipe = recipe;
				OriginalIndex = originalIndex;
				CanCraft = canCraft;
			}
		}

		internal static int CompareRecipeEntries(RecipeSortData a, RecipeSortData b, CraftingTab selectedTab, CraftingTabProfile profile)
		{
			if (a.CanCraft != b.CanCraft)
				return a.CanCraft ? -1 : 1;

			switch (profile)
			{
				case CraftingTabProfile.MeadKettle:
					return CompareMeadRecipes(a, b, selectedTab);

				case CraftingTabProfile.FoodPreparation:
					return CompareFoodPreparationRecipes(a, b, selectedTab);

				default:
					return CompareBiomeRecipes(a, b, selectedTab);
			}
		}

		private static int CompareBiomeRecipes(RecipeSortData a, RecipeSortData b, CraftingTab selectedTab)
		{
			bool hasA = TryGetClassification(a.Recipe, out RecipeClassification classificationA);
			bool hasB = TryGetClassification(b.Recipe, out RecipeClassification classificationB);

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

		private static int CompareFoodPreparationRecipes(RecipeSortData a, RecipeSortData b, CraftingTab selectedTab)
		{
			if (selectedTab == CraftingTab.Feasts)
				return CompareFeastRecipes(a, b);

			if (selectedTab != CraftingTab.All)
				return CompareBiomeRecipes(a, b, selectedTab);

			int groupA = GetFoodPreparationSortGroup(a.Recipe);
			int groupB = GetFoodPreparationSortGroup(b.Recipe);

			int groupComparison = groupA.CompareTo(groupB);

			if (groupComparison != 0)
				return groupComparison;

			if (groupA == 4)
				return CompareFeastRecipes(a, b);

			if (groupA == 5)
				return a.OriginalIndex.CompareTo(b.OriginalIndex);

			if (TryGetClassification(a.Recipe, out RecipeClassification classificationA) &&
				TryGetClassification(b.Recipe, out RecipeClassification classificationB))
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
			if (TryGetSpecialGroup(recipe, out SpecialRecipeGroup group) &&
				group == SpecialRecipeGroup.Feast)
			{
				return 4;
			}

			if (!TryGetClassification(recipe, out RecipeClassification classification))
				return 5;

			switch (classification.Biome)
			{
				case CraftingBiome.Plains:
					return 0;

				case CraftingBiome.Mistlands:
					return 1;

				case CraftingBiome.Ashlands:
					return 2;

				case CraftingBiome.DeepNorth:
					return 3;

				default:
					return 5;
			}
		}

		private static int CompareFeastRecipes(RecipeSortData a, RecipeSortData b)
		{
			bool hasA = TryGetClassification(a.Recipe, out RecipeClassification classificationA);
			bool hasB = TryGetClassification(b.Recipe, out RecipeClassification classificationB);

			if (hasA != hasB)
				return hasA ? -1 : 1;

			if (!hasA)
				return a.OriginalIndex.CompareTo(b.OriginalIndex);

			int biomeComparison = ((int)classificationA.Biome).CompareTo((int)classificationB.Biome);

			if (biomeComparison != 0)
				return biomeComparison;

			return a.OriginalIndex.CompareTo(b.OriginalIndex);
		}

		private static int CompareMeadRecipes(RecipeSortData a, RecipeSortData b, CraftingTab selectedTab)
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
			if (!TryGetSpecialGroup(recipe, out SpecialRecipeGroup group))
				return 3;

			switch (group)
			{
				case SpecialRecipeGroup.MeadRecovery:
					return 0;

				case SpecialRecipeGroup.MeadResistance:
					return 1;

				case SpecialRecipeGroup.MeadUtility:
					return 2;

				default:
					return 3;
			}
		}
	}
}