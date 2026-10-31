
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using MarsarahUI.Managers;
using static Skills;

namespace MarsarahUI.Patches.Logic
{
	internal static class CharacterStatisticsLogic
	{
		private static readonly LogManager log = new LogManager("Character Statistics Logic", LogManager.LogLevel.Warning);
		private static readonly MethodInfo getSkillMethod = AccessTools.Method(typeof(Skills), "GetSkill", new Type[] { typeof(SkillType) });

		internal readonly struct NotableFact
		{
			internal readonly string Title;
			internal readonly string Description;

			internal NotableFact(string title, string description)
			{
				Title = title;
				Description = description;
			}
		}

		internal static string BuildStatisticsText(PlayerProfile profile)
		{
			int enemiesSlain = Mathf.RoundToInt(GetStat(profile, PlayerStatType.EnemyKills));
			int bossesSlain = Mathf.RoundToInt(GetStat(profile, PlayerStatType.BossKills));
			int deaths = Mathf.RoundToInt(GetStat(profile, PlayerStatType.Deaths));
			int longestSurvival = Mathf.RoundToInt(GetStat(profile, PlayerStatType.ConsecutiveDaysSurvivedMax));
			int arrowsFired = Mathf.RoundToInt(GetStat(profile, PlayerStatType.ArrowsShot));
			int tombstonesRecovered = Mathf.RoundToInt(GetStat(profile, PlayerStatType.TombstonesOpenedOwn));

			int itemsCrafted = Mathf.RoundToInt(GetStat(profile, PlayerStatType.Crafts));
			int itemsUpgraded = Mathf.RoundToInt(GetStat(profile, PlayerStatType.Upgrades));
			int piecesBuilt = Mathf.RoundToInt(GetStat(profile, PlayerStatType.Builds));
			int thingsMined = Mathf.RoundToInt(GetStat(profile, PlayerStatType.Mines));
			int treesFelled = Mathf.RoundToInt(GetStat(profile, PlayerStatType.Tree));
			int creaturesTamed = Mathf.RoundToInt(GetStat(profile, PlayerStatType.CreatureTamed));
			int fishCaught = Mathf.RoundToInt(GetStat(profile, PlayerStatType.FishCaught));
			float timeInBase = GetStat(profile, PlayerStatType.TimeInBase);

			float distanceTraveled = GetStat(profile, PlayerStatType.DistanceTraveled) / 1000f;
			float distanceSailed = GetStat(profile, PlayerStatType.DistanceSail) / 1000f;
			int portalsUsed = Mathf.RoundToInt(GetStat(profile, PlayerStatType.PortalsUsed));
			int jumps = Mathf.RoundToInt(GetStat(profile, PlayerStatType.Jumps));

			return
				$"\n" +
				$"{enemiesSlain:N0}\n" +
				$"{bossesSlain:N0}\n" +
				$"{deaths:N0}\n" +
				$"{longestSurvival:N0} days\n" +
				$"{arrowsFired:N0}\n" +
				$"{tombstonesRecovered:N0}\n\n" +
				$"\n" +
				$"{itemsCrafted:N0}\n" +
				$"{itemsUpgraded:N0}\n" +
				$"{piecesBuilt:N0}\n" +
				$"{thingsMined:N0}\n" +
				$"{treesFelled:N0}\n" +
				$"{creaturesTamed:N0}\n" +
				$"{fishCaught:N0}\n" +
				$"{FormatTime(timeInBase)}\n\n" +
				$"\n" +
				$"{distanceTraveled:N1} km\n" +
				$"{distanceSailed:N1} km\n" +
				$"{portalsUsed:N0}\n" +
				$"{jumps:N0}";
		}

		internal static float GetStat(PlayerProfile profile, PlayerStatType statType)
		{
			if (profile.m_playerStats == null || profile.m_playerStats.Length == 0) return 0f;
			if (profile.m_playerStats[0] == null || profile.m_playerStats[0].m_stats == null) return 0f;

			return profile.m_playerStats[0].m_stats.TryGetValue(statType, out float value) ? value : 0f;
		}

		internal static string FormatPlaytime(PlayerProfile profile)
		{
			float totalSeconds = GetStat(profile, PlayerStatType.TimeInBase) + GetStat(profile, PlayerStatType.TimeOutOfBase);

			return FormatTime(totalSeconds);
		}

		private static string FormatTime(float totalSeconds)
		{
			int totalMinutes = Mathf.FloorToInt(totalSeconds / 60f);
			int hours = totalMinutes / 60;
			int minutes = totalMinutes % 60;

			return $"{hours:N0}h {minutes:00}m";
		}

