using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static Skills;

namespace MarsarahUI.Patches.UI
{
	internal class UISkillProgress : UIController
	{
		private static readonly LogManager log = new LogManager("UI Skill Progress", LogManager.LogLevel.Info);

		private static readonly MethodInfo getSkillMethod = AccessTools.Method(typeof(Skills), "GetSkill", new Type[] { typeof(SkillType) });
		private static readonly MethodInfo getNextLevelRequirementMethod = AccessTools.Method(typeof(Skills.Skill), "GetNextLevelRequirement");
		private static readonly MethodInfo getSkillDefMethod = AccessTools.Method(typeof(Skills), "GetSkillDef", new Type[] { typeof(SkillType) });

		private const float DisplayDuration = 3f;
		private const float BarHeight = 4f;
		private const float FadeDuration = 0.5f;
		private static readonly Color SkillTextColor = Color.yellow;

		private static GameObject UISkillProgressArea;
		private static GameObject UISkillProgressTextArea;
		private static Image skillProgressFill;
		private static Text skillProgressText;
		private static Image skillProgressIcon;
		private static float displayTimer;

		private static CanvasGroup skillProgressCanvasGroup;
		private static CanvasGroup skillProgressTextCanvasGroup;

		private static void SetUIActive(bool active)
		{
			UISkillProgressArea?.SetActive(active);
			UISkillProgressTextArea?.SetActive(active);
		}

		private struct SkillProgressState
		{
			internal bool Valid;
			internal int Level;
			internal int Percent;
		}

		[HarmonyPatch(typeof(Skills), nameof(Skills.RaiseSkill))]
		private static class SkillsRaiseSkillPatch
		{
			private static void Prefix(Skills __instance, SkillType skillType, out SkillProgressState __state)
			{
				__state = new SkillProgressState();

				if (ConfigManager.EffectiveSkillProgressBarChoice == ConfigManager.SkillProgressBarColor.Off) return;
				if (Player.m_localPlayer == null || __instance != Player.m_localPlayer.GetSkills()) return;
				if (skillType == SkillType.Run) return;

				Skills.Skill skill = GetSkill(__instance, skillType);
				if (skill == null || skill.m_level >= 100f) return;

				float requirement = GetNextLevelRequirement(skill);
				if (requirement <= 0f) return;

				__state.Valid = true;
				__state.Level = Mathf.FloorToInt(skill.m_level);
				__state.Percent = Mathf.FloorToInt(skill.m_accumulator / requirement * 100f);
			}

