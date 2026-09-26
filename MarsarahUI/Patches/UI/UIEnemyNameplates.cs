using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using static MarsarahUI.Managers.ConfigManager;
using System.Linq;

namespace MarsarahUI.Patches.UI
{
	internal class UIEnemyNameplates : UIController
	{
		private static readonly LogManager log = new LogManager("UI Enemy Nameplates", LogManager.LogLevel.Info);

		private const float BarHeight = 14f;
		private const float BarHeightBoss = 18f;

		private static float defaultDistance = -1f;
		private static float defaultBarHeight = -1f;
		private static float defaultBarHeightBoss = -1f;

		private static bool wasEnabled;

		private static readonly Dictionary<object, float> lastBarHeight = new Dictionary<object, float>();

		private static readonly FieldInfo m_hudsField;
		private static readonly Type hudDataType;
		private static readonly FieldInfo hud_m_gui_Field;
		private static readonly FieldInfo hud_m_character_Field;
		private static readonly FieldInfo hud_m_healthFast_Field;
		private static readonly FieldInfo hud_m_healthSlow_Field;
		private static readonly FieldInfo hud_m_healthFastFriendly_Field;
		private static readonly FieldInfo hud_m_name_Field;
		private static readonly FieldInfo hud_m_alerted_Field;
		private static readonly FieldInfo hud_m_aware_Field;

		private static TMP_FontAsset hpFont;
		private static Material hpFontMaterial;

		private class HpTexts
		{
			public TextMeshProUGUI HP;
			public TextMeshProUGUI HPPercent;
		}

		private static readonly ConditionalWeakTable<object, HpTexts> hpTextCache = new ConditionalWeakTable<object, HpTexts>();

