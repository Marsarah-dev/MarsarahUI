using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UISummonCounter : UIController
	{
		private static readonly LogManager log = new LogManager("UI Summon Counter", LogManager.LogLevel.Warning);

		private static int numSummons;

		private static GameObject UISummonsArea;
		private static Text UISummonsText;
		private static TextMeshProUGUI UISummonsTextTMP;

		[HarmonyPatch(typeof(Player), "Update")]
		private static class SummonCounters_PlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_localPlayer == null) return;
				if (!ConfigManager.EffectiveShowSummonCounter || !ShowUI) return;

				List<Character> allCharacters = Character.GetAllCharacters();
				int numSummonedSkeletons = 0;

				foreach (Character character in allCharacters)
				{
					if (!character.IsTamed()) continue;

					MonsterAI monsterAI = character.GetComponent<MonsterAI>();
					if (monsterAI == null) continue;

					GameObject followTarget = monsterAI.GetFollowTarget();
					if (followTarget == null) continue;

					Player targetPlayer = followTarget.GetComponent<Player>();
					if (targetPlayer == null) continue;

					if (targetPlayer.GetPlayerName() == ___m_localPlayer.GetPlayerName() && character.name.Contains("Skeleton_Friendly"))
					{
						numSummonedSkeletons++;
					}
				}

				numSummons = numSummonedSkeletons;
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class SummonCounter_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (ConfigManager.EffectiveShowSummonCounter)
				{
					CreateUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class SummonCounter_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (ConfigManager.EffectiveShowSummonCounter)
				{
					CreateUI(__instance);

					bool showSummonCounter = ShowUI && numSummons != 0;
					UISummonsArea?.SetActive(showSummonCounter);

					if (showSummonCounter)
					{
						Color color = GetColorFromNum(numSummons);

						UISummonsText.color = color;
						UISummonsTextTMP.color = color;

						UISummonsText.text = numSummons.ToString();
						UISummonsTextTMP.text = "💀";
					}
				}
				else
				{
					UISummonsArea?.SetActive(false);
				}
			}

			private static Color GetColorFromNum(int num)
			{
				if (num < 2) return new Color(1f, 0.549019f, 0f);
				if (num == 2) return Color.yellow;
				if (num >= 3) return Color.green;

				return Color.white;
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UISummonsArea != null && UISummonsText != null && UISummonsTextTMP != null) return;

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			string UIEmojiFontName = "NotoEmoji-Regular SDF";
			Vector2 UISummonsAreaSize = new Vector2(49f, 30f);

			UISummonsArea = new GameObject("SummonsArea");
			UISummonsArea.layer = 5;
			UISummonsArea.transform.SetParent(hud.m_healthPanel.transform);

			RectTransform summonsAreaTransform = UISummonsArea.AddComponent<RectTransform>();
			summonsAreaTransform.anchorMin = new Vector2(1f, 1f);
			summonsAreaTransform.anchorMax = new Vector2(1f, 1f);
			summonsAreaTransform.anchoredPosition = new Vector2(38f, -85f);
			summonsAreaTransform.sizeDelta = UISummonsAreaSize;
			UISummonsArea.transform.localScale = Vector3.one;

			Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(tempSprite => tempSprite.name == "InputFieldBackground");

			Image summonsAreaBackground = UISummonsArea.AddComponent<Image>();
			summonsAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
			summonsAreaBackground.sprite = sprite;
			summonsAreaBackground.type = Image.Type.Sliced;

			UISummonsText = CreateTextObject("SummonsText", UISummonsArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-5f, 0f), UISummonsAreaSize);
			UISummonsTextTMP = CreateTMPTextObject("SummonsEmojiTMP", UISummonsArea, Color.white, UIEmojiFontName, UITextFontSize, TextAlignmentOptions.MidlineLeft, new Vector2(5f, 0f), UISummonsAreaSize, log);
		}
	}
}