			private static void Postfix(Skills __instance, SkillType skillType, SkillProgressState __state)
			{
				if (!__state.Valid) return;

				Skills.Skill skill = GetSkill(__instance, skillType);
				if (skill == null) return;

				int level = Mathf.FloorToInt(skill.m_level);

				if (level != __state.Level) return;

				float requirement = GetNextLevelRequirement(skill);
				if (requirement <= 0f) return;

				int percent = Mathf.FloorToInt(skill.m_accumulator / requirement * 100f);

				if (percent <= __state.Percent) return;

				Sprite skillIcon = GetSkillIcon(__instance, skillType);

				ShowProgress(skillType.ToString(), level, percent, skillIcon);
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class SkillProgressHudAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				CreateUI(__instance);
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class SkillProgressHudUpdatePatch
		{
			private static void Postfix()
			{
				if (UISkillProgressArea == null || UISkillProgressTextArea == null) return;

				if (ConfigManager.EffectiveSkillProgressBarChoice == ConfigManager.SkillProgressBarColor.Off)
				{
					displayTimer = 0f;
					SetUIActive(false);
					return;
				}

				if (!ShowUI)
				{
					SetUIActive(false);
					return;
				}

				if (displayTimer <= 0f)
				{
					SetUIActive(false);
					return;
				}

				displayTimer -= Time.deltaTime;

				float alpha = displayTimer < FadeDuration
					? Mathf.Clamp01(displayTimer / FadeDuration)
					: 1f;

				SetUIAlpha(alpha);

				if (skillProgressFill != null)
				{
					skillProgressFill.color = GetBarColor(ConfigManager.EffectiveSkillProgressBarChoice);
				}
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UISkillProgressArea != null) return;

			UISkillProgressArea = new GameObject("SkillProgressArea");
			UISkillProgressArea.layer = 5;
			UISkillProgressArea.transform.SetParent(hud.transform, false);

			skillProgressCanvasGroup = UISkillProgressArea.AddComponent<CanvasGroup>();

			RectTransform areaTransform = UISkillProgressArea.AddComponent<RectTransform>();
			areaTransform.anchorMin = new Vector2(0f, 0f);
			areaTransform.anchorMax = new Vector2(1f, 0f);
			areaTransform.pivot = new Vector2(0.5f, 0f);
			areaTransform.anchoredPosition = Vector2.zero;
			areaTransform.sizeDelta = new Vector2(0f, 34f);
			areaTransform.localScale = Vector3.one;

			GameObject backgroundObject = new GameObject("SkillProgressBackground");
			backgroundObject.layer = 5;
			backgroundObject.transform.SetParent(UISkillProgressArea.transform, false);

			RectTransform backgroundTransform = backgroundObject.AddComponent<RectTransform>();
			backgroundTransform.anchorMin = new Vector2(0f, 0f);
			backgroundTransform.anchorMax = new Vector2(1f, 0f);
			backgroundTransform.pivot = new Vector2(0.5f, 0f);
			backgroundTransform.anchoredPosition = Vector2.zero;
			backgroundTransform.sizeDelta = new Vector2(0f, BarHeight);

			Image background = backgroundObject.AddComponent<Image>();
			background.color = new Color(0f, 0f, 0f, 0.55f);

			GameObject fillObject = new GameObject("SkillProgressFill");
			fillObject.layer = 5;
			fillObject.transform.SetParent(backgroundObject.transform, false);

			RectTransform fillTransform = fillObject.AddComponent<RectTransform>();
			fillTransform.anchorMin = new Vector2(0f, 0f);
			fillTransform.anchorMax = new Vector2(0f, 1f);
			fillTransform.pivot = new Vector2(0f, 0.5f);
			fillTransform.anchoredPosition = Vector2.zero;
			fillTransform.sizeDelta = Vector2.zero;

			skillProgressFill = fillObject.AddComponent<Image>();

			Vector2 skillTextAreaSize = new Vector2(96f, 30f);

			UISkillProgressTextArea = new GameObject("SkillProgressTextArea");
			UISkillProgressTextArea.layer = 5;
			UISkillProgressTextArea.transform.SetParent(hud.m_healthPanel.transform);

			skillProgressTextCanvasGroup = UISkillProgressTextArea.AddComponent<CanvasGroup>();

			RectTransform skillTextAreaTransform = UISkillProgressTextArea.AddComponent<RectTransform>();
			skillTextAreaTransform.anchorMin = new Vector2(1f, 1f);
			skillTextAreaTransform.anchorMax = new Vector2(1f, 1f);
			skillTextAreaTransform.sizeDelta = skillTextAreaSize;
			skillTextAreaTransform.localScale = Vector3.one;

			Sprite backgroundSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == "InputFieldBackground");

			Image skillTextAreaBackground = UISkillProgressTextArea.AddComponent<Image>();
			skillTextAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
			skillTextAreaBackground.sprite = backgroundSprite;
			skillTextAreaBackground.type = Image.Type.Sliced;

			skillProgressIcon = CreateUIImageObject("SkillProgressIcon", UISkillProgressTextArea, new Vector2(-31f, 0f), new Vector2(24f, 24f));
			skillProgressIcon.preserveAspect = true;

			skillProgressText = CreateTextObject("SkillProgressText", UISkillProgressTextArea, SkillTextColor, "AveriaSansLibre-Bold", 13, TextAnchor.MiddleCenter, new Vector2(12f, 0f), new Vector2(64f, 30f));

			UpdatePosition();

			UISkillProgressArea.transform.SetAsLastSibling();
			UISkillProgressArea.SetActive(false);
			UISkillProgressTextArea.SetActive(false);

			UISkillProgressArea.transform.SetAsLastSibling();
			UISkillProgressArea.SetActive(false);
		}

