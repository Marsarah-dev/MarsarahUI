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
		private const float PanelSpacing = 8f;

		private static readonly Vector2 SummonTextModeSize = new Vector2(85f, 34f);
		private static readonly Vector2 SummonIconModeIconPosition = new Vector2(-9f, 0f);
		private static readonly Vector2 SummonIconModeTextPosition = new Vector2(-8f, 0f);

		private static GameObject summonRoot;
		private static RectTransform summonRootRect;

		private static SummonPanel skeletonPanel;
		private static SummonPanel animalPanel;

		private class SummonPanel
		{
			internal readonly GameObject Root;
			internal readonly RectTransform Rect;
			internal readonly CanvasGroup CanvasGroup;
			internal readonly Text CountText;
			internal readonly Image Icon;

			private readonly string label;
			private readonly bool isAnimal;

			private GameObject background;
			private GameObject border;

			private float animationProgress;
			private bool targetVisible;

			internal bool IsActive => Root.activeSelf;

			internal SummonPanel(string name, string label, bool isAnimal, GameObject parent)
			{
				this.label = label;
				this.isAnimal = isAnimal;

				UIStyleDefinition style = UIStyleManager.Current;

				Root = new GameObject(name);
				Root.layer = 5;
				Root.transform.SetParent(parent.transform, false);

				Rect = Root.AddComponent<RectTransform>();
				Rect.anchorMin = new Vector2(0.5f, 0.5f);
				Rect.anchorMax = new Vector2(0.5f, 0.5f);
				Rect.pivot = new Vector2(0.5f, 0.5f);
				Rect.sizeDelta = style.SummonSize;

				CanvasGroup = Root.AddComponent<CanvasGroup>();
				CanvasGroup.alpha = 0f;
				CanvasGroup.interactable = false;
				CanvasGroup.blocksRaycasts = false;

				Icon = CreateUIImageObject("SummonIcon", Root, SummonIconModeIconPosition, new Vector2(24f, 24f));
				Icon.preserveAspect = true;
				Icon.color = Color.white;

				CountText = CreateTextObject("SummonText", Root, style.ValueTextColor, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleRight, SummonIconModeTextPosition, style.SummonSize);

				Root.SetActive(false);
			}

			internal void Update(int count, bool visible)
			{
				targetVisible = visible;

				if (visible)
				{
					if (!Root.activeSelf)
					{
						Root.SetActive(true);
					}

					UpdateDisplay(count);
				}

				float target = targetVisible ? 1f : 0f;

				animationProgress = ConfigManager.EffectiveUIAnimations
					? Mathf.MoveTowards(animationProgress, target, Time.deltaTime / AnimationDuration)
					: target;

				CanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, animationProgress);

				if (!targetVisible && animationProgress <= 0f)
				{
					Root.SetActive(false);
				}
			}

			private void UpdateDisplay(int count)
			{
				bool useText = ConfigManager.EffectiveInfoRailDisplayModeChoice == ConfigManager.InfoRailDisplayMode.Text;

				Color valueColor = UIStyleManager.GetSummonTextColor(count);
				string value = count.ToString();

				string displayText = useText ? CreateLabeledValue(label, value, valueColor) : value;
				Color displayColor = useText ? UIStyleManager.Current.LabelTextColor : valueColor;

				if (CountText.text != displayText)
				{
					CountText.text = displayText;
				}

				if (CountText.color != displayColor)
				{
					CountText.color = displayColor;
				}
			}

			internal void ApplyPosition(Vector2 position)
			{
				float easedProgress = Mathf.SmoothStep(0f, 1f, animationProgress);

				Rect.anchoredPosition = position + new Vector2(0f, Mathf.Lerp(SlideDistance, 0f, easedProgress));
			}

			internal void ApplyStyle()
			{
				UIStyleDefinition style = UIStyleManager.Current;

				background = ReplaceStyledBackground(background, "SummonBackground", Root, style.BackgroundType, style.BackgroundColor, style.VanillaBackgroundSprite, log);

				border = ReplaceStyledBorder(border, "SummonBorder", Root, style.SummonBorderType, style.BorderCapAsset, style.BorderColor, style.BorderCapSize, style.BorderOuterColor, style.BorderInnerColor, style.BorderOuterWidth, style.BorderInnerWidth, log);

				string iconName = isAnimal ? style.SummonAnimalIcon : style.SummonIcon;

				Icon.sprite = IconManager.LoadHudIcon(iconName);
				Icon.preserveAspect = true;
				Icon.color = Color.white;

				if (Icon.sprite == null)
				{
					log.Warn($"Could not load HUD icon '{iconName}'.");
				}
			}

			internal void ApplyDisplayMode()
			{
				bool useText = ConfigManager.EffectiveInfoRailDisplayModeChoice == ConfigManager.InfoRailDisplayMode.Text;
				UIStyleDefinition style = UIStyleManager.Current;

				Icon.gameObject.SetActive(!useText);

				Rect.sizeDelta = useText ? SummonTextModeSize : style.SummonSize;

				RectTransform textRect = CountText.rectTransform;

				if (useText)
				{
					textRect.anchoredPosition = Vector2.zero;
					textRect.sizeDelta = SummonTextModeSize;

					CountText.alignment = TextAnchor.MiddleCenter;
					CountText.fontSize = 14;
				}
				else
				{
					textRect.anchoredPosition = SummonIconModeTextPosition;
					textRect.sizeDelta = style.SummonSize;

					CountText.alignment = TextAnchor.MiddleRight;
					CountText.fontSize = 16;
				}
			}

			internal void FinalizeAnimation()
			{
				animationProgress = targetVisible ? 1f : 0f;

				CanvasGroup.alpha = animationProgress;
				Root.SetActive(targetVisible);
			}

			internal void HideImmediately()
			{
				targetVisible = false;
				animationProgress = 0f;

				CanvasGroup.alpha = 0f;
				Root.SetActive(false);
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class SummonDisplayHudAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;
				if (!ConfigManager.EffectiveShowSummonCounter) return;

				CreateUI(__instance);
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class SummonDisplayHudUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (!ConfigManager.EffectiveShowSummonCounter)
				{
					if (summonRoot != null)
					{
						skeletonPanel.HideImmediately();
						animalPanel.HideImmediately();
					}

					return;
				}

				if (summonRoot == null)
				{
					CreateUI(__instance);
				}

				if (summonRoot == null) return;

				UpdateDisplayMode();

				int skeletonCount = UISummonCounter.NumSummons;
				int animalCount = UISummonCounter.NumAnimalSummons;

				skeletonPanel.Update(skeletonCount, ShowUI && skeletonCount > 0);
				animalPanel.Update(animalCount, ShowUI && animalCount > 0);

				UpdateLayout();
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (summonRoot != null) return;
			if (hud == null || hud.m_healthPanel == null) return;

			summonRoot = new GameObject("SummonDisplay");
			summonRoot.layer = 5;
			summonRoot.transform.SetParent(hud.m_healthPanel.transform, false);

			summonRootRect = summonRoot.AddComponent<RectTransform>();
			summonRootRect.anchorMin = new Vector2(1f, 1f);
			summonRootRect.anchorMax = new Vector2(1f, 1f);
			summonRootRect.pivot = new Vector2(0.5f, 0.5f);
			summonRootRect.sizeDelta = Vector2.zero;

			skeletonPanel = new SummonPanel("SkeletonSummonDisplay", "Skeletons", false, summonRoot);
			animalPanel = new SummonPanel("AnimalSummonDisplay", "Spirits", true, summonRoot);

			UIStyleManager.StyleChanged -= ApplyStyle;
			UIStyleManager.StyleChanged += ApplyStyle;

			ApplyStyle();
			ApplySummonCounterTransform();

			log.Info("Created skeleton and animal summon displays.");
		}

		private static void UpdateLayout()
		{
			if (summonRoot == null) return;

			skeletonPanel.ApplyPosition(Vector2.zero);

			float animalOffset = skeletonPanel.IsActive
				? skeletonPanel.Rect.sizeDelta.y + PanelSpacing
				: 0f;

			animalPanel.ApplyPosition(new Vector2(0f, animalOffset));
		}

		internal static void ApplyStyle()
		{
			if (summonRoot == null) return;

			skeletonPanel.ApplyStyle();
			animalPanel.ApplyStyle();

			ApplyDisplayMode();

			log.Info($"Applied UI style '{ConfigManager.UIStyleChoice.Value}' to summon displays.");
		}

		internal static void ApplySummonCounterTransform()
		{
			if (summonRootRect == null) return;

			float scale = ConfigManager.SummonCounterScale.Value / 100f;

			summonRootRect.localScale = new Vector3(scale, scale, 1f);
			summonRootRect.anchoredPosition = ConfigManager.SummonCounterPosition.Value;

			UpdateLayout();

			log.Info($"Applied summon counter transform: position {ConfigManager.SummonCounterPosition.Value}, scale {ConfigManager.SummonCounterScale.Value}%.");
		}

		private static void ApplyDisplayMode()
		{
			skeletonPanel.ApplyDisplayMode();
			animalPanel.ApplyDisplayMode();

			currentDisplayMode = ConfigManager.EffectiveInfoRailDisplayModeChoice;

			UpdateLayout();
		}

		private static void UpdateDisplayMode()
		{
			ConfigManager.InfoRailDisplayMode mode = ConfigManager.EffectiveInfoRailDisplayModeChoice;

			if (mode == currentDisplayMode) return;

			ApplyDisplayMode();
		}

		internal static void ApplyAnimationSetting()
		{
			if (ConfigManager.EffectiveUIAnimations) return;
			if (summonRoot == null) return;

			skeletonPanel.FinalizeAnimation();
			animalPanel.FinalizeAnimation();

			UpdateLayout();

			log.Info("Summon counter animation states finalized.");
		}
	}
}