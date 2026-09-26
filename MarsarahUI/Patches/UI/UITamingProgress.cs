using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace MarsarahUI.Patches.UI
{
	internal class UITamingProgress : UIController
	{
		private static readonly LogManager log = new LogManager("UI Taming Progress", LogManager.LogLevel.Info);

		private static readonly FieldInfo hudsField;
		private static readonly Type hudDataType;
		private static readonly FieldInfo hudGuiField;
		private static readonly FieldInfo hudCharacterField;
		private static readonly MethodInfo getTamenessMethod;

		private static bool wasEnabled;
		private static readonly List<TextMeshProUGUI> tamingTexts = new List<TextMeshProUGUI>();

		private static readonly ConditionalWeakTable<object, TextMeshProUGUI> tamingCache = new ConditionalWeakTable<object, TextMeshProUGUI>();

		static UITamingProgress()
		{
			hudsField = typeof(EnemyHud).GetField("m_huds", BindingFlags.NonPublic | BindingFlags.Instance);
			hudDataType = typeof(EnemyHud).GetNestedType("HudData", BindingFlags.NonPublic);

			if (hudsField == null || hudDataType == null)
			{
				log.Error("Failed to locate EnemyHud.m_huds or nested type HudData via reflection.");
				return;
			}

			hudGuiField = hudDataType.GetField("m_gui", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hudCharacterField = hudDataType.GetField("m_character", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			getTamenessMethod = typeof(Tameable).GetMethod("GetTameness", BindingFlags.NonPublic | BindingFlags.Instance);

			if (hudGuiField == null || hudCharacterField == null || getTamenessMethod == null)
			{
				log.Error("Failed to locate one or more fields or methods required for taming progress.");
			}
		}

		[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
		private static class EnemyHud_ShowHud_Taming_Patch
		{
			private static void Postfix(EnemyHud __instance, Character c)
			{
				if (!ConfigManager.EffectiveShowTamingProgress) return;
				if (c == null || hudsField == null) return;

				IDictionary huds = hudsField.GetValue(__instance) as IDictionary;
				if (huds == null || !huds.Contains(c)) return;

				object hudData = huds[c];
				if (hudData == null) return;

				GameObject guiObject = hudGuiField?.GetValue(hudData) as GameObject;
				if (guiObject == null) return;

				RectTransform healthTransform = guiObject.transform.Find("Health") as RectTransform;
				if (healthTransform == null) return;

				AddTamingText(hudData, healthTransform);
			}
		}

		[HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
		private static class EnemyHud_UpdateHuds_Taming_Patch
		{
			private static void Postfix(EnemyHud __instance)
			{
				if (!ConfigManager.EffectiveShowTamingProgress)
				{
					if (wasEnabled)
					{
						HideAllTamingTexts();
						wasEnabled = false;
					}

					return;
				}

				wasEnabled = true;

				if (hudsField == null) return;

				IDictionary huds = hudsField.GetValue(__instance) as IDictionary;
				if (huds == null) return;

				foreach (DictionaryEntry entry in huds)
				{
					object hudData = entry.Value;
					if (hudData == null) continue;

					Character character = hudCharacterField?.GetValue(hudData) as Character;
					if (character == null || character.IsDead()) continue;

					UpdateTamingText(character, hudData);
				}
			}
		}

		private static void AddTamingText(object hudData, RectTransform healthTransform)
		{
			if (tamingCache.TryGetValue(hudData, out _)) return;

			string UITMPFontName = "Valheim-AveriaSansLibre";
			Vector2 UITextAreaSize = new Vector2(100f, 14f);
			int UITextFontSize = 11;

			TextMeshProUGUI tamingText = CreateTMPTextObject("TamingText", healthTransform.gameObject, Color.white, UITMPFontName, UITextFontSize, TextAlignmentOptions.BottomRight, Vector2.zero, UITextAreaSize, log);

			tamingText.gameObject.SetActive(false);
			tamingText.text = "";

			RectTransform tamingRect = tamingText.rectTransform;
			tamingRect.anchorMin = new Vector2(1f, 0f);
			tamingRect.anchorMax = new Vector2(1f, 0f);
			tamingRect.pivot = new Vector2(1f, 0f);
			tamingRect.anchoredPosition = new Vector2(-3f, -14f);

			tamingCache.Add(hudData, tamingText);
			tamingTexts.Add(tamingText);
		}

		private static void UpdateTamingText(Character character, object hudData)
		{
			if (!tamingCache.TryGetValue(hudData, out TextMeshProUGUI tamingText)) return;

			if (!ConfigManager.EffectiveShowTamingProgress || !ShowUI)
			{
				tamingText.gameObject.SetActive(false);
				return;
			}

			if (!character.TryGetComponent(out Tameable tameable) || tameable.IsTamed())
			{
				tamingText.gameObject.SetActive(false);
				return;
			}

			int tamingProgress = 0;

			if (getTamenessMethod != null)
			{
				tamingProgress = (int)getTamenessMethod.Invoke(tameable, null);
			}

			tamingText.gameObject.SetActive(tamingProgress != 0);

			if (tamingProgress == 0) return;

			string status = tameable.GetStatusString();

			tamingText.text = $"Taming: {tamingProgress}%";
			tamingText.color = status switch
			{
				"$hud_tamehungry" => new Color(1f, 0.549f, 0f),
				"$hud_tamefrightened" => Color.red,
				_ => Color.cyan
			};
		}

		private static void HideAllTamingTexts()
		{
			foreach (TextMeshProUGUI tamingText in tamingTexts)
			{
				if (tamingText != null && tamingText.gameObject.activeSelf)
				{
					tamingText.gameObject.SetActive(false);
				}
			}

			log.Info("Taming progress UI hidden.");
		}
	}
}