using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIBoatSpeed : UIController
	{
		private static readonly LogManager log = new LogManager("UI Boat Speed", LogManager.LogLevel.Warning);

		private static float boatSpeed;
		private static bool showBoatSpeedUI;

		internal static GameObject UIBoatArea;
		private static Text UIBoatText;
		private static TextMeshProUGUI UIBoatEmojiTMP;

		[HarmonyPatch(typeof(Ship), "GetSpeed")]
		private static class ShowBoatSpeed_Patch
		{
			private static void Prefix(Ship __instance, ref Rigidbody ___m_body)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (!ConfigManager.EffectiveShowBoatSpeed) return;

				if (__instance && __instance.HasPlayerOnboard() && ___m_body != null)
				{
					boatSpeed = Vector3.Dot(___m_body.linearVelocity, __instance.transform.forward);
				}

				showBoatSpeedUI = Traverse.Create(__instance).Method("HaveControllingPlayer", Array.Empty<object>()).GetValue<bool>();
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class BoatSpeed_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (ConfigManager.EffectiveShowBoatSpeed)
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

				if (!ConfigManager.EffectiveShowBoatSpeed)
				{
					UIBoatArea?.SetActive(false);
					return;
				}

				CreateUI(__instance);

				bool shouldBeVisible = showBoatSpeedUI && ShowUI;

				UIBoatArea?.SetActive(shouldBeVisible);

				if (!shouldBeVisible) return;

				if (UIBoatText != null)
				{
					UIBoatText.color = GetColorFromSpeed(boatSpeed);
					UIBoatText.text = boatSpeed > 0f
						? boatSpeed.ToString("0.00")
						: $"R {Math.Abs(boatSpeed):0.00}";
				}

				if (UIBoatEmojiTMP != null)
				{
					UIBoatEmojiTMP.text = "⛵";
				}
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

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			string UIEmojiFontName = "NotoEmoji-Regular SDF";
			Vector2 UIBoatAreaSize = new Vector2(80f, 30f);

			float xOffset = Game.m_noMap ? -145f : -283f;
			float yOffset = -225f;

			bool minimalStatusEffectsLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetName().Name == "MinimalStatusEffects");

			if (minimalStatusEffectsLoaded)
			{
				xOffset = -360f;
				yOffset = -25f;
			}

			UIBoatArea = new GameObject("BoatArea");
			UIBoatArea.layer = 5;
			UIBoatArea.transform.SetParent(hud.m_rootObject.transform);

			RectTransform boatAreaTransform = UIBoatArea.AddComponent<RectTransform>();
			boatAreaTransform.anchorMin = new Vector2(1f, 1f);
			boatAreaTransform.anchorMax = new Vector2(1f, 1f);
			boatAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset);
			boatAreaTransform.sizeDelta = UIBoatAreaSize;
			UIBoatArea.transform.localScale = Vector3.one;

			UIBoatText = CreateTextObject("BoatText", UIBoatArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-4f, 0f), UIBoatAreaSize);
			UIBoatEmojiTMP = CreateTMPTextObject("BoatEmojiTMP", UIBoatArea, Color.white, UIEmojiFontName, UITextFontSize + 4, TextAlignmentOptions.MidlineLeft, new Vector2(4f, 0f), UIBoatAreaSize, log);
		}
	}
}