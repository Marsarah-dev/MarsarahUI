using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static Skills;

namespace MarsarahUI.Patches.UI
{
	internal class UICharacterStatistics : UIController
	{
		private static readonly LogManager log = new LogManager("UI Character Statistics", LogManager.LogLevel.Warning);

		private static readonly FieldInfo profilesField = AccessTools.Field(typeof(FejdStartup), "m_profiles");
		private static readonly FieldInfo profileIndexField = AccessTools.Field(typeof(FejdStartup), "m_profileIndex");
		private static readonly FieldInfo characterSelectScreenField = AccessTools.Field(typeof(FejdStartup), "m_characterSelectScreen");
		private static readonly FieldInfo playerInstanceField = AccessTools.Field(typeof(FejdStartup), "m_playerInstance");
		private static readonly MethodInfo getSkillMethod = AccessTools.Method(typeof(Skills), "GetSkill", new System.Type[] { typeof(SkillType) });

		private static GameObject UICharacterStatisticsArea;
		private static Text statisticsValues;
		private static Text playtimeText;
		private static Text notableFactTitle;
		private static Text notableFactDescription;

		private static int lastFactProfileIndex = -1;
		private static NotableFact currentNotableFact;

		private struct NotableFact
		{
			internal string Title;
			internal string Description;

			internal NotableFact(string title, string description)
			{
				Title = title;
				Description = description;
			}
		}

		[HarmonyPatch(typeof(FejdStartup), "ShowCharacterSelection")]
		private static class FejdStartupShowCharacterSelectionPatch
		{
			private static void Postfix(FejdStartup __instance)
			{
				if (!ConfigManager.EffectiveShowCharacterStatistics)
					return;

				CreateUI(__instance);
				UpdateStatistics(__instance);
			}
		}

		[HarmonyPatch(typeof(FejdStartup), "UpdateCharacterList")]
		private static class FejdStartupUpdateCharacterListPatch
		{
			private static void Postfix(FejdStartup __instance)
			{
				if (UICharacterStatisticsArea == null) return;

				if (!ConfigManager.EffectiveShowCharacterStatistics)
				{
					UICharacterStatisticsArea.SetActive(false);
					return;
				}

				UICharacterStatisticsArea.SetActive(true);
				UpdateStatistics(__instance);
			}
		}

