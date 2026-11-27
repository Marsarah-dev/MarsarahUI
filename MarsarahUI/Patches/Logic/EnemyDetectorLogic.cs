using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using UnityEngine;
using MarsarahUI.Patches.UI;

namespace MarsarahUI.Patches.Logic
{
	internal static class EnemyDetectorLogic
	{
		// NOTE: The UI elements for this feature are handled by UIInfoRail

		private static readonly LogManager log = new LogManager("Enemy Detector Logic", LogManager.LogLevel.Warning);

		private const float DetectionRadius = 30f;
		private const float UpdateInterval = 0.2f;

		private static float nextUpdateTime;

		private static readonly HashSet<string> lesserEnemyPrefabs = new HashSet<string>
		{
			// Deep North
			"Greydwarf_Frozen",
			"Greydwarf_Shaman_Frozen",
			"Skeleton_DeepNorth"
		};

		private static readonly HashSet<string> toughEnemyPrefabs = new HashSet<string>
		{
			// Black Forest
			"Troll",
			"Bjorn",
			"Bjorn_sleeping",

			// Swamp
			"Abomination",
			"Writhan",

			// Mountains
			"StoneGolem",

			// Plains
			"GoblinBrute",
			"Unbjorn",

			// Mistlands
			"SeekerBrute",
			"Gjall",

			// Ashlands
			"FallenValkyrie",
			"Morgen",
			"Morgen_NonSleeping",
			"BonemawSerpent",

			// Deep North
			"Barka",
			"TrollFrost",

			// Ocean - balanced around Swamp progression
			"Serpent"
		};

		private static readonly HashSet<string> minibossPrefabs = new HashSet<string>
		{
			"Skeleton_Hildir",
			"Skeleton_Hildir_nochest",
			"Fenring_Cultist_Hildir",
			"Fenring_Cultist_Hildir_nochest",
			"GoblinBruteBros",
			"GoblinBruteBros_nochest",
			"Charred_Melee_Dyrnwyn"
		};

		private static readonly List<Character> characters = new List<Character>();

		internal static int NumLesserEnemies { get; private set; }
		internal static int NumEnemies { get; private set; }
		internal static int NumToughEnemies { get; private set; }
		internal static int NumNeutralEnemies { get; private set; }
		internal static int NumBosses { get; private set; }

		[HarmonyPatch(typeof(Player), "Update")]
		private static class EnemyDetectorPlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_localPlayer == null) return;
				if (!ConfigManager.EffectiveShowEnemyDetector) return;
				if (!UIController.ShowUI) return;

				if (Time.unscaledTime < nextUpdateTime) return;
				nextUpdateTime = Time.unscaledTime + UpdateInterval;

				int lesserEnemies = 0;
				int enemies = 0;
				int toughEnemies = 0;
				int bosses = 0;
				int neutralEnemies = 0;

				ConfigManager.EnemyDetectorMode mode = ConfigManager.EffectiveEnemyDetectorChoice;

				bool splitEnemies = mode == ConfigManager.EnemyDetectorMode.Split || mode == ConfigManager.EnemyDetectorMode.SplitDeepNorth;
				bool splitLesserEnemies = mode == ConfigManager.EnemyDetectorMode.SplitDeepNorth;

				characters.Clear();
				Character.GetCharactersInRange(___m_localPlayer.transform.position, DetectionRadius, characters);

				foreach (Character character in characters)
				{
					if (ShouldIgnoreCharacter(character)) continue;

					if (IsBossOrMiniboss(character))
					{
						if (splitEnemies)
						{
							bosses++;
						}
						else
						{
							enemies++;
						}

						continue;
					}

					if (IsNeutralEnemy(character))
					{
						neutralEnemies++;
						continue;
					}

					if (splitEnemies && IsToughEnemy(character))
					{
						toughEnemies++;
						continue;
					}

					if (splitLesserEnemies && IsLesserEnemy(character))
					{
						lesserEnemies++;
						continue;
					}

					enemies++;
				}

				NumLesserEnemies = lesserEnemies;
				NumEnemies = enemies;
				NumToughEnemies = toughEnemies;
				NumBosses = bosses;
				NumNeutralEnemies = neutralEnemies;
			}
		}

		private static bool ShouldIgnoreCharacter(Character character)
		{
			if (character == null) return true;

			/*if (character.m_name != "Human")
				log.Info($"Detected character: {character.m_name}");*/

			return character.m_name == "Human" ||
				character.m_name == "$enemy_deer" ||
				character.m_name == "$enemy_hare" ||
				character.m_name == "$enemy_kvastur" ||
				character.m_name == "$enemy_summonedroot" ||
				character.m_name == "$piece_trainingdummy" ||
				character.m_name == "$enemy_seal" ||
				character.m_name == "$enemy_seal_baby" ||
				character.m_name == "$enemy_shadowperson" ||
				character.IsTamed();
		}

		private static bool IsNeutralEnemy(Character character)
		{
			if (character.GetFaction() != Character.Faction.Dverger) return false;

			BaseAI baseAI = character.GetBaseAI();

			return baseAI != null && !baseAI.IsAggravated();
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

		private static bool IsToughEnemy(Character character)
		{
			return toughEnemyPrefabs.Contains(GetPrefabName(character));
		}

		private static bool IsLesserEnemy(Character character)
		{
			return lesserEnemyPrefabs.Contains(GetPrefabName(character));
		}
	}
}