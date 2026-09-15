using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

namespace MarsarahUI.Patches.UI
{
	internal class UIInfoRail : UIController
	{
		private static readonly LogManager log = new LogManager("UI Info Rail", LogManager.LogLevel.Warning);

		internal enum ElementType
		{
			Weight,
			Slots,
			Enemies,
			ToughEnemies,
			Bosses,
			NeutralEnemies,
			Summons,
			Skill
		}

		// Weight and Slots
		private static Image weightBarFill;
		private static Text weightText;
		private static TextMeshProUGUI weightIcon;
		private static Text slotsText;
		private static TextMeshProUGUI slotsIcon;

		// Enemy Detector
		private static Text enemyText;
		private static TextMeshProUGUI enemyIcon;
		private static Text neutralEnemyText;
		private static TextMeshProUGUI neutralEnemyIcon;
		private static Text toughEnemyText;
		private static TextMeshProUGUI toughEnemyIcon;
		private static Text bossText;
		private static TextMeshProUGUI bossIcon;

		// Summon Counter
		private static Text summonText;
		private static TextMeshProUGUI summonIcon;

		// Skill Progress
		private static Image skillIcon;
		private static Text skillText;

		// Rail
		private const float RailHeight = 30f;
		private const float AnimationDuration = 0.2f;
		private const float SlideDistance = 8f;
		private static GameObject UIRail;
		private static RectTransform railRect;

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
				UpdateSummonElements();
				UpdateSkillElements();
				UpdateAnimations();
				UpdateRailVisibility();
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UIRail != null) return;

			UIRail = new GameObject("InfoRail");
			UIRail.layer = 5;
			UIRail.transform.SetParent(hud.m_healthPanel.transform, false);

			railRect = UIRail.AddComponent<RectTransform>();
			railRect.anchorMin = new Vector2(1f, 1f);
			railRect.anchorMax = new Vector2(1f, 1f);
			railRect.pivot = new Vector2(0f, 0.5f);
			railRect.anchoredPosition = new Vector2(-90f, -230f);
			railRect.sizeDelta = new Vector2(0f, RailHeight);
			railRect.localScale = Vector3.one;

			Image background = UIRail.AddComponent<Image>();
			background.color = new Color(0f, 0f, 0f, 0.4f);

			HorizontalLayoutGroup layoutGroup = UIRail.AddComponent<HorizontalLayoutGroup>();
			layoutGroup.padding = new RectOffset(4, 4, 2, 2);
			layoutGroup.spacing = 0f;
			layoutGroup.childAlignment = TextAnchor.MiddleLeft;
			layoutGroup.childControlWidth = true;
			layoutGroup.childControlHeight = true;
			layoutGroup.childForceExpandWidth = false;
			layoutGroup.childForceExpandHeight = true;

			ContentSizeFitter sizeFitter = UIRail.AddComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

			CreateElement(ElementType.Weight, 130f);
			CreateElement(ElementType.Slots, 50f);
			CreateElement(ElementType.Enemies, 50f);
			CreateElement(ElementType.ToughEnemies, 50f);
			CreateElement(ElementType.Bosses, 50f);
			CreateElement(ElementType.NeutralEnemies, 50f);
			CreateElement(ElementType.Summons, 50f);
			CreateElement(ElementType.Skill, 96f);

			CreateInventoryElements();
			CreateEnemyElements();
			CreateSummonElements();
			CreateSkillElements();

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

				weightIcon = CreateTMPTextObject("WeightIcon", weightContent, Color.white, "NotoEmoji-Regular SDF", 20, TextAlignmentOptions.Midline, new Vector2(-50f, 0f), new Vector2(30f, RailHeight), log);
				weightIcon.text = "🏋️";

