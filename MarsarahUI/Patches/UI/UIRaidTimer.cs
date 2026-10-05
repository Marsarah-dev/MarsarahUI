using HarmonyLib;
using MarsarahUI.Managers;
using TMPro;
using UnityEngine;

namespace MarsarahUI.Patches.UI
{
	internal class UIRaidTimer : UIController
	{
		private static readonly LogManager log = new LogManager("UI Raid Timer", LogManager.LogLevel.Info);

		private const float FontSize = 18f;
		private static readonly Vector2 TimerOffset = new Vector2(0f, -30f);

		private static TextMeshProUGUI raidTimerText;
		private static bool uiWarningLogged;

		[HarmonyPatch(typeof(Hud), "UpdateEvent")]
		private static class RaidTimer_UpdateEventPatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null || __instance.m_eventBar == null) return;

				if (!ConfigManager.EffectiveShowRaidTimer || !ShowUI)
				{
					SetTimerVisible(false);
					return;
				}

				if (!__instance.m_eventBar.activeInHierarchy)
				{
					SetTimerVisible(false);
					return;
				}

				RandEventSystem eventSystem = RandEventSystem.instance;

				if (eventSystem == null)
				{
					SetTimerVisible(false);
					return;
				}

				RandomEvent raid = eventSystem.GetCurrentRandomEvent();

				if (raid == null)
				{
					SetTimerVisible(false);
					return;
				}

				EnsureTimerExists(__instance);

				if (raidTimerText == null) return;

				float remainingTime = Mathf.Max(0f, raid.m_duration - raid.GetTime());
				int remainingSeconds = Mathf.CeilToInt(remainingTime);
				int minutes = remainingSeconds / 60;
				int seconds = remainingSeconds % 60;

				raidTimerText.text = $"{minutes:00}:{seconds:00}";
				raidTimerText.enabled = true;
			}
		}

		private static void EnsureTimerExists(Hud hud)
		{
			if (raidTimerText != null && raidTimerText.transform.parent == hud.m_eventBar.transform) return;

			if (raidTimerText != null)
			{
				Object.Destroy(raidTimerText.gameObject);
				raidTimerText = null;
			}

			if (hud.m_eventName == null)
			{
				if (!uiWarningLogged)
				{
					log.Warn("Could not create raid timer because the vanilla event title was missing.");
					uiWarningLogged = true;
				}

				return;
			}

			GameObject timerObject = new GameObject("MarsarahUI Raid Timer");
			timerObject.layer = 5;
			timerObject.transform.SetParent(hud.m_eventBar.transform, false);

			RectTransform titleTransform = hud.m_eventName.rectTransform;

			RectTransform timerTransform = timerObject.AddComponent<RectTransform>();
			timerTransform.anchorMin = titleTransform.anchorMin;
			timerTransform.anchorMax = titleTransform.anchorMax;
			timerTransform.pivot = titleTransform.pivot;
			timerTransform.anchoredPosition = titleTransform.anchoredPosition + TimerOffset;
			timerTransform.sizeDelta = titleTransform.sizeDelta;
			timerTransform.localScale = Vector3.one;

			raidTimerText = timerObject.AddComponent<TextMeshProUGUI>();
			raidTimerText.font = hud.m_eventName.font;
			raidTimerText.fontSharedMaterial = hud.m_eventName.fontSharedMaterial;
			raidTimerText.fontSize = FontSize;
			raidTimerText.fontStyle = hud.m_eventName.fontStyle;
			raidTimerText.color = hud.m_eventName.color;
			raidTimerText.alignment = TextAlignmentOptions.Center;
			raidTimerText.textWrappingMode = TextWrappingModes.NoWrap;
			raidTimerText.overflowMode = TextOverflowModes.Overflow;
			raidTimerText.raycastTarget = false;
			raidTimerText.text = string.Empty;
			raidTimerText.enabled = false;

			uiWarningLogged = false;

			log.Info("Created raid timer below the vanilla raid title.");
		}

		private static void SetTimerVisible(bool visible)
		{
			if (raidTimerText != null)
			{
				raidTimerText.enabled = visible;
			}
		}
	}
}