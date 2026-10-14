using HarmonyLib;
using MarsarahUI.Managers;
using System;
using TMPro;
using UnityEngine;

namespace MarsarahUI.Patches.UI
{
	internal class UIBoatSpeed : UIController
	{
		private static readonly LogManager log = new LogManager("UI Boat Speed", LogManager.LogLevel.Warning);

		internal const float BoatAreaWidth = 80f;

		private static float boatSpeed;

		internal static GameObject UIBoatArea;
		private static GameObject UIBoatSpeedArea;
		private static TextMeshProUGUI UIBoatTextTMP;
		private static TextMeshProUGUI UIBoatEmojiTMP;
		private static RectTransform boatAreaTransform;

		[HarmonyPatch(typeof(Ship), "GetSpeed")]
		private static class ShowBoatSpeed_Patch
		{
			private static void Prefix(Ship __instance, ref Rigidbody ___m_body)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (!ConfigManager.EffectiveShowBoatSpeed) return;
				if (__instance == null || ___m_body == null || Player.m_localPlayer == null) return;
				if (Player.m_localPlayer.GetControlledShip() != __instance) return;

				boatSpeed = Vector3.Dot(___m_body.linearVelocity, __instance.transform.forward);
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class BoatSpeed_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (ConfigManager.EffectiveShowBoatSpeed || ConfigManager.EffectiveShowBoatHealth)
				{
					CreateUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class BoatSpeed_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				bool showSpeed = ConfigManager.EffectiveShowBoatSpeed;
				bool showHealth = ConfigManager.EffectiveShowBoatHealth;

				if (!showSpeed && !showHealth)
				{
					if (UIBoatArea != null) UIBoatArea.SetActive(false);
					return;
				}

				CreateUI(__instance);
				UpdateBoatSpeedPosition();

				Ship ship = Player.m_localPlayer != null ? Player.m_localPlayer.GetControlledShip() : null;
				bool shouldBeVisible = ship != null && ShowUI;

				if (UIBoatArea != null) UIBoatArea.SetActive(shouldBeVisible);
				if (!shouldBeVisible) return;

				if (UIBoatSpeedArea != null) UIBoatSpeedArea.SetActive(showSpeed);

				if (showSpeed)
				{
					if (UIBoatTextTMP != null)
					{
						UIBoatTextTMP.color = GetColorFromSpeed(boatSpeed);
						UIBoatTextTMP.text = boatSpeed > 0f
							? boatSpeed.ToString("0.00")
							: $"R {Math.Abs(boatSpeed):0.00}";
					}

					if (UIBoatEmojiTMP != null)
					{
						UIBoatEmojiTMP.text = "⛵";
					}
				}

				UIBoatHealth.UpdateUI(UIBoatArea, ship, showHealth, showSpeed);
			}
		}

		private static Color GetColorFromSpeed(float speed)
		{
			if (speed < 2.5f) return Color.red;
			if (speed < 5f) return new Color(1f, 0.549019f, 0f);
			if (speed < 7.5f) return Color.yellow;

			return Color.green;
		}

		private static void CreateUI(Hud hud)
		{
			if (UIBoatArea != null) return;

			int uiTextFontSize = 16;
			string uiTextFontName = "Valheim-AveriaSansLibre";
			string uiEmojiFontName = "NotoEmoji-Regular SDF";
			Vector2 uiBoatAreaSize = new Vector2(BoatAreaWidth, 30f);

			float xOffset = Game.m_noMap ? -145f : -283f;
			float yOffset = -225f;

			UIBoatArea = new GameObject("BoatArea");
			UIBoatArea.layer = 5;
			UIBoatArea.transform.SetParent(hud.m_rootObject.transform, false);

			boatAreaTransform = UIBoatArea.AddComponent<RectTransform>();
			boatAreaTransform.anchorMin = new Vector2(1f, 1f);
			boatAreaTransform.anchorMax = new Vector2(1f, 1f);
			boatAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset);
			boatAreaTransform.sizeDelta = uiBoatAreaSize;
			boatAreaTransform.localScale = Vector3.one;

			UIBoatSpeedArea = new GameObject("BoatSpeedArea");
			UIBoatSpeedArea.layer = 5;
			UIBoatSpeedArea.transform.SetParent(UIBoatArea.transform, false);

			RectTransform speedAreaTransform = UIBoatSpeedArea.AddComponent<RectTransform>();
			speedAreaTransform.anchorMin = new Vector2(0.5f, 0.5f);
			speedAreaTransform.anchorMax = new Vector2(0.5f, 0.5f);
			speedAreaTransform.anchoredPosition = Vector2.zero;
			speedAreaTransform.sizeDelta = uiBoatAreaSize;
			speedAreaTransform.localScale = Vector3.one;

			UIBoatTextTMP = CreateTMPTextObject("BoatTextTMP", UIBoatSpeedArea, Color.white, uiTextFontName, uiTextFontSize, TextAlignmentOptions.MidlineRight, new Vector2(-4f, 0f), uiBoatAreaSize, log);
			UIBoatEmojiTMP = CreateTMPTextObject("BoatEmojiTMP", UIBoatSpeedArea, Color.white, uiEmojiFontName, uiTextFontSize + 4, TextAlignmentOptions.MidlineLeft, new Vector2(4f, 0f), uiBoatAreaSize, log);

			UIBoatTextTMP.raycastTarget = false;
			UIBoatEmojiTMP.raycastTarget = false;
			UIBoatArea.SetActive(false);

			log.Info("Created shared sailing HUD area.");
		}

		private static void UpdateBoatSpeedPosition()
		{
			if (boatAreaTransform == null) return;

			if (ConfigManager.EffectiveStatusEffectsUnderMinimap && !Game.m_noMap)
			{
				boatAreaTransform.anchoredPosition = new Vector2(-360f, -25f);
			}
			else
			{
				float xOffset = Game.m_noMap ? -145f : -283f;
				boatAreaTransform.anchoredPosition = new Vector2(xOffset, -225f);
			}
		}
	}
}
