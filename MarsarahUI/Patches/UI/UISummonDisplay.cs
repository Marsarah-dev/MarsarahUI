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

		private static GameObject summonBackground;

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
			UIStyleDefinition style = UIStyleManager.Current;

			if (UISummonArea != null) return;

			Vector2 areaSize = style.SummonSize;

			UISummonArea = new GameObject("SummonDisplay");
			UISummonArea.layer = 5;
			UISummonArea.transform.SetParent(hud.m_healthPanel.transform, false);

			summonAreaRect = UISummonArea.AddComponent<RectTransform>();
			summonAreaRect.anchorMin = new Vector2(1f, 1f);
			summonAreaRect.anchorMax = new Vector2(1f, 1f);
			summonAreaRect.anchoredPosition = GetHiddenPosition();
			summonAreaRect.sizeDelta = areaSize;
			summonAreaRect.localScale = Vector3.one;

			summonBackground = CreateThreePartBackground("SummonBackground", UISummonArea, style.SummonBackgroundAsset, style.SummonSourceEndWidth, style.SummonEndWidth, log);

			summonCanvasGroup = UISummonArea.AddComponent<CanvasGroup>();
			summonCanvasGroup.alpha = 0f;
			summonCanvasGroup.interactable = false;
			summonCanvasGroup.blocksRaycasts = false;

			summonIcon = CreateUIImageObject("SummonIcon", UISummonArea, new Vector2(-12f, 0f), new Vector2(24f, 24f));
			summonIcon.sprite = IconManager.LoadHudIcon(style.SummonIcon);
			summonIcon.preserveAspect = true;
			summonIcon.color = Color.white;

			if (summonIcon.sprite == null)
			{
				log.Warn($"Could not load HUD icon '{style.SummonIcon}'.");
			}

			summonText = CreateTextObject("SummonText", UISummonArea, style.ValueTextColor, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleRight, new Vector2(-5f, 0f), areaSize);

			UIStyleManager.StyleChanged -= ApplyStyle;
			UIStyleManager.StyleChanged += ApplyStyle;

			ApplyStyle();

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

			if (summonText != null)
			{
				summonText.text = UISummonCounter.NumSummons.ToString();
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

		internal static void ApplyStyle()
		{
			if (UISummonArea == null) return;

			UIStyleDefinition style = UIStyleManager.Current;

			if (summonAreaRect != null)
			{
				summonAreaRect.sizeDelta = style.SummonSize;
			}

			summonBackground = ReplaceThreePartBackground(summonBackground, "SummonBackground", UISummonArea, style.SummonBackgroundAsset, style.SummonSourceEndWidth, style.SummonEndWidth, log);

			if (summonIcon != null)
			{
				summonIcon.sprite = IconManager.LoadHudIcon(style.SummonIcon);
				summonIcon.preserveAspect = true;
				summonIcon.color = Color.white;

				if (summonIcon.sprite == null)
				{
					log.Warn($"Could not load HUD icon '{style.SummonIcon}'.");
				}
			}

			if (summonText != null)
			{
				summonText.color = style.ValueTextColor;
				summonText.rectTransform.sizeDelta = style.SummonSize;
			}

			log.Info($"Applied information rail style '{ConfigManager.InfoRailStyleChoice.Value}' to summon display.");
		}
	}
}