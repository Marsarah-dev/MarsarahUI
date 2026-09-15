using HarmonyLib;
using MarsarahUI.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UISummonDisplay : UIController
	{
		private static readonly LogManager log = new LogManager("UI Summon Display", LogManager.LogLevel.Warning);

		private const float AnimationDuration = 0.2f;
		private const float SlideDistance = 8f;

		private static readonly Vector2 VisiblePosition = new Vector2(38f, -85f);

		private static GameObject UISummonArea;
		private static RectTransform summonAreaRect;
		private static CanvasGroup summonCanvasGroup;
		private static Text summonText;
		private static Image summonIcon;

		private static float animationProgress;
		private static bool targetVisible;
		private static bool animating;

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class SummonDisplayHudAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				CreateUI(__instance);
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class SummonDisplayHudUpdatePatch
		{
			private static void Postfix()
			{
				if (UISummonArea == null) return;

				bool visible =
					ConfigManager.EffectiveShowSummonCounter &&
					ShowUI &&
					UISummonCounter.NumSummons > 0;

				SetVisible(visible);
				UpdateDisplay();
				UpdateAnimation();
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UISummonArea != null) return;

			Vector2 areaSize = new Vector2(49f, 30f);

			UISummonArea = new GameObject("SummonDisplay");
			UISummonArea.layer = 5;
			UISummonArea.transform.SetParent(hud.m_healthPanel.transform, false);

			summonAreaRect = UISummonArea.AddComponent<RectTransform>();
			summonAreaRect.anchorMin = new Vector2(1f, 1f);
			summonAreaRect.anchorMax = new Vector2(1f, 1f);
			summonAreaRect.anchoredPosition = GetHiddenPosition();
			summonAreaRect.sizeDelta = areaSize;
			summonAreaRect.localScale = Vector3.one;

			Image background = UISummonArea.AddComponent<Image>();
			background.color = new Color(0f, 0f, 0f, 0.4f);

			summonCanvasGroup = UISummonArea.AddComponent<CanvasGroup>();
			summonCanvasGroup.alpha = 0f;
			summonCanvasGroup.interactable = false;
			summonCanvasGroup.blocksRaycasts = false;

			summonIcon = CreateUIImageObject("SummonIcon", UISummonArea, new Vector2(-12f, 0f), new Vector2(24f, 24f));
			summonIcon.sprite = IconManager.LoadHudIcon("Summon");
			summonIcon.preserveAspect = true;
			summonIcon.color = Color.white;

			if (summonIcon.sprite == null)
			{
				log.Warn("Could not load HUD icon 'Summon'.");
			}

			summonText = CreateTextObject("SummonText", UISummonArea, Color.white, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleRight, new Vector2(-5f, 0f), areaSize);

			UISummonArea.SetActive(false);

			log.Info("Created standalone summon display.");
		}

		private static void SetVisible(bool visible)
		{
			if (targetVisible == visible) return;

			targetVisible = visible;
			animating = true;

			if (visible)
			{
				UISummonArea.SetActive(true);
			}
		}

		private static void UpdateDisplay()
		{
			if (!targetVisible) return;

			int summons = UISummonCounter.NumSummons;
			Color color = GetSummonColor(summons);

			if (summonText != null)
			{
				summonText.text = summons.ToString();
				summonText.color = color;
			}

			if (summonIcon != null)
			{
				summonIcon.color = Color.white;
			}
		}

		private static void UpdateAnimation()
		{
			if (!animating) return;

			float target = targetVisible ? 1f : 0f;

			animationProgress = Mathf.MoveTowards(animationProgress, target, Time.deltaTime / AnimationDuration);

			float easedProgress = Mathf.SmoothStep(0f, 1f, animationProgress);

			if (summonCanvasGroup != null)
			{
				summonCanvasGroup.alpha = easedProgress;
			}

			if (summonAreaRect != null)
			{
				summonAreaRect.anchoredPosition = Vector2.Lerp(GetHiddenPosition(), VisiblePosition, easedProgress);
			}

			if (!Mathf.Approximately(animationProgress, target)) return;

			animating = false;

			if (!targetVisible)
			{
				UISummonArea.SetActive(false);
			}
		}

		private static Vector2 GetHiddenPosition()
		{
			return VisiblePosition + new Vector2(0f, SlideDistance);
		}

		private static Color GetSummonColor(int num)
		{
			if (num < 2) return new Color(1f, 0.549019f, 0f);
			if (num == 2) return Color.yellow;
			if (num >= 3) return Color.green;

			return Color.white;
		}
	}
}