using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MarsarahUI.Patches.UI
{
	internal class UIStatusEffects : UIController
	{
		private static readonly LogManager log = new LogManager("UI Status Effects", LogManager.LogLevel.Warning);

		private static readonly Vector2 StatusListPosition = new Vector2(-230f, -290f);

		// Status effects
		private const float StatusListWidth = 200f;
		private const float StatusListHeight = 400f;
		private const float EntrySpacing = 42f;
		private const float IconSize = 32f;
		private const float FontSize = 20f;

		private static bool layoutWarningLogged;
		private static bool vanillaLayoutCaptured;
		private static int capturedRootInstanceId;

		private static RectTransformState vanillaListRootState;
		private static RectTransformState vanillaNameState;
		private static RectTransformState vanillaIconState;
		private static RectTransformState vanillaCooldownState;
		private static TextState vanillaNameTextState;

		// Boat compass 
		private static readonly Vector2 SailingWindPosition = new Vector2(-350f, -140f);
		private static readonly Vector2 SailingControlsPosition = new Vector2(-270f, -215f);

		private const float SailingWindScale = 0.75f;
		private const float SailingControlsScale = 0.65f;

		private static bool vanillaSailingLayoutCaptured;
		private static int capturedSailingInstanceId;

		private static Vector2 vanillaWindPosition;
		private static Vector3 vanillaWindScale;
		private static Vector2 vanillaControlsPosition;
		private static Vector3 vanillaControlsScale;

		private class StatusEffectRefs
		{
			internal RectTransform Name;
			internal TMP_Text NameText;
			internal RectTransform Icon;
			internal RectTransform Cooldown;
			internal Transform TimeText;
		}

		private static readonly Dictionary<int, StatusEffectRefs> statusEffectRefs = new Dictionary<int, StatusEffectRefs>();

		private struct RectTransformState
		{
			internal Vector2 AnchorMin;
			internal Vector2 AnchorMax;
			internal Vector2 AnchoredPosition;
			internal Vector2 SizeDelta;

			internal RectTransformState(RectTransform transform)
			{
				AnchorMin = transform.anchorMin;
				AnchorMax = transform.anchorMax;
				AnchoredPosition = transform.anchoredPosition;
				SizeDelta = transform.sizeDelta;
			}

			internal void Apply(RectTransform transform)
			{
				transform.anchorMin = AnchorMin;
				transform.anchorMax = AnchorMax;
				transform.anchoredPosition = AnchoredPosition;
				transform.sizeDelta = SizeDelta;
			}
		}

		private struct TextState
		{
			internal bool RichText;
			internal TextWrappingModes TextWrappingMode;
			internal TextAlignmentOptions Alignment;
			internal float FontSize;

			internal TextState(TMP_Text text)
			{
				RichText = text.richText;
				TextWrappingMode = text.textWrappingMode;
				Alignment = text.alignment;
				FontSize = text.fontSize;
			}

			internal void Apply(TMP_Text text)
			{
				text.richText = RichText;
				text.textWrappingMode = TextWrappingMode;
				text.alignment = Alignment;
				text.fontSize = FontSize;
			}
		}

		[HarmonyPatch(typeof(Hud), "UpdateStatusEffects", new Type[] { typeof(List<StatusEffect>) })]
		private static class StatusEffects_UpdatePatch
		{
			private static void Postfix(List<StatusEffect> statusEffects, List<RectTransform> ___m_statusEffects, RectTransform ___m_statusEffectListRoot, RectTransform ___m_statusEffectTemplate, float ___m_statusEffectSpacing, int ___m_effectsPerRow)
			{
				if (statusEffects == null || ___m_statusEffects == null || ___m_statusEffectListRoot == null || ___m_statusEffectTemplate == null)
				{
					if (!layoutWarningLogged)
					{
						log.Warn("Could not update status effect layout because one or more vanilla UI references were missing.");
						layoutWarningLogged = true;
					}

					return;
				}

				if (!CaptureVanillaLayout(___m_statusEffectListRoot, ___m_statusEffectTemplate))
				{
					return;
				}

				layoutWarningLogged = false;

				if (!ConfigManager.EffectiveStatusEffectsUnderMinimap || Game.m_noMap)
				{
					RestoreVanillaLayout(___m_statusEffects, ___m_statusEffectListRoot, ___m_statusEffectSpacing, ___m_effectsPerRow);
					return;
				}

				PositionStatusEffectList(___m_statusEffectListRoot);

				int entryCount = Math.Min(statusEffects.Count, ___m_statusEffects.Count);

				for (int i = 0; i < entryCount; i++)
				{
					PositionStatusEffect(statusEffects[i], ___m_statusEffects[i], i);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "UpdateShipHud")]
		private static class SailingHud_UpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (__instance == null || __instance.m_shipWindIndicatorRoot == null || __instance.m_rudder == null) return;

				RectTransform sailingControls = __instance.m_rudder.transform.parent as RectTransform;
				if (sailingControls == null) return;

				CaptureVanillaSailingLayout(__instance.m_shipWindIndicatorRoot, sailingControls);

				if (!ConfigManager.EffectiveStatusEffectsUnderMinimap || Game.m_noMap)
				{
					RestoreVanillaSailingLayout(__instance.m_shipWindIndicatorRoot, sailingControls);
					return;
				}

				PositionSailingHud(__instance.m_shipWindIndicatorRoot, sailingControls);
			}
		}

		private static bool CaptureVanillaLayout(RectTransform listRoot, RectTransform template)
		{
			int rootInstanceId = listRoot.GetInstanceID();

			if (vanillaLayoutCaptured && capturedRootInstanceId == rootInstanceId)
			{
				return true;
			}

			RectTransform name = template.Find("Name") as RectTransform;
			RectTransform icon = template.Find("Icon") as RectTransform;
			RectTransform cooldown = template.Find("Cooldown") as RectTransform;
			TMP_Text nameText = name?.GetComponent<TMP_Text>();

			if (name == null || icon == null || cooldown == null || nameText == null)
			{
				if (!layoutWarningLogged)
				{
					log.Warn("Could not capture the vanilla status effect layout.");
					layoutWarningLogged = true;
				}

				return false;
			}

			vanillaListRootState = new RectTransformState(listRoot);
			vanillaNameState = new RectTransformState(name);
			vanillaIconState = new RectTransformState(icon);
			vanillaCooldownState = new RectTransformState(cooldown);
			vanillaNameTextState = new TextState(nameText);

			capturedRootInstanceId = rootInstanceId;
			vanillaLayoutCaptured = true;

			return true;
		}

		private static void RestoreVanillaLayout(List<RectTransform> statusEffects, RectTransform listRoot, float statusEffectSpacing, int effectsPerRow)
		{
			vanillaListRootState.Apply(listRoot);

			if (effectsPerRow <= 0) return;

			for (int i = 0; i < statusEffects.Count; i++)
			{
				RectTransform statusEffectObject = statusEffects[i];
				if (statusEffectObject == null) continue;

				int row = i / effectsPerRow;
				int column = i - row * effectsPerRow;

				statusEffectObject.anchoredPosition = new Vector2(-4f - column * statusEffectSpacing, -row * statusEffectSpacing);

				RectTransform name = statusEffectObject.Find("Name") as RectTransform;
				if (name != null)
				{
					vanillaNameState.Apply(name);

					TMP_Text nameText = name.GetComponent<TMP_Text>();
					if (nameText != null)
					{
						vanillaNameTextState.Apply(nameText);
					}
				}

				RectTransform icon = statusEffectObject.Find("Icon") as RectTransform;
				if (icon != null)
				{
					vanillaIconState.Apply(icon);
				}

				RectTransform cooldown = statusEffectObject.Find("Cooldown") as RectTransform;
				if (cooldown != null)
				{
					vanillaCooldownState.Apply(cooldown);
				}
			}
		}

		private static void PositionStatusEffectList(RectTransform listRoot)
		{
			listRoot.anchorMin = new Vector2(1f, 1f);
			listRoot.anchorMax = new Vector2(1f, 1f);
			listRoot.anchoredPosition = StatusListPosition;
			listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, StatusListWidth);
			listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, StatusListHeight);
		}

		private static void PositionStatusEffect(StatusEffect statusEffect, RectTransform statusEffectObject, int index)
		{
			if (statusEffect == null || statusEffectObject == null) return;

			statusEffectObject.localPosition = new Vector3(0f, -EntrySpacing * index, 0f);

			PositionName(statusEffect, statusEffectObject);
			PositionIcon(statusEffectObject);
			PositionCooldown(statusEffectObject);
		}

		private static void PositionName(StatusEffect statusEffect, RectTransform statusEffectObject)
		{
			RectTransform nameTransform = statusEffectObject.Find("Name") as RectTransform;
			if (nameTransform == null) return;

			TMP_Text nameText = nameTransform.GetComponent<TMP_Text>();
			if (nameText == null) return;

			nameText.richText = true;
			nameText.textWrappingMode = TextWrappingModes.Normal;
			nameText.alignment = TextAlignmentOptions.MidlineLeft;
			nameText.fontSize = FontSize;

			nameTransform.anchorMin = new Vector2(0f, 0.5f);
			nameTransform.anchorMax = new Vector2(1f, 0.5f);
			nameTransform.anchoredPosition = new Vector2(120f + IconSize, 2f);
			nameTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, StatusListWidth);
			nameTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, IconSize + 20f);

			string iconText = statusEffect.GetIconText();
			Transform timeText = statusEffectObject.Find("TimeText");

			if (!string.IsNullOrEmpty(iconText))
			{
				if (timeText != null)
				{
					timeText.gameObject.SetActive(false);
				}

				string displayName = Localization.instance.Localize(statusEffect.m_name);
				nameText.text = $"{displayName} <color=#ffb75c>{iconText}</color>";
			}
		}

		private static void PositionIcon(RectTransform statusEffectObject)
		{
			RectTransform icon = statusEffectObject.Find("Icon") as RectTransform;
			if (icon == null) return;

			icon.anchorMin = new Vector2(0.5f, 0.5f);
			icon.anchorMax = new Vector2(0.5f, 0.5f);
			icon.anchoredPosition = new Vector2(IconSize, 0f);
			icon.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, IconSize);
			icon.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, IconSize);
		}

		private static void PositionCooldown(RectTransform statusEffectObject)
		{
			RectTransform cooldown = statusEffectObject.Find("Cooldown") as RectTransform;
			if (cooldown == null) return;

			cooldown.anchorMin = new Vector2(0.5f, 0.5f);
			cooldown.anchorMax = new Vector2(0.5f, 0.5f);
			cooldown.anchoredPosition = new Vector2(20f, -10f);
			cooldown.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 16f);
			cooldown.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 16f);
		}

		private static void CaptureVanillaSailingLayout(RectTransform windIndicator, RectTransform sailingControls)
		{
			int instanceId = windIndicator.GetInstanceID();

			if (vanillaSailingLayoutCaptured && capturedSailingInstanceId == instanceId) return;

			vanillaWindPosition = windIndicator.anchoredPosition;
			vanillaWindScale = windIndicator.localScale;

			vanillaControlsPosition = sailingControls.anchoredPosition;
			vanillaControlsScale = sailingControls.localScale;

			capturedSailingInstanceId = instanceId;
			vanillaSailingLayoutCaptured = true;
		}

		private static void PositionSailingHud(RectTransform windIndicator, RectTransform sailingControls)
		{
			windIndicator.anchoredPosition = SailingWindPosition;
			windIndicator.localScale = new Vector3(SailingWindScale, SailingWindScale, 1f);

			sailingControls.anchoredPosition = SailingControlsPosition;
			sailingControls.localScale = new Vector3(SailingControlsScale, SailingControlsScale, 1f);
		}

		private static void RestoreVanillaSailingLayout(RectTransform windIndicator, RectTransform sailingControls)
		{
			if (!vanillaSailingLayoutCaptured) return;

			windIndicator.anchoredPosition = vanillaWindPosition;
			windIndicator.localScale = vanillaWindScale;

			sailingControls.anchoredPosition = vanillaControlsPosition;
			sailingControls.localScale = vanillaControlsScale;
		}

		private static StatusEffectRefs GetStatusEffectRefs(RectTransform statusEffectObject)
		{
			if (statusEffectObject == null) return null;

			int instanceId = statusEffectObject.GetInstanceID();

			if (statusEffectRefs.TryGetValue(instanceId, out StatusEffectRefs refs))
			{
				return refs;
			}

			RectTransform name = statusEffectObject.Find("Name") as RectTransform;
			RectTransform icon = statusEffectObject.Find("Icon") as RectTransform;
			RectTransform cooldown = statusEffectObject.Find("Cooldown") as RectTransform;
			Transform timeText = statusEffectObject.Find("TimeText");

			refs = new StatusEffectRefs
			{
				Name = name,
				NameText = name?.GetComponent<TMP_Text>(),
				Icon = icon,
				Cooldown = cooldown,
				TimeText = timeText
			};

			statusEffectRefs[instanceId] = refs;

			return refs;
		}
	}
}