				weightText = CreateTextObject("WeightText", weightContent, Color.white, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(15f, 0f), new Vector2(100f, RailHeight));
			}

			GameObject slotsContent = GetElementContent(ElementType.Slots);

			if (slotsContent != null)
			{
				slotsIcon = CreateTMPTextObject("SlotsIcon", slotsContent, Color.white, "NotoEmoji-Regular SDF", 20, TextAlignmentOptions.Midline, new Vector2(-13f, 0f), new Vector2(24f, RailHeight), log);
				slotsIcon.text = "🎒";

				slotsText = CreateTextObject("SlotsText", slotsContent, Color.white, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, RailHeight));
			}
		}

		private static void CreateEnemyElements()
		{
			GameObject enemyContent = GetElementContent(ElementType.Enemies);

			if (enemyContent != null)
			{
				enemyIcon = CreateTMPTextObject("EnemyIcon", enemyContent, Color.white, "NotoEmoji-Regular SDF", 20, TextAlignmentOptions.Midline, new Vector2(-13f, 0f), new Vector2(24f, RailHeight), log);
				enemyText = CreateTextObject("EnemyText", enemyContent, Color.white, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, RailHeight));
			}

			GameObject toughEnemyContent = GetElementContent(ElementType.ToughEnemies);

			if (toughEnemyContent != null)
			{
				toughEnemyIcon = CreateTMPTextObject("ToughEnemyIcon", toughEnemyContent, Color.white, "NotoEmoji-Regular SDF", 20, TextAlignmentOptions.Midline, new Vector2(-13f, 0f), new Vector2(24f, RailHeight), log);
				toughEnemyIcon.text = "☠";

				toughEnemyText = CreateTextObject("ToughEnemyText", toughEnemyContent, Color.white, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, RailHeight));
			}

			GameObject bossContent = GetElementContent(ElementType.Bosses);

			if (bossContent != null)
			{
				bossIcon = CreateTMPTextObject("BossIcon", bossContent, Color.white, "NotoEmoji-Regular SDF", 20, TextAlignmentOptions.Midline, new Vector2(-13f, 0f), new Vector2(24f, RailHeight), log);
				bossIcon.text = "👑";

				bossText = CreateTextObject("BossText", bossContent, Color.magenta, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, RailHeight));
			}

			GameObject neutralContent = GetElementContent(ElementType.NeutralEnemies);

			if (neutralContent != null)
			{
				neutralEnemyIcon = CreateTMPTextObject("NeutralEnemyIcon", neutralContent, Color.white, "NotoEmoji-Regular SDF", 20, TextAlignmentOptions.Midline, new Vector2(-13f, 0f), new Vector2(24f, RailHeight), log);
				neutralEnemyIcon.text = "🧔";

				neutralEnemyText = CreateTextObject("NeutralEnemyText", neutralContent, Color.green, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, RailHeight));
				neutralEnemyText.color = Color.green;
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

				Color weightColor = GetWeightColor(weightPercent);

				if (weightBarFill != null)
				{
					weightBarFill.fillAmount = weightPercent;
					weightBarFill.color = weightColor;
				}

				if (weightText != null)
				{
					weightText.text = $"{UIInventoryWeightAndSlots.CurrentWeight:0.0}/{UIInventoryWeightAndSlots.MaxWeight:0}";
				}

				if (weightIcon != null)
				{
					weightIcon.color = weightColor;
				}
			}

			if (showSlots)
			{
				Color slotsColor = GetSlotsColor(UIInventoryWeightAndSlots.SlotsUsedPercent);

				if (slotsText != null)
				{
					slotsText.text = UIInventoryWeightAndSlots.FreeSlots.ToString();
					slotsText.color = slotsColor;
				}

				if (slotsIcon != null)
				{
					slotsIcon.color = slotsColor;
				}
			}
		}

		private static Color GetSlotsColor(float percent)
		{
			if (percent < 33f) return Color.green;
			if (percent < 66f) return Color.yellow;
			if (percent < 100f) return new Color(1f, 0.549019f, 0f);

			return Color.red;
		}

		private static Color GetWeightColor(float percent)
		{
			Color green = Color.green;
			Color yellow = Color.yellow;
			Color orange = new Color(1f, 0.549019f, 0f);

			if (percent <= 0.33f)
			{
				return green;
			}

			if (percent <= 0.66f)
			{
				return Color.Lerp(green, yellow, (percent - 0.33f) / 0.33f);
			}

			if (percent < 1f)
			{
				return Color.Lerp(yellow, orange, (percent - 0.66f) / 0.34f);
			}

			return Color.red;
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

			SetElementVisible(ElementType.Enemies, true);
			SetElementVisible(ElementType.ToughEnemies, separateToughEnemies && toughEnemies > 0);
			SetElementVisible(ElementType.Bosses, bosses > 0);
			SetElementVisible(ElementType.NeutralEnemies, neutralEnemies > 0);

			if (enemyText != null)
			{
				enemyText.text = enemies.ToString();
				enemyText.color = GetEnemyColor(enemies);
			}

			if (enemyIcon != null)
			{
				enemyIcon.text = enemies == 0 ? "👁" : "😈";
				enemyIcon.color = Color.white;
			}

			if (separateToughEnemies && toughEnemies > 0)
			{
				if (toughEnemyText != null)
				{
					toughEnemyText.text = toughEnemies.ToString();
					toughEnemyText.color = GearProgressionManager.GetThreatColor(UIEnemyDetector.ToughEnemyThreatLevel);
				}

				if (toughEnemyIcon != null)
				{
					toughEnemyIcon.color = Color.white;
				}
			}

			if (bosses > 0)
			{
				if (bossText != null)
				{
					bossText.text = bosses.ToString();
					bossText.color = Color.magenta;
				}

				if (bossIcon != null)
				{
					bossIcon.color = Color.white;
				}
			}

			if (neutralEnemies > 0)
			{
				if (neutralEnemyText != null)
				{
					neutralEnemyText.text = neutralEnemies.ToString();
					neutralEnemyText.color = Color.green;
				}

				if (neutralEnemyIcon != null)
				{
					neutralEnemyIcon.color = Color.white;
				}
			}
		}

		private static Color GetEnemyColor(int num)
		{
			if (num < 3) return Color.green;
			if (num < 5) return Color.yellow;
			if (num < 7) return new Color(1f, 0.549019f, 0f);

			return Color.red;
		}

		private static void CreateSummonElements()
		{
			GameObject summonContent = GetElementContent(ElementType.Summons);

			if (summonContent == null) return;

			summonIcon = CreateTMPTextObject("SummonIcon", summonContent, Color.white, "NotoEmoji-Regular SDF", 20, TextAlignmentOptions.Midline, new Vector2(-13f, 0f), new Vector2(24f, RailHeight), log);
			summonIcon.text = "💀";

			summonText = CreateTextObject("SummonText", summonContent, Color.white, "AveriaSansLibre-Bold", 16, TextAnchor.MiddleCenter, new Vector2(13f, 0f), new Vector2(24f, RailHeight));
		}

		private static void UpdateSummonElements()
		{
			bool enabled = ConfigManager.EffectiveShowSummonCounter;
			int summons = UISummonCounter.NumSummons;

			bool visible = enabled && summons > 0;

			SetElementVisible(ElementType.Summons, visible);

			if (!visible) return;

			if (summonText != null)
			{
				summonText.text = summons.ToString();
				summonText.color = GetSummonColor(summons);
			}

			if (summonIcon != null)
			{
				summonIcon.color = Color.white;
			}
		}

		private static Color GetSummonColor(int num)
		{
			if (num < 2) return new Color(1f, 0.549019f, 0f);
			if (num == 2) return Color.yellow;
			if (num >= 3) return Color.green;

			return Color.white;
		}

		private static void CreateSkillElements()
		{
			GameObject skillContent = GetElementContent(ElementType.Skill);

			if (skillContent == null) return;

			skillIcon = CreateUIImageObject("SkillIcon", skillContent, new Vector2(-31f, 0f), new Vector2(24f, 24f));
			skillIcon.preserveAspect = true;

			skillText = CreateTextObject("SkillText", skillContent, Color.yellow, "AveriaSansLibre-Bold", 13, TextAnchor.MiddleCenter, new Vector2(12f, 0f), new Vector2(64f, RailHeight));
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
				skillText.color = Color.yellow;
			}
		}
	}
}