		private static void CreateUI(FejdStartup startup)
		{
			if (UICharacterStatisticsArea != null) return;

			GameObject characterSelectScreen = GetCharacterSelectScreen(startup);

			if (characterSelectScreen == null)
			{
				log.Warn("Could not find the character selection screen.");
				return;
			}

			UICharacterStatisticsArea = new GameObject("CharacterStatisticsArea");
			UICharacterStatisticsArea.layer = 5;
			UICharacterStatisticsArea.transform.SetParent(characterSelectScreen.transform, false);

			RectTransform areaTransform = UICharacterStatisticsArea.AddComponent<RectTransform>();
			areaTransform.anchorMin = new Vector2(1f, 0.5f);
			areaTransform.anchorMax = new Vector2(1f, 0.5f);
			areaTransform.pivot = new Vector2(1f, 0.5f);
			areaTransform.anchoredPosition = new Vector2(-60f, 10f);
			areaTransform.sizeDelta = new Vector2(390f, 650f);
			areaTransform.localScale = Vector3.one;

			Image background = UICharacterStatisticsArea.AddComponent<Image>();
			background.color = new Color(0f, 0f, 0f, 0.45f);
			CreateFrame(UICharacterStatisticsArea);

			CreateTextObject("CharacterStatisticsTitle", UICharacterStatisticsArea, new Color(1f, 0.65f, 0.15f), "AveriaSansLibre-Bold", 20, TextAnchor.MiddleCenter, new Vector2(0f, 290f), new Vector2(370f, 30f)).text = "CHARACTER STATISTICS";
			playtimeText = CreateTextObject("CharacterStatisticsPlaytime", UICharacterStatisticsArea, new Color(0.9f, 0.9f, 0.85f), "AveriaSansLibre-Bold", 14, TextAnchor.MiddleCenter, new Vector2(0f, 263f), new Vector2(370f, 25f));
			Text labels = CreateTextObject("CharacterStatisticsLabels", UICharacterStatisticsArea, new Color(0.9f, 0.9f, 0.85f), "AveriaSansLibre-Bold", 15, TextAnchor.UpperLeft, new Vector2(-57f, 12f), new Vector2(250f, 460f));

			labels.supportRichText = true;
			labels.lineSpacing = 1.05f;
			labels.text =
				"<color=#FFA626>COMBAT & SURVIVAL</color>\n" +
				"Foes Dispatched\n" +
				"Bosses Demoted\n" +
				"Valhalla Rejections\n" +
				"Longest Survival\n" +
				"Arrows Liberated\n" +
				"Walks of Shame\n\n" +
				"<color=#FFA626>CRAFT & HOMESTEAD</color>\n" +
				"Items Crafted\n" +
				"Items Upgraded\n" +
				"Pieces Built\n" +
				"Rocks and Ores Brutally Smashed\n" +
				"Trees Brutally Murdered\n" +
				"Creatures Tamed\n" +
				"Fish Acquired Legally\n" +
				"Time at Home\n\n" +
				"<color=#FFA626>TRAVEL & EXPLORATION</color>\n" +
				"Distance Traveled\n" +
				"Distance Sailed\n" +
				"Portals Used\n" +
				"Jumps";

			statisticsValues = CreateTextObject("CharacterStatisticsValues", UICharacterStatisticsArea, new Color(1f, 0.75f, 0.28f), "AveriaSansLibre-Bold", 15, TextAnchor.UpperRight, new Vector2(125f, 12f), new Vector2(100f, 460f));
			statisticsValues.lineSpacing = 1.05f;

			// Notable Random Facts
			CreateTextObject("CharacterStatisticsNotableFactHeading", UICharacterStatisticsArea, new Color(1f, 0.65f, 0.15f), "AveriaSansLibre-Bold", 15, TextAnchor.MiddleCenter, new Vector2(0f, -230f), new Vector2(370f, 25f)).text = "NOTABLE RANDOM FACT";
			notableFactTitle = CreateTextObject("CharacterStatisticsNotableFactTitle", UICharacterStatisticsArea, new Color(1f, 0.75f, 0.28f), "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(0f, -262f), new Vector2(370f, 25f));
			notableFactDescription = CreateTextObject("CharacterStatisticsNotableFactDescription", UICharacterStatisticsArea, new Color(0.9f, 0.9f, 0.85f), "AveriaSansLibre-Bold", 13, TextAnchor.MiddleCenter, new Vector2(0f, -292f), new Vector2(350f, 40f));
		}

		private static void CreateFrame(GameObject parent)
		{
			Color frameColor = new Color(0.72f, 0.40f, 0.10f, 0.85f);
			float thickness = 2f;

			CreateFrameEdge("FrameTop", parent, frameColor, new Vector2(0f, 324f), new Vector2(390f, thickness));
			CreateFrameEdge("FrameBottom", parent, frameColor, new Vector2(0f, -324f), new Vector2(390f, thickness));
			CreateFrameEdge("FrameLeft", parent, frameColor, new Vector2(-194f, 0f), new Vector2(thickness, 650f));
			CreateFrameEdge("FrameRight", parent, frameColor, new Vector2(194f, 0f), new Vector2(thickness, 650f));
		}

		private static void CreateFrameEdge(string name, GameObject parent, Color color, Vector2 position, Vector2 size)
		{
			GameObject edge = new GameObject(name);
			edge.layer = 5;
			edge.transform.SetParent(parent.transform, false);

			RectTransform rectTransform = edge.AddComponent<RectTransform>();
			rectTransform.anchoredPosition = position;
			rectTransform.sizeDelta = size;
			rectTransform.localScale = Vector3.one;

			Image image = edge.AddComponent<Image>();
			image.color = color;
			image.raycastTarget = false;
		}

		private static void UpdateStatistics(FejdStartup startup)
		{
			if (statisticsValues == null) return;

			PlayerProfile profile = GetSelectedProfile(startup);

			if (profile == null)
			{
				UICharacterStatisticsArea.SetActive(false);
				return;
			}

			UICharacterStatisticsArea.SetActive(true);

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

			playtimeText.text = $"Playtime: {FormatPlaytime(profile)}";

			statisticsValues.text =
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

			UpdateNotableFact(startup, profile);
		}

		private static GameObject GetCharacterSelectScreen(FejdStartup startup)
		{
			if (characterSelectScreenField == null)
			{
				log.Warn("Could not find m_characterSelectScreen in FejdStartup.");
				return null;
			}

			object characterSelectScreen = characterSelectScreenField.GetValue(startup);

			if (characterSelectScreen is GameObject gameObject)
				return gameObject;

			if (characterSelectScreen is Component component)
				return component.gameObject;

			log.Warn("m_characterSelectScreen was not a GameObject or Component.");
			return null;
		}

		private static PlayerProfile GetSelectedProfile(FejdStartup startup)
		{
			if (profilesField == null || profileIndexField == null)
			{
				log.Warn("Could not find character profile fields in FejdStartup.");
				return null;
			}

			List<PlayerProfile> profiles = profilesField.GetValue(startup) as List<PlayerProfile>;

			if (profiles == null || profiles.Count == 0) return null;

			int profileIndex = (int)profileIndexField.GetValue(startup);

			if (profileIndex < 0 || profileIndex >= profiles.Count) return null;

			return profiles[profileIndex];
		}

		private static float GetStat(PlayerProfile profile, PlayerStatType statType)
		{
			if (profile.m_playerStats == null || profile.m_playerStats.Length == 0) return 0f;
			if (profile.m_playerStats[0] == null || profile.m_playerStats[0].m_stats == null) return 0f;

			return profile.m_playerStats[0].m_stats.TryGetValue(statType, out float value) ? value : 0f;
		}

		private static string FormatPlaytime(PlayerProfile profile)
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

		private static void UpdateNotableFact(FejdStartup startup, PlayerProfile profile)
		{
			int profileIndex = (int)profileIndexField.GetValue(startup);

			if (profileIndex != lastFactProfileIndex)
			{
				lastFactProfileIndex = profileIndex;
				currentNotableFact = GetRandomNotableFact(startup, profile);
			}

			notableFactTitle.text = currentNotableFact.Title;
			notableFactDescription.text = currentNotableFact.Description;
		}

		private static NotableFact GetRandomNotableFact(FejdStartup startup, PlayerProfile profile)
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

			int bossLastHits = Mathf.RoundToInt(GetStat(profile, PlayerStatType.BossLastHits));
			if (bossLastHits >= 5)
				facts.Add(new NotableFact("The Finishing Touch", $"Dealt the final blow to {bossLastHits:N0} bosses."));

			int fishLost = Mathf.RoundToInt(GetStat(profile, PlayerStatType.FishLost));
			if (fishLost >= 10)
				facts.Add(new NotableFact("The Ones That Got Away", $"Lost {fishLost:N0} hooked fish."));

			int ravenHits = Mathf.RoundToInt(GetStat(profile, PlayerStatType.RavenHits));
			if (ravenHits >= 5)
				facts.Add(new NotableFact("Odin's HR Has Been Notified", $"Hit a raven {ravenHits:N0} {Pluralize(ravenHits, "time", "times")}."));

			int skeletonSummons = Mathf.RoundToInt(GetStat(profile, PlayerStatType.SkeletonSummons));
			if (skeletonSummons >= 250)
				facts.Add(new NotableFact("Necromancy Is a Hobby", $"Summoned {skeletonSummons:N0} skeletons."));

			int otherTombstones = Mathf.RoundToInt(GetStat(profile, PlayerStatType.TombstonesOpenedOther));
			if (otherTombstones >= 5)
				facts.Add(new NotableFact("Retrieval Specialist", $"Recovered {otherTombstones:N0} other players' tombstones."));

			int doorsOpened = Mathf.RoundToInt(GetStat(profile, PlayerStatType.DoorsOpened));
			int doorsClosed = Mathf.RoundToInt(GetStat(profile, PlayerStatType.DoorsClosed));
			int doorsLeftOpen = doorsOpened - doorsClosed;

			if (doorsOpened > 0 && doorsLeftOpen >= 100 && doorsLeftOpen >= doorsOpened * 0.1f)
				facts.Add(new NotableFact("Born in a Barn", $"Opened {doorsLeftOpen:N0} more doors than you closed."));

			int leviathansSunk = Mathf.RoundToInt(GetStat(profile, PlayerStatType.LeviathanSink));
			if (leviathansSunk >= 5)
				facts.Add(new NotableFact("Thar She Blows", $"Sank {leviathansSunk:N0} {Pluralize(leviathansSunk, "leviathan", "leviathans")}."));

			int treasuresFound =
				Mathf.RoundToInt(GetStat(profile, PlayerStatType.TreasureBuriedFound)) +
				//Mathf.RoundToInt(GetStat(profile, PlayerStatType.TreasureDungeonFound)) +
				Mathf.RoundToInt(GetStat(profile, PlayerStatType.TreasureLocationFound));

			if (treasuresFound >= 10)
				facts.Add(new NotableFact("Treasure Hunter", $"Discovered {treasuresFound:N0} hidden treasures."));

			float distanceSailedAtHelm = GetStat(profile, PlayerStatType.DistanceSailHelm) / 1000f;
			if (distanceSailedAtHelm >= 100f)
				facts.Add(new NotableFact("Captain", $"Sailed {distanceSailedAtHelm:N1} km at the helm."));

			int builtPieces = Mathf.RoundToInt(GetStat(profile, PlayerStatType.BuiltPieces));
			int removedPieces = Mathf.RoundToInt(GetStat(profile, PlayerStatType.BuildPiecesRemoved));

			if (builtPieces >= 100 && removedPieces >= builtPieces * 0.5f)
				facts.Add(new NotableFact("Measure Twice, Build Once?", $"Removed {removedPieces:N0} pieces after building {builtPieces:N0}."));

			Skills skills = GetSelectedCharacterSkills(startup);

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
			int count = Mathf.RoundToInt(GetStat(profile, statType));

			if (count < threshold) return;

			facts.Add(new NotableFact(title, $"{description} {count:N0} {Pluralize(count, "time", "times")}."));
		}

		private static string Pluralize(int count, string singular, string plural)
		{
			return count == 1 ? singular : plural;
		}

		private static Skills GetSelectedCharacterSkills(FejdStartup startup)
		{
			if (playerInstanceField == null)
			{
				log.Warn("Could not find m_playerInstance in FejdStartup.");
				return null;
			}

			object playerInstance = playerInstanceField.GetValue(startup);
			Player player = null;

			if (playerInstance is Player playerComponent)
			{
				player = playerComponent;
			}
			else if (playerInstance is GameObject gameObject)
			{
				player = gameObject.GetComponent<Player>();
			}
			else if (playerInstance is Component component)
			{
				player = component.GetComponent<Player>();
			}

			return player?.GetSkills();
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
	}
}