using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using MarsarahUI.Patches.Logic;

namespace MarsarahUI.Patches.UI
{
	internal class UICharacterStatistics : UIController
	{
		private static readonly LogManager log = new LogManager("UI Character Statistics", LogManager.LogLevel.Warning);

		private static readonly FieldInfo profilesField = AccessTools.Field(typeof(FejdStartup), "m_profiles");
		private static readonly FieldInfo profileIndexField = AccessTools.Field(typeof(FejdStartup), "m_profileIndex");
		private static readonly FieldInfo characterSelectScreenField = AccessTools.Field(typeof(FejdStartup), "m_characterSelectScreen");
		private static readonly FieldInfo playerInstanceField = AccessTools.Field(typeof(FejdStartup), "m_playerInstance");
		
		private static GameObject UICharacterStatisticsArea;
		private static Text statisticsValues;
		private static Text playtimeText;
		private static Text notableFactTitle;
		private static Text notableFactDescription;

		private static int lastFactProfileIndex = -1;
		private static CharacterStatisticsLogic.NotableFact currentNotableFact;

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

			playtimeText.text = $"Playtime: {CharacterStatisticsLogic.FormatPlaytime(profile)}";
			statisticsValues.text = CharacterStatisticsLogic.BuildStatisticsText(profile);

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

		private static void UpdateNotableFact(FejdStartup startup, PlayerProfile profile)
		{
			int profileIndex = (int)profileIndexField.GetValue(startup);

			if (profileIndex != lastFactProfileIndex)
			{
				lastFactProfileIndex = profileIndex;

				Skills skills = GetSelectedCharacterSkills(startup);
				currentNotableFact = CharacterStatisticsLogic.GetRandomNotableFact(profile, skills);
			}

			notableFactTitle.text = currentNotableFact.Title;
			notableFactDescription.text = currentNotableFact.Description;
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
	}
}