		internal static NotableFact GetRandomNotableFact(PlayerProfile profile, Skills skills)
		{
			List<NotableFact> facts = new List<NotableFact>();

			// Rare or unusual deaths. One occurrence is already notable.
			AddDeathFact(facts, profile, PlayerStatType.DeathByTree, 1, "The Forest Remembers", "Killed by a falling tree");
			AddDeathFact(facts, profile, PlayerStatType.DeathByEdgeOfWorld, 1, "Curiosity Won", "Claimed by the edge of the world");
			AddDeathFact(facts, profile, PlayerStatType.DeathByCart, 1, "Occupational Hazard", "Killed by a cart");
			AddDeathFact(facts, profile, PlayerStatType.DeathByCatapult, 1, "Flight Test Failed", "Killed by a catapult");
			AddDeathFact(facts, profile, PlayerStatType.DeathByDrawBridge, 1, "Workplace Safety Violation", "Killed by a drawbridge");
			AddDeathFact(facts, profile, PlayerStatType.DeathByIncinerator, 1, "Disposal Error", "Killed by the obliterator");
			AddDeathFact(facts, profile, PlayerStatType.DeathByTurret, 1, "Security System Working as Intended", "Killed by a turret");
			AddDeathFact(facts, profile, PlayerStatType.DeathByBoat, 1, "Captain Went Down With the Ship", "Killed by a boat");
			AddDeathFact(facts, profile, PlayerStatType.DeathByAshlandsLava, 1, "The Floor Was Lava", "Killed by Ashlands lava");
			AddDeathFact(facts, profile, PlayerStatType.DeathByAshlandsOcean, 1, "Hostile Waters", "Killed by the Ashlands ocean");

			// More ordinary deaths only become notable when they form a pattern.
			AddDeathFact(facts, profile, PlayerStatType.DeathByFall, 3, "Natural Enemy: Gravity", "Died from falling");
			AddDeathFact(facts, profile, PlayerStatType.DeathByDrowning, 2, "Swimming Lessons Recommended", "Drowned");
			AddDeathFact(facts, profile, PlayerStatType.DeathBySmoke, 2, "Ventilation Required", "Died from smoke inhalation");
			AddDeathFact(facts, profile, PlayerStatType.DeathBySelf, 2, "Own Worst Enemy", "Died by your own hand");
			AddDeathFact(facts, profile, PlayerStatType.DeathByBurning, 3, "Fire Safety Optional", "Burned to death");
			AddDeathFact(facts, profile, PlayerStatType.DeathByFreezing, 3, "Should Have Packed a Cloak", "Frozen to death");
			AddDeathFact(facts, profile, PlayerStatType.DeathByPoisoned, 3, "Poison Control", "Died from poison");

			int bossLastHits = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.BossLastHits));
			if (bossLastHits >= 5)
				facts.Add(new NotableFact("The Finishing Touch", $"Dealt the final blow to {bossLastHits:N0} bosses."));

