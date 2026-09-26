using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static MarsarahUI.Managers.ConfigManager;

namespace MarsarahUI.Patches.UI
{
	internal class UITimeAndDay : UIController
	{
		private static readonly LogManager log = new LogManager("UI Time And Day", LogManager.LogLevel.Warning);

		public static string TimeString;
		public static int CurrentDay;

		private static Text UITimeText;
		private static Text UIDayText;

		private static readonly MethodInfo getCurrentDayMethod = AccessTools.Method(typeof(EnvMan), "GetCurrentDay");

		[HarmonyPatch(typeof(EnvMan), "Update")]
		private static class TimeAndDay_EnvManPatch
		{
			private static void Prefix(EnvMan __instance, ref float ___m_smoothDayFraction)
			{
				bool dayEnabled = ConfigManager.EffectiveShowCurrentDay;
				bool timeEnabled = ConfigManager.EffectiveTimeChoice != TimeMode.Off;

				if (!dayEnabled && !timeEnabled) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (dayEnabled)
				{
					if (getCurrentDayMethod != null)
					{
						CurrentDay = (int)getCurrentDayMethod.Invoke(__instance, null);
					}
				}

				if (timeEnabled)
				{
					if (ConfigManager.EffectiveTimeChoice == TimeMode.DayPhases)
					{
						TimeString = GetStringFromFraction(___m_smoothDayFraction);
					}
					else
					{
						int hours = (int)(___m_smoothDayFraction * 24f);
						int minutes = (int)((___m_smoothDayFraction * 24f - hours) * 60f);
						string hoursString = hours < 10 ? "0" + hours : hours.ToString();
						string minutesString = minutes < 10 ? "0" + minutes : minutes.ToString();

						TimeString = "Time " + hoursString + ":" + minutesString;
					}
				}
			}

			private static string GetStringFromFraction(float dayFraction)
			{
				if (dayFraction < 0.20f) return "Night";
				if (dayFraction < 0.25f) return "Dawn";
				if (dayFraction < 0.33f) return "Morning";
				if (dayFraction < 0.50f) return "Day";
				if (dayFraction < 0.66f) return "Afternoon";
				if (dayFraction < 0.75f) return "Evening";
				if (dayFraction < 0.80f) return "Dusk";

				return "Night";
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class TimeAndDay_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (ConfigManager.EffectiveTimeChoice != TimeMode.Off || ConfigManager.EffectiveShowCurrentDay)
				{
					CreateUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class TimeAndDay_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				bool timeEnabled = ConfigManager.EffectiveTimeChoice != TimeMode.Off;
				bool dayEnabled = ConfigManager.EffectiveShowCurrentDay;

				if (timeEnabled || dayEnabled)
				{
					CreateUI(__instance);

					bool showTimeAndDayUI = Game.m_noMap ? ShowUI : ShowUI && Minimap.instance != null && Minimap.instance.m_mapSmall != null && Minimap.instance.m_mapSmall.activeInHierarchy;

					UITimeText.enabled = showTimeAndDayUI && timeEnabled;
					UIDayText.enabled = showTimeAndDayUI && dayEnabled;

					if (showTimeAndDayUI)
					{
						UITimeText.color = GetColorFromString(TimeString);
						UIDayText.color = Color.white;

						UITimeText.text = TimeString;
						UIDayText.text = "Day " + CurrentDay;
					}
				}
				else
				{
					if (UITimeText != null)
						UITimeText.enabled = false;

					if (UIDayText != null)
						UIDayText.enabled = false;
				}
			}

			private static Color GetColorFromString(string word)
			{
				if (word == "Night") return Color.red;
				if (word == "Dawn") return new Color(1f, 0.549019f, 0f);
				if (word == "Dusk") return new Color(1f, 0.549019f, 0f);
				if (word == "Morning") return Color.yellow;
				if (word == "Evening") return Color.yellow;
				if (word == "Day") return Color.green;
				if (word == "Afternoon") return Color.green;

				return Color.white;
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UITimeText != null && UIDayText != null) return;

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			Vector2 UITimeAreaSize = new Vector2(200f, 30f);

			GameObject UITimeArea = new GameObject("TimeArea");
			UITimeArea.layer = 5;
			UITimeArea.transform.SetParent(hud.m_rootObject.transform);

			RectTransform timeAreaTransform = UITimeArea.AddComponent<RectTransform>();
			timeAreaTransform.anchorMin = new Vector2(1f, 1f);
			timeAreaTransform.anchorMax = new Vector2(1f, 1f);
			timeAreaTransform.anchoredPosition = new Vector2(-140f, -25f);
			timeAreaTransform.sizeDelta = UITimeAreaSize;
			UITimeArea.transform.localScale = Vector3.one;

			UITimeAreaSize.x /= 2f;

			UITimeText = CreateTextObject("TimeText", UITimeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(40f, 0f), UITimeAreaSize);
			UIDayText = CreateTextObject("DayText", UITimeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleLeft, new Vector2(-40f, 0f), UITimeAreaSize);

			UpdatePositions();
		}

		public static void UpdatePositions()
		{
			if (UITimeText == null || UIDayText == null) return;

			bool timeEnabled = ConfigManager.EffectiveTimeChoice != TimeMode.Off;
			bool dayEnabled = ConfigManager.EffectiveShowCurrentDay;

			RectTransform timeTransform = UITimeText.rectTransform;
			RectTransform dayTransform = UIDayText.rectTransform;

			Vector2 timePosition = timeTransform.anchoredPosition;
			Vector2 dayPosition = dayTransform.anchoredPosition;

			if (timeEnabled && dayEnabled)
			{
				timePosition.x = 40f;
				dayPosition.x = -40f;

				UITimeText.alignment = TextAnchor.MiddleRight;
				UIDayText.alignment = TextAnchor.MiddleLeft;
			}
			else
			{
				timePosition.x = 0f;
				dayPosition.x = 0f;

				UITimeText.alignment = TextAnchor.MiddleCenter;
				UIDayText.alignment = TextAnchor.MiddleCenter;
			}

			timeTransform.anchoredPosition = timePosition;
			dayTransform.anchoredPosition = dayPosition;
		}
	}
}