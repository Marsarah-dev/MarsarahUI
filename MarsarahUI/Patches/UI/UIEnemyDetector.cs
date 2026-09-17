using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;

namespace MarsarahUI.Patches.UI
{
	internal class UIEnemyDetector
	{
		private static readonly LogManager log = new LogManager("UI Enemy Detector", LogManager.LogLevel.Warning);

		private const float DetectionRadius = 30f;

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

			// Ocean - balanced around Swamp progression
			"Serpent"
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

				bool splitEnemies =	ConfigManager.EffectiveEnemyDetectorChoice == ConfigManager.EnemyDetectorMode.Split;

				List<Character> characters = new List<Character>();
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

					enemies++;
				}

				NumEnemies = enemies;
				NumToughEnemies = toughEnemies;
				NumBosses = bosses;
				NumNeutralEnemies = neutralEnemies;
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

		private static bool IsToughEnemy(Character character)
		{
			return toughEnemyPrefabs.Contains(GetPrefabName(character));
		}
	}
}