			int fishLost = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.FishLost));
			if (fishLost >= 10)
				facts.Add(new NotableFact("The Ones That Got Away", $"Lost {fishLost:N0} hooked fish."));

			int ravenHits = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.RavenHits));
			if (ravenHits >= 5)
				facts.Add(new NotableFact("Odin's HR Has Been Notified", $"Hit a raven {ravenHits:N0} {Pluralize(ravenHits, "time", "times")}."));

			int skeletonSummons = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.SkeletonSummons));
			if (skeletonSummons >= 250)
				facts.Add(new NotableFact("Necromancy Is a Hobby", $"Summoned {skeletonSummons:N0} skeletons."));

			int otherTombstones = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.TombstonesOpenedOther));
			if (otherTombstones >= 5)
				facts.Add(new NotableFact("Retrieval Specialist", $"Recovered {otherTombstones:N0} other players' tombstones."));

			int doorsOpened = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.DoorsOpened));
			int doorsClosed = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.DoorsClosed));
			int doorsLeftOpen = doorsOpened - doorsClosed;

			if (doorsOpened > 0 && doorsLeftOpen >= 100 && doorsLeftOpen >= doorsOpened * 0.1f)
				facts.Add(new NotableFact("Born in a Barn", $"Opened {doorsLeftOpen:N0} more doors than you closed."));

			int leviathansSunk = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.LeviathanSink));
			if (leviathansSunk >= 5)
				facts.Add(new NotableFact("Thar She Blows", $"Sank {leviathansSunk:N0} {Pluralize(leviathansSunk, "leviathan", "leviathans")}."));

			int treasuresFound =
				Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.TreasureBuriedFound)) +
				//Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.TreasureDungeonFound)) +
				Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.TreasureLocationFound));

			if (treasuresFound >= 10)
				facts.Add(new NotableFact("Treasure Hunter", $"Discovered {treasuresFound:N0} hidden treasures."));

			float distanceSailedAtHelm = CharacterStatisticsLogic.GetStat(profile, PlayerStatType.DistanceSailHelm) / 1000f;
			if (distanceSailedAtHelm >= 100f)
				facts.Add(new NotableFact("Captain", $"Sailed {distanceSailedAtHelm:N1} km at the helm."));

			int builtPieces = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.BuiltPieces));
			int removedPieces = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, PlayerStatType.BuildPiecesRemoved));

			if (builtPieces >= 100 && removedPieces >= builtPieces * 0.5f)
				facts.Add(new NotableFact("Measure Twice, Build Once?", $"Removed {removedPieces:N0} pieces after building {builtPieces:N0}."));

			if (skills != null)
			{
				if (AreAllVanillaSkillsMaxed(skills))
				{
					facts.Add(new NotableFact("Master of Everything", "Reached level 100 in every skill."));
				}
				else
				{
					AddSkillFact(facts, skills, SkillType.Jump, "Mountain Goat", "Jump");
					AddSkillFact(facts, skills, SkillType.Swim, "Still Faster Than a Boat", "Swim");
					AddSkillFact(facts, skills, SkillType.Sneak, "You Were Never Here", "Sneak");
					AddSkillFact(facts, skills, SkillType.WoodCutting, "The Forest Definitely Remembers", "Wood Cutting");
					AddSkillFact(facts, skills, SkillType.Pickaxes, "Professional Rock Argument", "Pickaxes");
					AddSkillFact(facts, skills, SkillType.Blocking, "Not Today", "Blocking");
					AddSkillFact(facts, skills, SkillType.Run, "Cardio Is a Lifestyle", "Run");
					AddSkillFact(facts, skills, SkillType.Fishing, "Fish Fear Me", "Fishing");
					AddSkillFact(facts, skills, SkillType.Ride, "Born in the Saddle", "Ride");
					AddSkillFact(facts, skills, SkillType.Farming, "Outstanding in the Field", "Farming");
					AddSkillFact(facts, skills, SkillType.Cooking, "Yes, Chef", "Cooking");
				}
			}

			if (facts.Count == 0)
				return new NotableFact("The Saga Has Just Begun", "No particularly notable exploits yet.");

			return facts[UnityEngine.Random.Range(0, facts.Count)];
		}

		private static void AddDeathFact(List<NotableFact> facts, PlayerProfile profile, PlayerStatType statType, int threshold, string title, string description)
		{
			int count = Mathf.RoundToInt(CharacterStatisticsLogic.GetStat(profile, statType));

			if (count < threshold) return;

			facts.Add(new NotableFact(title, $"{description} {count:N0} {Pluralize(count, "time", "times")}."));
		}

		private static void AddSkillFact(List<NotableFact> facts, Skills skills, SkillType skillType, string title, string skillName)
		{
			Skills.Skill skill = GetSkill(skills, skillType);

			if (skill == null || skill.m_level < 100f) return;

			facts.Add(new NotableFact(title, $"Reached level 100 in {skillName}."));
		}

		private static Skills.Skill GetSkill(Skills skills, SkillType skillType)
		{
			if (getSkillMethod == null)
			{
				log.Warn("Could not find Skills.GetSkill.");
				return null;
			}

			return getSkillMethod.Invoke(skills, new object[] { skillType }) as Skills.Skill;
		}

		private static bool AreAllVanillaSkillsMaxed(Skills skills)
		{
			if (skills.m_skills == null || skills.m_skills.Count == 0) return false;

			bool foundVanillaSkill = false;

			foreach (Skills.SkillDef skillDef in skills.m_skills)
			{
				if (skillDef == null || skillDef.m_skill == SkillType.None) continue;
				if (!Enum.IsDefined(typeof(SkillType), skillDef.m_skill)) continue;

				foundVanillaSkill = true;

				Skills.Skill skill = GetSkill(skills, skillDef.m_skill);

				if (skill == null || skill.m_level < 100f)
					return false;
			}

			return foundVanillaSkill;
		}

		private static string Pluralize(int count, string singular, string plural)
		{
			return count == 1 ? singular : plural;
		}
	}
}