		static UIEnemyNameplates()
		{
			m_hudsField = typeof(EnemyHud).GetField("m_huds", BindingFlags.NonPublic | BindingFlags.Instance);
			hudDataType = typeof(EnemyHud).GetNestedType("HudData", BindingFlags.NonPublic);

			if (m_hudsField == null || hudDataType == null)
			{
				log.Error("Failed to locate EnemyHud.m_huds or nested type HudData via reflection.");
				return;
			}

			hud_m_gui_Field = hudDataType.GetField("m_gui", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_character_Field = hudDataType.GetField("m_character", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_healthFast_Field = hudDataType.GetField("m_healthFast", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_healthSlow_Field = hudDataType.GetField("m_healthSlow", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_healthFastFriendly_Field = hudDataType.GetField("m_healthFastFriendly", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_name_Field = hudDataType.GetField("m_name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_alerted_Field = hudDataType.GetField("m_alerted", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_aware_Field = hudDataType.GetField("m_aware", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			if (hud_m_gui_Field == null ||
				hud_m_character_Field == null ||
				hud_m_healthFast_Field == null ||
				hud_m_healthSlow_Field == null ||
				hud_m_healthFastFriendly_Field == null ||
				hud_m_name_Field == null ||
				hud_m_alerted_Field == null ||
				hud_m_aware_Field == null)
			{
				log.Error("Failed to locate one or more EnemyHud.HudData fields required for enemy nameplates.");
			}
		}

		[HarmonyPatch(typeof(EnemyHud), "Awake")]
		private static class EnemyHud_Awake_Patch
		{
			private static void Postfix(EnemyHud __instance)
			{
				if (__instance == null) return;
				if (ConfigManager.EffectiveEnemyNameplateChoice == EnemyNameplateMode.Off) return;

				if (defaultDistance < 0f)
				{
					defaultDistance = __instance.m_maxShowDistance;
				}

				UpdateDisplayDistance(__instance);
			}
		}

		[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
		private static class EnemyHud_ShowHud_CustomBar_Patch
		{
			private static void Prefix(EnemyHud __instance, Character c, out bool __state)
			{
				__state = true;

				if (ConfigManager.EffectiveEnemyNameplateChoice == EnemyNameplateMode.Off) return;
				if (__instance == null || c == null || m_hudsField == null) return;

				IDictionary huds = m_hudsField.GetValue(__instance) as IDictionary;

				__state = huds != null && huds.Contains(c);
			}

			private static void Postfix(EnemyHud __instance, Character c, bool __state)
			{
				if (ConfigManager.EffectiveEnemyNameplateChoice == EnemyNameplateMode.Off) return;
				if (__state) return;
				if (__instance == null || c == null || m_hudsField == null) return;

				IDictionary huds = m_hudsField.GetValue(__instance) as IDictionary;
				if (huds == null || !huds.Contains(c)) return;

				object hudData = huds[c];
				if (hudData == null) return;

				GameObject guiObject = hud_m_gui_Field?.GetValue(hudData) as GameObject;
				if (guiObject == null) return;

				RectTransform healthTransform = guiObject.transform.Find("Health") as RectTransform;
				if (healthTransform == null) return;

				GuiBar fastBar = hud_m_healthFast_Field?.GetValue(hudData) as GuiBar;
				GuiBar slowBar = hud_m_healthSlow_Field?.GetValue(hudData) as GuiBar;
				GuiBar fastFriendlyBar = hud_m_healthFastFriendly_Field?.GetValue(hudData) as GuiBar;

				ApplyBarSettings(c, healthTransform, fastBar, slowBar, fastFriendlyBar, true);

				EnemyNameplateMode mode = ConfigManager.EffectiveEnemyNameplateChoice;

				if (mode != EnemyNameplateMode.BarsOnly)
				{
					AddHpText(hudData, healthTransform);
				}
			}
		}

		[HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
		private static class EnemyHud_UpdateHuds_CustomBar_Patch
		{
			private static void Postfix(EnemyHud __instance)
			{
				bool enabled = ConfigManager.EffectiveEnemyNameplateChoice != EnemyNameplateMode.Off;

				if (!enabled)
				{
					if (wasEnabled)
					{
						RestoreVanillaState(__instance);
						wasEnabled = false;
					}

					return;
				}

				wasEnabled = true;

				if (m_hudsField == null) return;

				if (defaultDistance < 0f)
				{
					defaultDistance = __instance.m_maxShowDistance;
				}

				UpdateDisplayDistance(__instance);

				IDictionary huds = m_hudsField.GetValue(__instance) as IDictionary;
				if (huds == null) return;

				foreach (DictionaryEntry entry in huds)
				{
					object hudData = entry.Value;
					if (hudData == null) continue;

					Character character = hud_m_character_Field?.GetValue(hudData) as Character;
					if (character == null || character.IsDead()) continue;

					GameObject guiObject = hud_m_gui_Field?.GetValue(hudData) as GameObject;
					RectTransform healthTransform = guiObject?.transform.Find("Health") as RectTransform;

					GuiBar fastBar = hud_m_healthFast_Field?.GetValue(hudData) as GuiBar;
					GuiBar slowBar = hud_m_healthSlow_Field?.GetValue(hudData) as GuiBar;
					GuiBar fastFriendlyBar = hud_m_healthFastFriendly_Field?.GetValue(hudData) as GuiBar;

					if (healthTransform != null)
					{
						ApplyBarSettings(character, healthTransform, fastBar, slowBar, fastFriendlyBar, enabled);
					}

					UpdateHpText(character, hudData, enabled);
					UpdateAlertAndName(character, hudData, enabled);
					UpdatePlayerBarColor(character, hudData, enabled);
				}
			}
		}

		private static void UpdateDisplayDistance(EnemyHud enemyHud)
		{
			if (enemyHud == null || defaultDistance < 0f) return;

			bool enabled = ConfigManager.EffectiveEnemyNameplateChoice != EnemyNameplateMode.Off;
			enemyHud.m_maxShowDistance = enabled ? defaultDistance * 2f : defaultDistance;
		}

		private static void ApplyBarSettings(Character character, RectTransform health, GuiBar fastBar, GuiBar slowBar, GuiBar fastFriendlyBar, bool enabled)
		{
			if (character == null || health == null) return;

			if (!character.IsBoss() && defaultBarHeight < 0f)
			{
				defaultBarHeight = health.sizeDelta.y;
			}

			if (character.IsBoss() && defaultBarHeightBoss < 0f)
			{
				defaultBarHeightBoss = health.sizeDelta.y;
			}

			float targetHeight = enabled
				? character.IsBoss() ? BarHeightBoss : BarHeight
				: character.IsBoss() ? defaultBarHeightBoss : defaultBarHeight;

			if (targetHeight >= 0f &&
				(!lastBarHeight.TryGetValue(health, out float lastHeight) || lastHeight != targetHeight))
			{
				health.sizeDelta = new Vector2(health.sizeDelta.x, targetHeight);

				if (fastBar?.m_bar != null)
				{
					fastBar.m_bar.sizeDelta = new Vector2(fastBar.m_bar.sizeDelta.x, targetHeight);
				}

				if (slowBar?.m_bar != null)
				{
					slowBar.m_bar.sizeDelta = new Vector2(slowBar.m_bar.sizeDelta.x, targetHeight);
				}

				if (fastFriendlyBar?.m_bar != null)
				{
					fastFriendlyBar.m_bar.sizeDelta = new Vector2(fastFriendlyBar.m_bar.sizeDelta.x, targetHeight);
				}

				lastBarHeight[health] = targetHeight;
			}

			if (!enabled) return;

			Color bossColor = Color.magenta;
			Color neutralColor = Color.yellow;
			Color playerColor = Color.green;
			Color enemyColor = Color.red;

			if (character.IsBoss())
			{
				fastBar?.SetColor(bossColor);
			}
			else if (character.IsTamed())
			{
				fastBar?.SetColor(neutralColor);
				fastFriendlyBar?.SetColor(neutralColor);
			}
			else if (character.IsPlayer())
			{
				Color color = character.IsPVPEnabled() ? bossColor : playerColor;

				fastBar?.SetColor(color);
				fastFriendlyBar?.SetColor(color);
			}
			else
			{
				fastBar?.SetColor(enemyColor);
				fastFriendlyBar?.SetColor(neutralColor);
			}
		}

		private static void AddHpText(object hudData, RectTransform healthTransform)
		{
			if (hpTextCache.TryGetValue(hudData, out _)) return;

			TextMeshProUGUI hpText = CreateHpTextObject("HpText", healthTransform.gameObject);
			TextMeshProUGUI hpPercentText = CreateHpTextObject("HpPercentText", healthTransform.gameObject);

			hpText.gameObject.SetActive(false);
			hpPercentText.gameObject.SetActive(false);

			hpTextCache.Add(hudData, new HpTexts
			{
				HP = hpText,
				HPPercent = hpPercentText
			});
		}

		private static void UpdateHpText(Character character, object hudData, bool enabled)
		{
			if (!hpTextCache.TryGetValue(hudData, out HpTexts hpTexts)) return;

			EnemyNameplateMode mode = ConfigManager.EffectiveEnemyNameplateChoice;

			bool showHealth = enabled &&
				(mode == EnemyNameplateMode.BarsWithHealth || mode == EnemyNameplateMode.BarsWithBoth);

			bool showPercent = enabled &&
				(mode == EnemyNameplateMode.BarsWithPercent || mode == EnemyNameplateMode.BarsWithBoth);

			bool showBoth = enabled && mode == EnemyNameplateMode.BarsWithBoth;

			hpTexts.HP.gameObject.SetActive(showHealth);
			hpTexts.HPPercent.gameObject.SetActive(showPercent);

			if (!showHealth && !showPercent) return;

			UpdateHpTextLayout(hpTexts, showBoth);

			float currentHealth = character.GetHealth();
			float maxHealth = character.GetMaxHealth();
			float healthFraction = Mathf.Clamp01(currentHealth / Math.Max(1f, maxHealth));

			hpTexts.HP.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
			hpTexts.HPPercent.text = $"{Mathf.RoundToInt(healthFraction * 100f)}%";

			Color textColor = Color.white;

			if (character.IsTamed() || character.IsPlayer())
			{
				textColor = Color.black;
			}
			else
			{
				BaseAI characterAI = character.GetBaseAI();
				bool isEnemy = characterAI != null && Player.m_localPlayer != null && characterAI.IsEnemy(Player.m_localPlayer);

				if (!isEnemy)
				{
					textColor = Color.black;
				}
			}

			hpTexts.HP.color = textColor;
			hpTexts.HPPercent.color = textColor;
		}

		private static void UpdateHpTextLayout(HpTexts hpTexts, bool showBoth)
		{
			RectTransform hpRect = hpTexts.HP.rectTransform;

			if (showBoth)
			{
				hpRect.anchorMin = hpRect.anchorMax = new Vector2(0f, 0.5f);
				hpRect.pivot = new Vector2(0f, 0.5f);
				hpRect.anchoredPosition = new Vector2(3f, 1f);
				hpTexts.HP.alignment = TextAlignmentOptions.Left;
			}
			else
			{
				hpRect.anchorMin = hpRect.anchorMax = new Vector2(0.5f, 0.5f);
				hpRect.pivot = new Vector2(0.5f, 0.5f);
				hpRect.anchoredPosition = new Vector2(0f, 1f);
				hpTexts.HP.alignment = TextAlignmentOptions.Center;
			}

			RectTransform percentRect = hpTexts.HPPercent.rectTransform;

			if (showBoth)
			{
				percentRect.anchorMin = percentRect.anchorMax = new Vector2(1f, 0.5f);
				percentRect.pivot = new Vector2(1f, 0.5f);
				percentRect.anchoredPosition = new Vector2(-3f, 1f);
				hpTexts.HPPercent.alignment = TextAlignmentOptions.Right;
			}
			else
			{
				percentRect.anchorMin = percentRect.anchorMax = new Vector2(0.5f, 0.5f);
				percentRect.pivot = new Vector2(0.5f, 0.5f);
				percentRect.anchoredPosition = new Vector2(0f, 1f);
				hpTexts.HPPercent.alignment = TextAlignmentOptions.Center;
			}
		}

		private static void UpdateAlertAndName(Character character, object hudData, bool enabled)
		{
			RectTransform alerted = hud_m_alerted_Field?.GetValue(hudData) as RectTransform;
			RectTransform aware = hud_m_aware_Field?.GetValue(hudData) as RectTransform;

			if (enabled)
			{
				alerted?.gameObject.SetActive(false);
				aware?.gameObject.SetActive(false);
			}

			TextMeshProUGUI nameText = hud_m_name_Field?.GetValue(hudData) as TextMeshProUGUI;
			if (nameText == null) return;

			if (!enabled)
			{
				nameText.color = Color.white;
				return;
			}

			BaseAI ai = character.GetBaseAI();
			bool isAlerted = ai?.IsAlerted() ?? false;
			bool hasTarget = ai?.HaveTarget() ?? false;

			if (isAlerted)
			{
				nameText.color = Color.red;
			}
			else if (hasTarget)
			{
				nameText.color = Color.yellow;
			}
			else
			{
				nameText.color = Color.white;
			}
		}

		private static void UpdatePlayerBarColor(Character character, object hudData, bool enabled)
		{
			if (!enabled || !character.IsPlayer()) return;

			GuiBar fastBar = hud_m_healthFast_Field?.GetValue(hudData) as GuiBar;
			GuiBar fastFriendlyBar = hud_m_healthFastFriendly_Field?.GetValue(hudData) as GuiBar;

			Color color = character.IsPVPEnabled() ? Color.magenta : Color.green;

			fastBar?.SetColor(color);
			fastFriendlyBar?.SetColor(color);
		}

		private static void RestoreVanillaState(EnemyHud enemyHud)
		{
			if (enemyHud == null) return;

			if (defaultDistance >= 0f)
			{
				enemyHud.m_maxShowDistance = defaultDistance;
			}

			if (m_hudsField == null) return;

			IDictionary huds = m_hudsField.GetValue(enemyHud) as IDictionary;
			if (huds == null) return;

			foreach (DictionaryEntry entry in huds)
			{
				object hudData = entry.Value;
				if (hudData == null) continue;

				Character character = hud_m_character_Field?.GetValue(hudData) as Character;
				GameObject guiObject = hud_m_gui_Field?.GetValue(hudData) as GameObject;
				RectTransform healthTransform = guiObject?.transform.Find("Health") as RectTransform;

				GuiBar fastBar = hud_m_healthFast_Field?.GetValue(hudData) as GuiBar;
				GuiBar slowBar = hud_m_healthSlow_Field?.GetValue(hudData) as GuiBar;
				GuiBar fastFriendlyBar = hud_m_healthFastFriendly_Field?.GetValue(hudData) as GuiBar;

				if (character != null && healthTransform != null)
				{
					ApplyBarSettings(character, healthTransform, fastBar, slowBar, fastFriendlyBar, false);
				}

				if (hpTextCache.TryGetValue(hudData, out HpTexts hpTexts))
				{
					hpTexts.HP.gameObject.SetActive(false);
					hpTexts.HPPercent.gameObject.SetActive(false);
				}
			}

			log.Info("Restored vanilla enemy nameplate state.");
		}

		private static TextMeshProUGUI CreateHpTextObject(string name, GameObject parent)
		{
			EnsureHpTextResources();

			GameObject textObject = new GameObject(name);
			textObject.layer = 5;
			textObject.transform.SetParent(parent.transform, false);

			RectTransform rectTransform = textObject.AddComponent<RectTransform>();
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = new Vector2(100f, 14f);
			rectTransform.localScale = Vector3.one;

			TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
			text.color = Color.white;
			text.font = hpFont;
			text.fontSize = 11;
			text.alignment = TextAlignmentOptions.Center;
			text.text = "";
			text.raycastTarget = false;

			if (hpFontMaterial != null)
			{
				text.fontSharedMaterial = hpFontMaterial;
			}

			return text;
		}

		private static void EnsureHpTextResources()
		{
			if (hpFont == null)
			{
				hpFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(font => font.name == "Valheim-AveriaSansLibre");

				if (hpFont == null)
				{
					log.Warn("Could not find Valheim-AveriaSansLibre TMP font.");
					return;
				}
			}

			if (hpFontMaterial == null)
			{
				Material sourceMaterial = hpFont.material;

				if (sourceMaterial == null)
				{
					log.Warn("Could not find material for Valheim-AveriaSansLibre TMP font.");
					return;
				}

				hpFontMaterial = new Material(sourceMaterial);

				if (hpFontMaterial.HasProperty(ShaderUtilities.ID_OutlineWidth) &&
					hpFontMaterial.HasProperty(ShaderUtilities.ID_OutlineColor))
				{
					hpFontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.125f);
					hpFontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
				}
			}
		}
	}
}