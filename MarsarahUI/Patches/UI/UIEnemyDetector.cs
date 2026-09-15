using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;

namespace MarsarahUI.Patches.UI
{
	internal class UIEnemyDetector
	{
		private static readonly LogManager log = new LogManager("UI Enemy Detector", LogManager.LogLevel.Warning);

		private const float DetectionRadius = 30f;
		private const float BossDetectionRadius = 60f;

		private static readonly Dictionary<string, string> toughEnemyTiers = new Dictionary<string, string>
		{
			// Black Forest
			{ "Troll", "BlackForest" },
			{ "Bjorn", "BlackForest" },
			{ "Bjorn_sleeping", "BlackForest" },

			// Swamp
			{ "Abomination", "Swamp" },
			//{ "Wraith", "Swamp" },
			{ "Writhan", "Swamp" },

			// Mountains
			{ "StoneGolem", "Mountain" },

			// Plains
			{ "GoblinBrute", "Plains" },
			{ "Unbjorn", "Plains" },

			// Mistlands
			{ "SeekerBrute", "Mistlands" },
			{ "Gjall", "Mistlands" },

			// Ashlands
			{ "FallenValkyrie", "AshLands" },
			{ "Morgen", "AshLands" },
			{ "Morgen_NonSleeping", "AshLands" },
			{ "BonemawSerpent", "AshLands" },

			// Ocean - balanced around Swamp progression
			{ "Serpent", "Swamp" }
		};

		private static readonly HashSet<string> minibossPrefabs = new HashSet<string>
		{
			"Skeleton_Hildir",
			"Fenring_Cultist_Hildir",
			"GoblinBruteBros",
			"Charred_Melee_Dyrnwyn"
		};

		internal static int NumEnemies { get; private set; }
		internal static int NumToughEnemies { get; private set; }
		internal static int NumNeutralEnemies { get; private set; }
		internal static GearProgressionManager.ThreatLevel ToughEnemyThreatLevel { get; private set; }
		internal static int NumBosses { get; private set; }

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
				int bosses = 0;
				int neutralEnemies = 0;

				GearProgressionManager.ThreatLevel highestThreat = GearProgressionManager.ThreatLevel.Safe;
				int armorWeight = GearProgressionManager.GetEquippedArmorWeight(___m_localPlayer);

				bool separateToughEnemies =
					ConfigManager.EffectiveEnemyDetectorChoice ==
					ConfigManager.EnemyDetectorMode.SeparateToughEnemies;

				List<Character> characters = new List<Character>();
				Character.GetCharactersInRange(___m_localPlayer.transform.position, BossDetectionRadius, characters);

				foreach (Character character in characters)
				{
					if (ShouldIgnoreCharacter(character)) continue;

					bool isBoss = IsBossOrMiniboss(character);

					if (isBoss)
					{
						bosses++;
						continue;
					}

					float distanceSqr = (character.transform.position - ___m_localPlayer.transform.position).sqrMagnitude;

					if (distanceSqr > DetectionRadius * DetectionRadius) continue;

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
				NumBosses = bosses;
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

		private static bool IsBossOrMiniboss(Character character)
		{
			if (character.IsBoss()) return true;

			string prefabName = GetPrefabName(character);

			return minibossPrefabs.Contains(prefabName);
		}

		private static string GetPrefabName(Character character)
		{
			string prefabName = character.gameObject.name;

			if (prefabName.EndsWith("(Clone)"))
			{
				prefabName = prefabName.Substring(0, prefabName.Length - "(Clone)".Length);
			}

			return prefabName;
		}

		private static bool TryGetToughEnemyTier(Character character, out string progressionBiome)
		{
			return toughEnemyTiers.TryGetValue(GetPrefabName(character), out progressionBiome);
		}
	}
}