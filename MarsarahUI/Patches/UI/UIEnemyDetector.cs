using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIEnemyDetector : UIController
	{
		private static readonly LogManager log = new LogManager("UI Enemy Detector", LogManager.LogLevel.Warning);

		private static int numEnemies;
		private static int numEnemiesPassive;

		internal static GameObject UIEnemyArea;
		private static Text UIEnemyText;
		private static TextMeshProUGUI UIEnemyEmojiTMP;

		internal static GameObject UIFriendlyArea;
		private static Text UIFriendlyText;
		private static TextMeshProUGUI UIFriendlyEmojiTMP;

		[HarmonyPatch(typeof(Player), "Update")]
		private static class EnemyDetector_PlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_localPlayer == null) return;
				if (!ConfigManager.EffectiveShowEnemyDetector) return;

				int ignoredCharacters = 0;
				int passiveDverger = 0;

				List<Character> characters = new List<Character>();
				Character.GetCharactersInRange(___m_localPlayer.transform.position, 30f, characters);

				foreach (Character character in characters)
				{
					if (character.GetFaction() == Character.Faction.Dverger &&
						character.GetBaseAI() != null &&
						!character.GetBaseAI().IsAggravated())
					{
						passiveDverger++;
					}

					if (character.m_name == "Human" ||
						character.m_name == "$enemy_deer" ||
						character.m_name == "$enemy_hare" ||
						character.m_name == "$enemy_summonedroot" ||
						character.IsTamed())
					{
						ignoredCharacters++;
					}
				}

				numEnemies = characters.Count - ignoredCharacters - passiveDverger;
				numEnemiesPassive = passiveDverger;
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class EnemyDetector_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (ConfigManager.EffectiveShowEnemyDetector)
				{
					CreateUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class EnemyDetector_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (!ConfigManager.EffectiveShowEnemyDetector)
				{
					SetUIActive(false);
					return;
				}

				CreateUI(__instance);

				bool shouldBeVisible = ShowUI;

				UIEnemyArea?.SetActive(shouldBeVisible);
				UIFriendlyArea?.SetActive(shouldBeVisible && numEnemiesPassive != 0);

				if (!shouldBeVisible) return;

				Color enemyColor = GetColorFromNum(numEnemies);

				if (UIEnemyText != null)
				{
					UIEnemyText.color = enemyColor;
					UIEnemyText.text = numEnemies.ToString();
				}

				if (UIEnemyEmojiTMP != null)
				{
					UIEnemyEmojiTMP.color = enemyColor;

					if (numEnemies == 0)
					{
						UIEnemyEmojiTMP.text = "👁";
					}
					else if (numEnemies < 5)
					{
						UIEnemyEmojiTMP.text = "😈";
					}
					else if (numEnemies < 7)
					{
						UIEnemyEmojiTMP.text = "👿";
					}
					else
					{
						UIEnemyEmojiTMP.text = "☠";
					}
				}

				Color friendlyColor = GetColorFromNum(numEnemiesPassive);

				if (UIFriendlyText != null)
				{
					UIFriendlyText.color = friendlyColor;
					UIFriendlyText.text = numEnemiesPassive.ToString();
				}

				if (UIFriendlyEmojiTMP != null)
				{
					UIFriendlyEmojiTMP.color = friendlyColor;
					UIFriendlyEmojiTMP.text = "🧔";
				}
			}
		}

		private static void SetUIActive(bool active)
		{
			UIEnemyArea?.SetActive(active);
			UIFriendlyArea?.SetActive(active && numEnemiesPassive != 0);
		}

		private static Color GetColorFromNum(int num)
		{
			if (num < 3) return Color.green;
			if (num < 5) return Color.yellow;
			if (num < 7) return new Color(1f, 0.549019f, 0f);

			return Color.red;
		}

		private static void CreateUI(Hud hud)
		{
			if (UIEnemyArea != null && UIFriendlyArea != null) return;

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			string UIEmojiFontName = "NotoEmoji-Regular SDF";
			Vector2 UIEnemyAreaSize = new Vector2(50f, 30f);

			float xOffset = ConfigManager.EffectiveShowInventoryWeightAndSlots ? 122f : -65f;
			float yOffset = -230f;

			UIEnemyArea = new GameObject("EnemyArea");
			UIEnemyArea.layer = 5;
			UIEnemyArea.transform.SetParent(hud.m_healthPanel.transform);

			RectTransform enemyAreaTransform = UIEnemyArea.AddComponent<RectTransform>();
			enemyAreaTransform.anchorMin = new Vector2(1f, 1f);
			enemyAreaTransform.anchorMax = new Vector2(1f, 1f);
			enemyAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset);
			enemyAreaTransform.sizeDelta = UIEnemyAreaSize;
			UIEnemyArea.transform.localScale = Vector3.one;

			Sprite backgroundSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == "InputFieldBackground");

			Image enemyAreaBackground = UIEnemyArea.AddComponent<Image>();
			enemyAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
			enemyAreaBackground.sprite = backgroundSprite;
			enemyAreaBackground.type = Image.Type.Sliced;

			UIEnemyText = CreateTextObject("EnemyText", UIEnemyArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-4f, 0f), UIEnemyAreaSize);
			UIEnemyEmojiTMP = CreateTMPTextObject("EnemyEmojiTMP", UIEnemyArea, Color.green, UIEmojiFontName, UITextFontSize + 4, TextAlignmentOptions.MidlineLeft, new Vector2(4f, 0f), UIEnemyAreaSize, log);

			UIFriendlyArea = new GameObject("FriendlyArea");
			UIFriendlyArea.layer = 5;
			UIFriendlyArea.transform.SetParent(hud.m_healthPanel.transform);

			xOffset += 54f;

			RectTransform friendlyAreaTransform = UIFriendlyArea.AddComponent<RectTransform>();
			friendlyAreaTransform.anchorMin = new Vector2(1f, 1f);
			friendlyAreaTransform.anchorMax = new Vector2(1f, 1f);
			friendlyAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset);
			friendlyAreaTransform.sizeDelta = UIEnemyAreaSize;
			UIFriendlyArea.transform.localScale = Vector3.one;

			Image friendlyAreaBackground = UIFriendlyArea.AddComponent<Image>();
			friendlyAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
			friendlyAreaBackground.sprite = backgroundSprite;
			friendlyAreaBackground.type = Image.Type.Sliced;

			UIFriendlyText = CreateTextObject("FriendlyText", UIFriendlyArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-4f, 0f), UIEnemyAreaSize);
			UIFriendlyEmojiTMP = CreateTMPTextObject("FriendlyEmojiTMP", UIFriendlyArea, Color.green, UIEmojiFontName, UITextFontSize + 4, TextAlignmentOptions.MidlineLeft, new Vector2(4f, 0f), UIEnemyAreaSize, log);
		}
	}
}