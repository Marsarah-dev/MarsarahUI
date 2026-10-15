using HarmonyLib;
using MarsarahUI.Managers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIBoatSpeed : UIController
	{
		private static readonly LogManager log = new LogManager("UI Boat Speed", LogManager.LogLevel.Warning);

		internal const float BoatAreaWidth = 80f;

		private static float boatSpeed;

		internal static GameObject UIBoatArea;
		private static GameObject UIBoatSpeedArea;
		private static Text UIBoatText;
		private static Image UIBoatIcon;
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

				if (UIBoatSpeedArea != null)
				{
					UIBoatSpeedArea.SetActive(showSpeed);

					bool compactLayout = ConfigManager.EffectiveStatusEffectsUnderMinimap && !Game.m_noMap;
					RectTransform speedTransform = UIBoatSpeedArea.transform as RectTransform;

					if (speedTransform != null)
					{
						float yOffset = !compactLayout && !showHealth ? -27f : 0f;
						speedTransform.anchoredPosition = new Vector2(0f, yOffset);
					}
				}

				if (showSpeed)
				{
					if (UIBoatText != null)
					{
						UIBoatText.color = UIStyleManager.Current.WeightFillColorMode == InfoRailColorMode.WeightGradient ? GetColorFromSpeed(boatSpeed) : UIStyleManager.Current.ValueTextColor;
						UIBoatText.text = boatSpeed > 0f ? boatSpeed.ToString("0.00") : $"R {Math.Abs(boatSpeed):0.00}";
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
			string uiTextFontName = "AveriaSansLibre-Bold";
			Vector2 uiBoatAreaSize = new Vector2(BoatAreaWidth, 30f);

			float xOffset = Game.m_noMap ? -145f : -283f;
			float yOffset = Game.m_noMap ? -225f : -175f;

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

			UIBoatText = CreateTextObject("BoatText", UIBoatSpeedArea, Color.white, uiTextFontName, uiTextFontSize, TextAnchor.MiddleRight, new Vector2(-4f, 0f), uiBoatAreaSize);
			UIBoatIcon = CreateUIImageObject("BoatIcon", UIBoatSpeedArea, new Vector2(-23f, 0f), new Vector2(24f, 24f));
			UIBoatIcon.preserveAspect = true;
			UIBoatIcon.raycastTarget = false;
			UIBoatArea.SetActive(false);

			UIStyleManager.StyleChanged -= ApplyBoatIconStyle;
			UIStyleManager.StyleChanged += ApplyBoatIconStyle;

			ApplyBoatIconStyle();

			log.Info("Created shared sailing HUD area.");
		}

		private static void UpdateBoatSpeedPosition()
		{
			if (boatAreaTransform == null) return;

			if (ConfigManager.EffectiveStatusEffectsUnderMinimap && !Game.m_noMap)
			{
				boatAreaTransform.anchoredPosition = new Vector2(-290f, -25f);
			}
			else
			{
				float xOffset = Game.m_noMap ? -145f : -283f;
				float yOffset = Game.m_noMap ? -225f : -195f;

				boatAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset);
			}
		}

		private static void ApplyBoatIconStyle()
		{
			if (UIBoatIcon == null) return;

			string iconName = UIStyleManager.Current.BoatIcon;
			UIBoatIcon.sprite = IconManager.LoadHudIcon(iconName);
			UIBoatIcon.preserveAspect = true;

			if (UIBoatIcon.sprite == null)
			{
				log.Warn($"Could not load boat icon '{iconName}'.");
			}
		}
	}
}
