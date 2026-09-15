using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;

namespace MarsarahUI.Patches.UI
{
	internal class UIEnemyDetector
	{
		private static readonly LogManager log = new LogManager("UI Enemy Detector", LogManager.LogLevel.Warning);

		private static readonly Dictionary<string, string> toughEnemyTiers = new Dictionary<string, string>
		{
			// Black Forest
			{ "Troll", "BlackForest" },
			{ "Bjorn", "BlackForest" },
			{ "Bjorn_sleeping", "BlackForest" },
			{ "Skeleton_Hildir", "BlackForest" },

			// Swamp
			{ "Abomination", "Swamp" },
			{ "Wraith", "Swamp" },
			{ "Writhan", "Swamp" },

			// Mountains
			{ "StoneGolem", "Mountain" },
			{ "Fenring_Cultist_Hildir", "Mountain" },

			// Plains
			{ "GoblinBrute", "Plains" },
			{ "Unbjorn", "Plains" },
			{ "GoblinBruteBros", "Plains" },

			// Mistlands
			{ "SeekerBrute", "Mistlands" },
			{ "Gjall", "Mistlands" },

			// Ashlands
			{ "FallenValkyrie", "AshLands" },
			{ "Morgen", "AshLands" },
			{ "Morgen_NonSleeping", "AshLands" },
			{ "Charred_Melee_Dyrnwyn", "AshLands" },
			{ "BonemawSerpent", "AshLands" },

			// Ocean - balanced around Swamp progression
			{ "Serpent", "Swamp" }
		};

		internal static int NumEnemies { get; private set; }
		internal static int NumToughEnemies { get; private set; }
		internal static int NumNeutralEnemies { get; private set; }
		internal static GearProgressionManager.ThreatLevel ToughEnemyThreatLevel { get; private set; }

		[HarmonyPatch(typeof(Player), "Update")]
		private static class EnemyDetectorPlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_localPlayer == null) return;
				if (!ConfigManager.EffectiveShowEnemyDetector) return;

				int enemies = 0;
				int toughEnemies = 0;
				int neutralEnemies = 0;

				GearProgressionManager.ThreatLevel highestThreat = GearProgressionManager.ThreatLevel.Safe;
				int armorWeight = GearProgressionManager.GetEquippedArmorWeight(___m_localPlayer);

				bool separateToughEnemies =
					ConfigManager.EffectiveEnemyDetectorChoice ==
					ConfigManager.EnemyDetectorMode.SeparateToughEnemies;

				List<Character> characters = new List<Character>();
				Character.GetCharactersInRange(___m_localPlayer.transform.position, 30f, characters);

				foreach (Character character in characters)
				{
					if (ShouldIgnoreCharacter(character)) continue;

					if (IsNeutralEnemy(character))
					{
						neutralEnemies++;
						continue;
					}

					if (separateToughEnemies && TryGetToughEnemyTier(character, out string progressionBiome))
					{
						toughEnemies++;

						GearProgressionManager.ThreatLevel threat =
							GearProgressionManager.GetThreatLevel(armorWeight, progressionBiome);

						if (threat > highestThreat)
						{
							highestThreat = threat;
						}

						continue;
					}

					enemies++;
				}

				NumEnemies = enemies;
				NumToughEnemies = toughEnemies;
				NumNeutralEnemies = neutralEnemies;
				ToughEnemyThreatLevel = highestThreat;
			}
		}

		private static bool ShouldIgnoreCharacter(Character character)
		{
			if (character == null) return true;

			return character.m_name == "Human" ||
				character.m_name == "$enemy_deer" ||
				character.m_name == "$enemy_hare" ||
				character.m_name == "$enemy_summonedroot" ||
				character.IsTamed();
		}

		private static bool IsNeutralEnemy(Character character)
		{
			return character.GetFaction() == Character.Faction.Dverger &&
				character.GetBaseAI() != null &&
				!character.GetBaseAI().IsAggravated();
		}

		private static bool TryGetToughEnemyTier(Character character, out string progressionBiome)
		{
			progressionBiome = null;

			string prefabName = character.gameObject.name;

			if (prefabName.EndsWith("(Clone)"))
			{
				prefabName = prefabName.Substring(0, prefabName.Length - "(Clone)".Length);
			}

			return toughEnemyTiers.TryGetValue(prefabName, out progressionBiome);
		}
	}
}