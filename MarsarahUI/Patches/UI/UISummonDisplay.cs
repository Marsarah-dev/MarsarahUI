using HarmonyLib;
using MarsarahUI.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UISummonDisplay : UIController
	{
		private static readonly LogManager log = new LogManager("UI Summon Display", LogManager.LogLevel.Warning);

		private static ConfigManager.InfoRailDisplayMode currentDisplayMode;

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
		private static GameObject summonBorder;

		private static readonly Vector2 SummonTextModeSize = new Vector2(85f, 34f);

		private static readonly Vector2 SummonIconModeIconPosition = new Vector2(-9f, 0f);
		private static readonly Vector2 SummonIconModeTextPosition = new Vector2(-8f, 0f);

		private static readonly Vector2 SummonTextModeLabelPosition = new Vector2(0f, 11f);
		private static readonly Vector2 SummonTextModeValuePosition = new Vector2(0f, -11f);

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
				UpdateDisplayMode();
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

			summonBackground = CreateStyledBackground("SummonBackground", UISummonArea, style.BackgroundType, style.BackgroundColor, style.VanillaBackgroundSprite, log);
			summonBorder = CreateStyledBorder("SummonBorder", UISummonArea, style.SummonBorderType, style.BorderCapAsset, style.BorderColor, style.BorderCapSize, style.BorderOuterColor, style.BorderInnerColor, style.BorderOuterWidth, style.BorderInnerWidth, log);

			summonCanvasGroup = UISummonArea.AddComponent<CanvasGroup>();
			summonCanvasGroup.alpha = 0f;
			summonCanvasGroup.interactable = false;
			summonCanvasGroup.blocksRaycasts = false;

			summonIcon = CreateUIImageObject("SummonIcon", UISummonArea, SummonIconModeIconPosition, new Vector2(24f, 24f));
			summonIcon.sprite = IconManager.LoadHudIcon(style.SummonIcon);
			summonIcon.preserveAspect = true;
			summonIcon.color = Color.white;

			if (summonIcon.sprite == null)
			{
				log.Warn($"Could not load HUD icon '{style.SummonIcon}'.");
			}

			summonText = CreateTextObject("SummonText", UISummonArea, style.ValueTextColor, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleRight, SummonIconModeTextPosition, areaSize);

			UIStyleManager.StyleChanged -= ApplyStyle;
			UIStyleManager.StyleChanged += ApplyStyle;

			ApplyStyle();
			ApplyDisplayMode();

			UISummonArea.SetActive(false);

			log.Info("Created standalone summon display.");
		}

		private static void SetVisible(bool visible)
		{
			if (targetVisible == visible) return;

			targetVisible = visible;

			if (!ConfigManager.EffectiveInfoRailAnimations)
			{
				animationProgress = visible ? 1f : 0f;
				animating = false;

				if (summonCanvasGroup != null)
				{
					summonCanvasGroup.alpha = visible ? 1f : 0f;
				}

				if (summonAreaRect != null)
				{
					summonAreaRect.anchoredPosition = visible ? VisiblePosition : GetHiddenPosition();
				}

				UISummonArea.SetActive(visible);
				return;
			}

			animating = true;

			if (visible)
			{
				UISummonArea.SetActive(true);
			}
		}

		private static void UpdateDisplay()
		{
			if (!targetVisible) return;
			if (summonText == null) return;

			bool useText = ConfigManager.EffectiveInfoRailDisplayModeChoice == ConfigManager.InfoRailDisplayMode.Text;

			Color valueColor = UIStyleManager.GetSummonTextColor(UISummonCounter.NumSummons);
			string value = UISummonCounter.NumSummons.ToString();

			summonText.text = useText ? CreateLabeledValue("Summons", value, valueColor) : value;
			summonText.color = useText ? UIStyleManager.Current.LabelTextColor : valueColor;
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

			summonBackground = ReplaceStyledBackground(summonBackground, "SummonBackground", UISummonArea, style.BackgroundType, style.BackgroundColor, style.VanillaBackgroundSprite, log);
			summonBorder = ReplaceStyledBorder(summonBorder, "SummonBorder", UISummonArea, style.SummonBorderType, style.BorderCapAsset, style.BorderColor, style.BorderCapSize, style.BorderOuterColor, style.BorderInnerColor, style.BorderOuterWidth, style.BorderInnerWidth, log);

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
				summonText.color = UIStyleManager.GetSummonTextColor(UISummonCounter.NumSummons);
				summonText.rectTransform.sizeDelta = style.SummonSize;
			}

			ApplyDisplayMode();

			log.Info($"Applied information rail style '{ConfigManager.InfoRailStyleChoice.Value}' to summon display.");
		}

		private static void ApplyDisplayMode()
		{
			bool useText = ConfigManager.EffectiveInfoRailDisplayModeChoice == ConfigManager.InfoRailDisplayMode.Text;
			UIStyleDefinition style = UIStyleManager.Current;

			if (summonIcon != null)
			{
				summonIcon.gameObject.SetActive(!useText);
			}

			if (summonAreaRect != null)
			{
				summonAreaRect.sizeDelta = useText ? SummonTextModeSize : style.SummonSize;
			}

			if (summonText != null)
			{
				RectTransform textRect = summonText.rectTransform;

				if (useText)
				{
					textRect.anchoredPosition = Vector2.zero;
					textRect.sizeDelta = SummonTextModeSize;
					summonText.alignment = TextAnchor.MiddleCenter;
					summonText.fontSize = 14;
				}
				else
				{
					textRect.anchoredPosition = SummonIconModeTextPosition;
					textRect.sizeDelta = style.SummonSize;
					summonText.alignment = TextAnchor.MiddleRight;
					summonText.fontSize = 16;
				}
			}

			currentDisplayMode = ConfigManager.EffectiveInfoRailDisplayModeChoice;
		}

		private static void UpdateDisplayMode()
		{
			ConfigManager.InfoRailDisplayMode mode = ConfigManager.EffectiveInfoRailDisplayModeChoice;

			if (mode == currentDisplayMode) return;

			ApplyDisplayMode();
		}

		internal static void ApplyAnimationSetting()
		{
			if (ConfigManager.EffectiveInfoRailAnimations) return;
			if (UISummonArea == null) return;

			animationProgress = targetVisible ? 1f : 0f;
			animating = false;

			if (summonCanvasGroup != null)
			{
				summonCanvasGroup.alpha = targetVisible ? 1f : 0f;
			}

			if (summonAreaRect != null)
			{
				summonAreaRect.anchoredPosition = targetVisible ? VisiblePosition : GetHiddenPosition();
			}

			UISummonArea.SetActive(targetVisible);

			log.Info("Summon counter animation state finalized.");
		}
	}
}