		private static void ShowProgress(string skillName, int level, int percent, Sprite skillIcon)
		{
			if (UISkillProgressArea == null || skillProgressFill == null || skillProgressText == null) return;

			float progress = Mathf.Clamp01(percent / 100f);

			RectTransform fillTransform = skillProgressFill.rectTransform;
			fillTransform.anchorMax = new Vector2(progress, 1f);

			skillProgressFill.color = GetBarColor(ConfigManager.EffectiveSkillProgressBarChoice);

			if (skillIcon != null)
			{
				skillProgressIcon.sprite = skillIcon;
				skillProgressIcon.gameObject.SetActive(true);
				skillProgressText.text = $"{level} - {percent}%";
			}
			else
			{
				skillProgressIcon.gameObject.SetActive(false);
				skillProgressText.text = $"{skillName} {level} - {percent}%";
			}

			SetUIAlpha(1f);
			displayTimer = DisplayDuration;
			SetUIActive(true);
		}

		private static Color GetBarColor(ConfigManager.SkillProgressBarColor color)
		{
			switch (color)
			{
				case ConfigManager.SkillProgressBarColor.White:
					return new Color(0.92f, 0.92f, 0.88f);

				case ConfigManager.SkillProgressBarColor.Green:
					return new Color(0.45f, 0.75f, 0.38f);

				case ConfigManager.SkillProgressBarColor.Blue:
					return new Color(0.35f, 0.60f, 0.88f);

				case ConfigManager.SkillProgressBarColor.Cyan:
					return new Color(0.35f, 0.80f, 0.82f);

				case ConfigManager.SkillProgressBarColor.Red:
					return new Color(0.85f, 0.36f, 0.32f);

				case ConfigManager.SkillProgressBarColor.Purple:
					return new Color(0.65f, 0.45f, 0.82f);

				case ConfigManager.SkillProgressBarColor.Gold:
				default:
					return new Color(1f, 0.72f, 0.36f);
			}
		}

		private static Skills.Skill GetSkill(Skills skills, SkillType skillType)
		{
			if (getSkillMethod == null)
			{
				log.Warn("Could not find Skills.GetSkill.");
				return null;
			}

			return getSkillMethod.Invoke(skills, new object[] { skillType }) as Skills.Skill;
		}

		private static Sprite GetSkillIcon(Skills skills, SkillType skillType)
		{
			if (getSkillDefMethod == null)
			{
				log.Warn("Could not find Skills.GetSkillDef.");
				return null;
			}

			Skills.SkillDef skillDef = getSkillDefMethod.Invoke(skills, new object[] { skillType }) as Skills.SkillDef;

			return skillDef?.m_icon;
		}

		private static float GetNextLevelRequirement(Skills.Skill skill)
		{
			if (getNextLevelRequirementMethod == null)
			{
				log.Warn("Could not find Skills.Skill.GetNextLevelRequirement.");
				return 0f;
			}

			return (float)getNextLevelRequirementMethod.Invoke(skill, null);
		}

		internal static void UpdatePosition()
		{
			if (UISkillProgressTextArea == null) return;

			bool inventoryEnabled = ConfigManager.EffectiveShowInventoryWeightAndSlots;
			bool enemyDetectorEnabled = ConfigManager.EffectiveShowEnemyDetector;

			float xOffset;

			if (inventoryEnabled && enemyDetectorEnabled)
			{
				xOffset = 253f;
			}
			else if (inventoryEnabled)
			{
				xOffset = 145f;
			}
			else if (enemyDetectorEnabled)
			{
				xOffset = 66f;
			}
			else
			{
				xOffset = -42f;
			}

			RectTransform textAreaTransform = UISkillProgressTextArea.GetComponent<RectTransform>();
			textAreaTransform.anchoredPosition = new Vector2(xOffset, -230f);
		}

		private static void SetUIAlpha(float alpha)
		{
			if (skillProgressCanvasGroup != null)
			{
				skillProgressCanvasGroup.alpha = alpha;
			}

			if (skillProgressTextCanvasGroup != null)
			{
				skillProgressTextCanvasGroup.alpha = alpha;
			}
		}
	}
}