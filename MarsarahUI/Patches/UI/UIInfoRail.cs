using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIInfoRail : UIController
	{
		private static readonly LogManager log = new LogManager("UI Info Rail", LogManager.LogLevel.Info);


		internal enum ElementType
		{
			Weight,
			Slots,
			Enemies,
			ToughEnemies,
			Bosses,
			NeutralEnemies,
			Skill
		}

		private static readonly Dictionary<ElementType, RailSeparator> separators = new Dictionary<ElementType, RailSeparator>();

		private static readonly ElementType[] elementOrder =
		{
			ElementType.Weight,
			ElementType.Slots,
			ElementType.Enemies,
			ElementType.ToughEnemies,
			ElementType.Bosses,
			ElementType.NeutralEnemies,
			ElementType.Skill
		};

		private class RailSeparator
		{
			internal GameObject Root;
			internal LayoutElement Layout;
			internal CanvasGroup CanvasGroup;
			internal RectTransform LineRect;
			internal Image LineImage;
			internal float Progress;
			internal bool TargetVisible;
			internal bool Animating;
		}

		// Weight and Slots
		private static Image weightBarFill;
		private static Text weightText;
		private static Image weightIcon;
		private static Text slotsText;
		private static Image slotsIcon;

		// Enemy Detector
		private static Text enemyText;
		private static Image enemyIcon;
		private static Text neutralEnemyText;
		private static Image neutralEnemyIcon;
		private static Text toughEnemyText;
		private static Image toughEnemyIcon;
		private static Text bossText;
		private static Image bossIcon;

		// Skill Progress
		private static Image skillIcon;
		private static Text skillText;

		// Rail
		private const float WeightWidth = 136f;
		private const float SlotsWidth = 52f;
		private const float CounterWidth = 52f;
		private const float SkillWidth = 100f;

		private const float IconSize = 24f;
		private const float CounterIconX = -13f;
		private const float CounterTextX = 13f;

		private const float AnimationDuration = 0.2f;
		private const float SlideDistance = 8f;
		private static GameObject UIRail;
		private static RectTransform railRect;

		private static GameObject railBackground;
		private static HorizontalLayoutGroup railLayoutGroup;

		private static readonly Dictionary<ElementType, RailElement> elements = new Dictionary<ElementType, RailElement>();

		private class RailElement
		{
			internal GameObject Root;
			internal GameObject Content;
			internal RectTransform ContentRect;
			internal LayoutElement Layout;
			internal CanvasGroup CanvasGroup;
			internal float PreferredWidth;
			internal float Progress;
			internal bool TargetVisible;
			internal bool Animating;
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class InfoRailHudAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				CreateUI(__instance);
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class InfoRailHudUpdatePatch
		{
			private static void Postfix()
			{
				if (UIRail == null) return;

				UpdateInventoryElements();
				UpdateEnemyElements();
				UpdateSkillElements();

				UpdateSeparatorTargets();
				UpdateAnimations();
				UpdateRailVisibility();
			}
		}

		private static Image CreateRailIcon(string objectName, GameObject parent, string iconName, Vector2 position)
		{
			Image icon = CreateUIImageObject(objectName, parent, position, new Vector2(24f, 24f));
			icon.sprite = IconManager.LoadHudIcon(iconName);
			icon.preserveAspect = true;
			icon.color = Color.white;

			if (icon.sprite == null)
			{
				log.Warn($"Could not load HUD icon '{iconName}'.");
			}

			return icon;
		}

		private static void CreateUI(Hud hud)
		{
			if (UIRail != null) return;

			UIStyleDefinition style = UIStyleManager.Current;

			UIRail = new GameObject("InfoRail");
			UIRail.layer = 5;
			UIRail.transform.SetParent(hud.m_healthPanel.transform, false);

			railRect = UIRail.AddComponent<RectTransform>();
			railRect.anchorMin = new Vector2(1f, 1f);
			railRect.anchorMax = new Vector2(1f, 1f);
			railRect.pivot = new Vector2(0f, 0.5f);
			railRect.anchoredPosition = new Vector2(-88f, -230f);
			railRect.sizeDelta = new Vector2(0f, style.RailHeight);
			railRect.localScale = Vector3.one;

			railBackground = CreateStyledBackground("RailBackground", UIRail, style.BackgroundType, style.BackgroundColor, style.VanillaBackgroundSprite, style.RailBackgroundAsset, style.RailSourceEndWidth, style.RailEndWidth, log);

			railLayoutGroup = UIRail.AddComponent<HorizontalLayoutGroup>();
			railLayoutGroup.padding = style.RailPadding;
			railLayoutGroup.spacing = 0f;
			railLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
			railLayoutGroup.childControlWidth = true;
			railLayoutGroup.childControlHeight = true;
			railLayoutGroup.childForceExpandWidth = false;
			railLayoutGroup.childForceExpandHeight = true;

			ContentSizeFitter sizeFitter = UIRail.AddComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

			CreateElement(ElementType.Weight, WeightWidth);
			CreateSeparator(ElementType.Weight);

			CreateElement(ElementType.Slots, SlotsWidth);
			CreateSeparator(ElementType.Slots);

			CreateElement(ElementType.Enemies, CounterWidth);
			CreateSeparator(ElementType.Enemies);

			CreateElement(ElementType.ToughEnemies, CounterWidth);
			CreateSeparator(ElementType.ToughEnemies);

			CreateElement(ElementType.Bosses, CounterWidth);
			CreateSeparator(ElementType.Bosses);

			CreateElement(ElementType.NeutralEnemies, CounterWidth);
			CreateSeparator(ElementType.NeutralEnemies);

			CreateElement(ElementType.Skill, SkillWidth);

			CreateInventoryElements();
			CreateEnemyElements();
			CreateSkillElements();

			UIStyleManager.StyleChanged -= ApplyStyle;
			UIStyleManager.StyleChanged += ApplyStyle;

			ApplyStyle();

			UIRail.SetActive(false);

			log.Info("Created information rail.");
		}

		private static void CreateElement(ElementType type, float preferredWidth)
		{
			GameObject root = new GameObject($"{type}Element");
			root.layer = 5;
			root.transform.SetParent(UIRail.transform, false);

			RectTransform rootRect = root.AddComponent<RectTransform>();
			rootRect.localScale = Vector3.one;

			LayoutElement layout = root.AddComponent<LayoutElement>();
			layout.preferredWidth = 0f;
			layout.minWidth = 0f;
			layout.flexibleWidth = 0f;

			GameObject content = new GameObject("Content");
			content.layer = 5;
			content.transform.SetParent(root.transform, false);

			RectTransform contentRect = content.AddComponent<RectTransform>();
			contentRect.anchorMin = Vector2.zero;
			contentRect.anchorMax = Vector2.one;
			contentRect.offsetMin = Vector2.zero;
			contentRect.offsetMax = Vector2.zero;
			contentRect.localScale = Vector3.one;

			CanvasGroup canvasGroup = content.AddComponent<CanvasGroup>();
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;

			elements[type] = new RailElement
			{
				Root = root,
				Content = content,
				ContentRect = contentRect,
				Layout = layout,
				CanvasGroup = canvasGroup,
				PreferredWidth = preferredWidth,
				Progress = 0f,
				TargetVisible = false,
				Animating = false
			};

			root.SetActive(false);
		}

		private static void CreateSeparator(ElementType afterElement)
		{
			UIStyleDefinition style = UIStyleManager.Current;

			GameObject root = new GameObject($"{afterElement}Separator");
			root.layer = 5;
			root.transform.SetParent(UIRail.transform, false);

			RectTransform rootRect = root.AddComponent<RectTransform>();
			rootRect.localScale = Vector3.one;

			LayoutElement layout = root.AddComponent<LayoutElement>();
			layout.preferredWidth = 0f;
			layout.minWidth = 0f;
			layout.flexibleWidth = 0f;

			CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;

			GameObject lineObject = new GameObject("Line");
			lineObject.layer = 5;
			lineObject.transform.SetParent(root.transform, false);

			RectTransform lineRect = lineObject.AddComponent<RectTransform>();
			lineRect.anchorMin = new Vector2(0.5f, 0.5f);
			lineRect.anchorMax = new Vector2(0.5f, 0.5f);
			lineRect.pivot = new Vector2(0.5f, 0.5f);
			lineRect.anchoredPosition = Vector2.zero;
			lineRect.sizeDelta = new Vector2(style.SeparatorLineWidth, style.SeparatorHeight);

			Image line = lineObject.AddComponent<Image>();
			line.color = style.SeparatorColor;

			separators[afterElement] = new RailSeparator
			{
				Root = root,
				Layout = layout,
				CanvasGroup = canvasGroup,
				LineRect = lineRect,
				LineImage = line,
				Progress = 0f,
				TargetVisible = false,
				Animating = false
			};

			root.SetActive(false);
		}

		private static void UpdateSeparatorTargets()
		{
			for (int i = 0; i < elementOrder.Length - 1; i++)
			{
				ElementType currentType = elementOrder[i];

				if (!elements.TryGetValue(currentType, out RailElement currentElement))
				{
					continue;
				}

				bool hasVisibleElementAfter = false;

				for (int j = i + 1; j < elementOrder.Length; j++)
				{
					if (elements.TryGetValue(elementOrder[j], out RailElement laterElement) &&
						laterElement.TargetVisible)
					{
						hasVisibleElementAfter = true;
						break;
					}
				}

				bool shouldShow = currentElement.TargetVisible && hasVisibleElementAfter;

				SetSeparatorVisible(currentType, shouldShow);
			}
		}

		private static void SetSeparatorVisible(ElementType afterElement, bool visible)
		{
			if (!separators.TryGetValue(afterElement, out RailSeparator separator)) return;
			if (separator.TargetVisible == visible && !separator.Animating) return;

			separator.TargetVisible = visible;

			if (visible)
			{
				separator.Root.SetActive(true);
			}

			separator.Animating = true;
		}

		internal static GameObject GetElementContent(ElementType type)
		{
			if (!elements.TryGetValue(type, out RailElement element)) return null;

			return element.Content;
		}

		internal static void SetElementVisible(ElementType type, bool visible, bool animate = true)
		{
			if (!elements.TryGetValue(type, out RailElement element)) return;
			if (element.TargetVisible == visible && !element.Animating) return;

			element.TargetVisible = visible;

			if (!animate)
			{
				element.Progress = visible ? 1f : 0f;
				element.Layout.preferredWidth = visible ? element.PreferredWidth : 0f;
				element.CanvasGroup.alpha = visible ? 1f : 0f;
				element.ContentRect.anchoredPosition = Vector2.zero;
				element.Animating = false;
				element.Root.SetActive(visible);

				RebuildLayout();
				UpdateRailVisibility();
				return;
			}

			if (visible)
			{
				element.Root.SetActive(true);
			}

			element.Animating = true;
			UpdateRailVisibility();
		}

		private static void UpdateAnimations()
		{
			bool layoutChanged = false;

			foreach (RailElement element in elements.Values)
			{
				if (!element.Animating) continue;

				float target = element.TargetVisible ? 1f : 0f;
				element.Progress = Mathf.MoveTowards(element.Progress, target, Time.deltaTime / AnimationDuration);

				float easedProgress = Mathf.SmoothStep(0f, 1f, element.Progress);

				element.Layout.preferredWidth = element.PreferredWidth * easedProgress;
				element.CanvasGroup.alpha = easedProgress;
				element.ContentRect.anchoredPosition = new Vector2(Mathf.Lerp(-SlideDistance, 0f, easedProgress), 0f);

				layoutChanged = true;

				if (!Mathf.Approximately(element.Progress, target)) continue;

				element.Animating = false;

				if (!element.TargetVisible)
				{
					element.Root.SetActive(false);
				}
			}

			foreach (RailSeparator separator in separators.Values)
			{
				if (!separator.Animating) continue;

				float target = separator.TargetVisible ? 1f : 0f;

				separator.Progress = Mathf.MoveTowards(
					separator.Progress,
					target,
					Time.deltaTime / AnimationDuration);

				float easedProgress = Mathf.SmoothStep(0f, 1f, separator.Progress);

				separator.Layout.preferredWidth = UIStyleManager.Current.SeparatorWidth * easedProgress;
				separator.CanvasGroup.alpha = easedProgress;

				layoutChanged = true;

				if (!Mathf.Approximately(separator.Progress, target)) continue;

				separator.Animating = false;

				if (!separator.TargetVisible)
				{
					separator.Root.SetActive(false);
				}
			}

			if (layoutChanged)
			{
				RebuildLayout();
			}
		}

		private static void RebuildLayout()
		{
			if (railRect == null) return;

			LayoutRebuilder.ForceRebuildLayoutImmediate(railRect);
		}

		private static void UpdateRailVisibility()
		{
			if (UIRail == null) return;

			bool hasVisibleElement = false;

			foreach (RailElement element in elements.Values)
			{
				if (!element.Root.activeSelf) continue;

				hasVisibleElement = true;
				break;
			}

			UIRail.SetActive(ShowUI && hasVisibleElement);
		}

		private static void CreateInventoryElements()
		{
			UIStyleDefinition style = UIStyleManager.Current;

			GameObject weightContent = GetElementContent(ElementType.Weight);

			if (weightContent != null)
			{
				GameObject fillArea = new GameObject("WeightBarFill");
				fillArea.layer = 5;
				fillArea.transform.SetParent(weightContent.transform, false);

				RectTransform fillRect = fillArea.AddComponent<RectTransform>();
				fillRect.anchorMin = new Vector2(0f, 0.5f);
				fillRect.anchorMax = new Vector2(0f, 0.5f);
				fillRect.pivot = new Vector2(0.5f, 0.5f);
				fillRect.anchoredPosition = new Vector2(80f, 0f);
				fillRect.sizeDelta = new Vector2(94f, 24f);

				weightBarFill = fillArea.AddComponent<Image>();
				weightBarFill.sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == "bar_monster_hp_5");
				weightBarFill.type = Image.Type.Filled;
				weightBarFill.fillMethod = Image.FillMethod.Horizontal;
				weightBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
				weightBarFill.fillAmount = 0f;

				weightIcon = CreateRailIcon("WeightIcon", weightContent, style.WeightIcon, new Vector2(-50f, 0f));
				weightIcon.preserveAspect = true;

				weightText = CreateTextObject("WeightText", weightContent, style.ValueTextColor, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(15f, 0f), new Vector2(100f, UIStyleManager.Current.RailHeight));
			}

			GameObject slotsContent = GetElementContent(ElementType.Slots);

			if (slotsContent != null)
			{
				slotsIcon = CreateRailIcon("SlotsIcon", slotsContent, style.SlotsIcon, new Vector2(-13f, 0f));

				slotsText = CreateTextObject("SlotsText", slotsContent, style.ValueTextColor, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, UIStyleManager.Current.RailHeight));
			}
		}

		private static void CreateEnemyElements()
		{
			UIStyleDefinition style = UIStyleManager.Current;

			GameObject enemyContent = GetElementContent(ElementType.Enemies);

			if (enemyContent != null)
			{
				enemyIcon = CreateRailIcon("EnemyIcon", enemyContent, style.EnemyIcon, new Vector2(-13f, 0f));
				enemyText = CreateTextObject("EnemyText", enemyContent, style.ValueTextColor, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, UIStyleManager.Current.RailHeight));
			}

			GameObject toughEnemyContent = GetElementContent(ElementType.ToughEnemies);

			if (toughEnemyContent != null)
			{
				toughEnemyIcon = CreateRailIcon("ToughEnemyIcon", toughEnemyContent, style.ToughEnemyIcon, new Vector2(-13f, 0f));
				toughEnemyText = CreateTextObject("ToughEnemyText", toughEnemyContent, style.ValueTextColor, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, UIStyleManager.Current.RailHeight));
			}

			GameObject bossContent = GetElementContent(ElementType.Bosses);

			if (bossContent != null)
			{
				bossIcon = CreateRailIcon("BossIcon", bossContent, style.BossIcon, new Vector2(-13f, 0f));
				bossText = CreateTextObject("BossText", bossContent, style.ValueTextColor, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, UIStyleManager.Current.RailHeight));
			}

			GameObject neutralContent = GetElementContent(ElementType.NeutralEnemies);

			if (neutralContent != null)
			{
				neutralEnemyIcon = CreateRailIcon("NeutralEnemyIcon", neutralContent, style.NeutralIcon, new Vector2(-13f, 0f));
				neutralEnemyText = CreateTextObject("NeutralEnemyText", neutralContent, style.ValueTextColor, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, UIStyleManager.Current.RailHeight));
			}
		}

		private static void UpdateInventoryElements()
		{
			ConfigManager.InventoryDisplayMode mode = ConfigManager.EffectiveInventoryDisplayChoice;

			bool showWeight =
				mode == ConfigManager.InventoryDisplayMode.WeightAndFreeSlots ||
				mode == ConfigManager.InventoryDisplayMode.WeightOnly;

			bool showSlots =
				mode == ConfigManager.InventoryDisplayMode.WeightAndFreeSlots ||
				mode == ConfigManager.InventoryDisplayMode.FreeSlotsOnly;

			SetElementVisible(ElementType.Weight, showWeight);
			SetElementVisible(ElementType.Slots, showSlots);

			if (showWeight)
			{
				float weightPercent = UIInventoryWeightAndSlots.MaxWeight > 0f
					? Mathf.Clamp01(UIInventoryWeightAndSlots.CurrentWeight / UIInventoryWeightAndSlots.MaxWeight)
					: 0f;

				if (weightBarFill != null)
				{
					weightBarFill.fillAmount = weightPercent;
					weightBarFill.color = UIStyleManager.Current.WeightFillColor;
				}

				if (weightText != null)
				{
					weightText.text = $"{UIInventoryWeightAndSlots.CurrentWeight:0.0}/{UIInventoryWeightAndSlots.MaxWeight:0}";
					weightText.color = UIStyleManager.Current.ValueTextColor;
				}
			}

			if (showSlots)
			{
				if (slotsText != null)
				{
					slotsText.text = UIInventoryWeightAndSlots.FreeSlots.ToString();
					slotsText.color = UIStyleManager.Current.ValueTextColor;
				}
			}
		}

		private static void UpdateEnemyElements()
		{
			ConfigManager.EnemyDetectorMode mode = ConfigManager.EffectiveEnemyDetectorChoice;
			bool detectorEnabled = mode != ConfigManager.EnemyDetectorMode.Off;

			if (!detectorEnabled)
			{
				SetElementVisible(ElementType.Enemies, false);
				SetElementVisible(ElementType.ToughEnemies, false);
				SetElementVisible(ElementType.Bosses, false);
				SetElementVisible(ElementType.NeutralEnemies, false);
				return;
			}

			bool separateToughEnemies = mode == ConfigManager.EnemyDetectorMode.SeparateToughEnemies;

			int enemies = UIEnemyDetector.NumEnemies;
			int toughEnemies = UIEnemyDetector.NumToughEnemies;
			int bosses = UIEnemyDetector.NumBosses;
			int neutralEnemies = UIEnemyDetector.NumNeutralEnemies;

			SetElementVisible(ElementType.Enemies, enemies > 0);
			SetElementVisible(ElementType.ToughEnemies, separateToughEnemies && toughEnemies > 0);
			SetElementVisible(ElementType.Bosses, bosses > 0);
			SetElementVisible(ElementType.NeutralEnemies, neutralEnemies > 0);

			if (enemyText != null)
			{
				enemyText.text = enemies.ToString();
			}

			if (separateToughEnemies && toughEnemies > 0 && toughEnemyText != null)
			{
				toughEnemyText.text = toughEnemies.ToString();
			}

			if (bosses > 0 && bossText != null)
			{
				bossText.text = bosses.ToString();
			}

			if (neutralEnemies > 0 && neutralEnemyText != null)
			{
				neutralEnemyText.text = neutralEnemies.ToString();
			}
		}

		private static void CreateSkillElements()
		{
			UIStyleDefinition style = UIStyleManager.Current;

			GameObject skillContent = GetElementContent(ElementType.Skill);

			if (skillContent == null) return;

			skillIcon = CreateUIImageObject("SkillIcon", skillContent, new Vector2(-31f, 0f), new Vector2(24f, 24f));
			skillIcon.preserveAspect = true;

			skillText = CreateTextObject("SkillText", skillContent, style.ValueTextColor, "AveriaSansLibre-Bold", 13, TextAnchor.MiddleCenter, new Vector2(12f, 0f), new Vector2(64f, UIStyleManager.Current.RailHeight));
		}

		private static void UpdateSkillElements()
		{
			bool enabled = ConfigManager.EffectiveSkillProgressBarChoice != ConfigManager.SkillProgressBarColor.Off;
			bool visible = enabled && UISkillProgress.IsDisplaying;

			SetElementVisible(ElementType.Skill, visible);

			if (!visible) return;

			if (skillIcon != null)
			{
				skillIcon.sprite = UISkillProgress.CurrentSkillIcon;
				skillIcon.gameObject.SetActive(UISkillProgress.CurrentSkillIcon != null);
			}

			if (skillText != null)
			{
				skillText.text = UISkillProgress.CurrentDisplayText;
			}
		}

		internal static void ApplyStyle()
		{
			if (UIRail == null) return;

			UIStyleDefinition style = UIStyleManager.Current;

			if (railRect != null)
			{
				railRect.sizeDelta = new Vector2(railRect.sizeDelta.x, style.RailHeight);
			}

			if (railLayoutGroup != null)
			{
				railLayoutGroup.padding = style.RailPadding;
			}

			railBackground = ReplaceStyledBackground(railBackground, "RailBackground", UIRail, style.BackgroundType, style.BackgroundColor, style.VanillaBackgroundSprite, style.RailBackgroundAsset, style.RailSourceEndWidth, style.RailEndWidth, log);

			ApplyIconStyle(weightIcon, style.WeightIcon);
			ApplyIconStyle(slotsIcon, style.SlotsIcon);
			ApplyIconStyle(enemyIcon, style.EnemyIcon);
			ApplyIconStyle(toughEnemyIcon, style.ToughEnemyIcon);
			ApplyIconStyle(bossIcon, style.BossIcon);
			ApplyIconStyle(neutralEnemyIcon, style.NeutralIcon);

			if (weightText != null) weightText.color = style.ValueTextColor;
			if (slotsText != null) slotsText.color = style.ValueTextColor;
			if (enemyText != null) enemyText.color = style.ValueTextColor;
			if (toughEnemyText != null) toughEnemyText.color = style.ValueTextColor;
			if (bossText != null) bossText.color = style.ValueTextColor;
			if (neutralEnemyText != null) neutralEnemyText.color = style.ValueTextColor;
			if (skillText != null) skillText.color = style.ValueTextColor;

			if (weightBarFill != null)
			{
				weightBarFill.color = style.WeightFillColor;
			}

			foreach (RailSeparator separator in separators.Values)
			{
				if (separator.LineRect != null)
				{
					separator.LineRect.sizeDelta = new Vector2(style.SeparatorLineWidth, style.SeparatorHeight);
				}

				if (separator.LineImage != null)
				{
					separator.LineImage.color = style.SeparatorColor;
				}

				if (!separator.Animating)
				{
					separator.Layout.preferredWidth = separator.TargetVisible ? style.SeparatorWidth : 0f;
				}
			}

			UpdateTextHeights(style.RailHeight);
			RebuildLayout();

			log.Info($"Applied information rail style '{ConfigManager.InfoRailStyleChoice.Value}'.");
		}

		private static void ApplyIconStyle(Image icon, string iconName)
		{
			if (icon == null) return;

			icon.sprite = IconManager.LoadHudIcon(iconName);
			icon.preserveAspect = true;

			if (icon.sprite == null)
			{
				log.Warn($"Could not load HUD icon '{iconName}'.");
			}
		}

		private static void UpdateTextHeights(float railHeight)
		{
			UpdateTextHeight(weightText, railHeight);
			UpdateTextHeight(slotsText, railHeight);
			UpdateTextHeight(enemyText, railHeight);
			UpdateTextHeight(toughEnemyText, railHeight);
			UpdateTextHeight(bossText, railHeight);
			UpdateTextHeight(neutralEnemyText, railHeight);
			UpdateTextHeight(skillText, railHeight);
		}

		private static void UpdateTextHeight(Text text, float height)
		{
			if (text == null) return;

			RectTransform rect = text.rectTransform;
			rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
		}
	}
} 