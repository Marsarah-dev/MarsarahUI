using HarmonyLib;
using MarsarahUI.Managers;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIInventoryWeightAndSlots : UIController
	{
		private static readonly LogManager log = new LogManager("UI Inventory", LogManager.LogLevel.Warning);

		private static float currentWeight;
		private static float maxWeight;
		private static float freeSlots;
		private static float freeSlotsPercent;

		internal static GameObject UIWeightBarArea;
		private static GameObject UIWeightBarEmojiArea;
		private static Image weightBarFill;
		private static Text UIWeightBarText;
		private static TextMeshProUGUI UIWeightBarEmojiTMP;

		internal static GameObject UISlotsArea;
		private static Text UISlotsText;
		private static TextMeshProUGUI UISlotsEmojiTMP;

		[HarmonyPatch(typeof(Player), "Update")]
		private static class InventoryWeightAndSlots_PlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_localPlayer == null) return;
				if (!ConfigManager.EffectiveShowInventoryWeightAndSlots) return;

				Inventory inventory = ___m_localPlayer.GetInventory();

				currentWeight = inventory.GetTotalWeight();
				maxWeight = ___m_localPlayer.GetMaxCarryWeight();
				freeSlots = inventory.GetEmptySlots();
				freeSlotsPercent = inventory.SlotsUsedPercentage();
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class InventoryWeightAndSlots_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (ConfigManager.EffectiveShowInventoryWeightAndSlots)
				{
					CreateUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class InventoryWeightAndSlots_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (!ConfigManager.EffectiveShowInventoryWeightAndSlots)
				{
					SetUIActive(false);
					return;
				}

				CreateUI(__instance);

				bool shouldBeVisible = ShowUI;

				SetUIActive(shouldBeVisible);

				if (!shouldBeVisible || weightBarFill == null) return;

				float weightPercent = maxWeight > 0f
					? Mathf.Clamp01(currentWeight / maxWeight)
					: 0f;

				weightBarFill.fillAmount = weightPercent;

				Color barColor = GetColorBlendFromPercent(weightPercent);
				weightBarFill.color = barColor;

				if (UIWeightBarText != null)
				{
					UIWeightBarText.text = $"{currentWeight:0.0}/{maxWeight:0}";
				}

				if (UIWeightBarEmojiTMP != null)
				{
					UIWeightBarEmojiTMP.text = "🏋️";
					UIWeightBarEmojiTMP.color = barColor;
				}

				Color slotsColor = GetColorFromPercent(freeSlotsPercent);

				if (UISlotsText != null)
				{
					UISlotsText.color = slotsColor;
					UISlotsText.text = freeSlots.ToString();
				}

				if (UISlotsEmojiTMP != null)
				{
					UISlotsEmojiTMP.color = slotsColor;
					UISlotsEmojiTMP.text = "🎒";
				}
			}
		}

		private static void SetUIActive(bool active)
		{
			UIWeightBarArea?.SetActive(active);
			UIWeightBarEmojiArea?.SetActive(active);
			UISlotsArea?.SetActive(active);
		}

		private static Color GetColorFromPercent(float percent)
		{
			if (percent < 33f) return Color.green;
			if (percent < 66f) return Color.yellow;
			if (percent < 100f) return new Color(1f, 0.549019f, 0f);

			return Color.red;
		}

		private static Color GetColorBlendFromPercent(float percent)
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

		private static void CreateUI(Hud hud)
		{
			if (UIWeightBarArea != null) return;

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			string UIEmojiFontName = "NotoEmoji-Regular SDF";

			Vector2 UIWeightAreaSize = new Vector2(100f, 30f);
			Vector2 UIWeightAreaEmojiSize = new Vector2(30f, 30f);

			float xOffset = -10f;
			float yOffset = -230f;

			UIWeightBarArea = new GameObject("WeightAreaBar");
			UIWeightBarArea.layer = 5;
			UIWeightBarArea.transform.SetParent(hud.m_healthPanel.transform);

			RectTransform weightAreaTransform = UIWeightBarArea.AddComponent<RectTransform>();
			weightAreaTransform.anchorMin = new Vector2(1f, 1f);
			weightAreaTransform.anchorMax = new Vector2(1f, 1f);
			weightAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset);
			weightAreaTransform.sizeDelta = UIWeightAreaSize;
			UIWeightBarArea.transform.localScale = Vector3.one;

			GameObject weightBackgroundArea = new GameObject("WeightBarBackground");
			weightBackgroundArea.transform.SetParent(UIWeightBarArea.transform, false);

			Image weightBarBackground = weightBackgroundArea.AddComponent<Image>();

			RectTransform backgroundRect = weightBackgroundArea.GetComponent<RectTransform>();
			backgroundRect.anchorMin = Vector2.zero;
			backgroundRect.anchorMax = Vector2.one;
			backgroundRect.offsetMin = Vector2.zero;
			backgroundRect.offsetMax = Vector2.zero;

			weightBarBackground.color = new Color(0f, 0f, 0f, 0.4f);

			GameObject fillArea = new GameObject("WeightBarFill");
			fillArea.transform.SetParent(UIWeightBarArea.transform, false);

			weightBarFill = fillArea.AddComponent<Image>();

			RectTransform fillRect = fillArea.GetComponent<RectTransform>();
			fillRect.anchorMin = Vector2.zero;
			fillRect.anchorMax = Vector2.one;
			fillRect.offsetMin = new Vector2(3f, 3f);
			fillRect.offsetMax = new Vector2(-3f, -3f);

			Sprite barSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == "bar_monster_hp_5");

			weightBarFill.sprite = barSprite;
			weightBarFill.type = Image.Type.Filled;
			weightBarFill.fillMethod = Image.FillMethod.Horizontal;
			weightBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
			weightBarFill.fillAmount = 0f;

			UIWeightBarText = CreateTextObject("WeightText", UIWeightBarArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, Vector2.zero, UIWeightAreaSize);

			UIWeightBarEmojiArea = new GameObject("WeightAreaEmoji");
			UIWeightBarEmojiArea.layer = 5;
			UIWeightBarEmojiArea.transform.SetParent(hud.m_healthPanel.transform);

			xOffset -= 65f;

			RectTransform emojiAreaTransform = UIWeightBarEmojiArea.AddComponent<RectTransform>();
			emojiAreaTransform.anchorMin = new Vector2(1f, 1f);
			emojiAreaTransform.anchorMax = new Vector2(1f, 1f);
			emojiAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset);
			emojiAreaTransform.sizeDelta = UIWeightAreaEmojiSize;
			UIWeightBarEmojiArea.transform.localScale = Vector3.one;

			GameObject emojiBackgroundArea = new GameObject("WeightEmojiBackground");
			emojiBackgroundArea.transform.SetParent(UIWeightBarEmojiArea.transform, false);

			Image emojiBackground = emojiBackgroundArea.AddComponent<Image>();

			RectTransform emojiBackgroundRect = emojiBackgroundArea.GetComponent<RectTransform>();
			emojiBackgroundRect.anchorMin = Vector2.zero;
			emojiBackgroundRect.anchorMax = Vector2.one;
			emojiBackgroundRect.offsetMin = Vector2.zero;
			emojiBackgroundRect.offsetMax = Vector2.zero;

			emojiBackground.color = new Color(0f, 0f, 0f, 0.4f);

			UIWeightBarEmojiTMP = CreateTMPTextObject("WeightEmojiTMP", UIWeightBarEmojiArea, Color.green, UIEmojiFontName, UITextFontSize + 4, TextAlignmentOptions.Midline, Vector2.zero, UIWeightAreaEmojiSize, log);

			Vector2 UISlotsAreaSize = new Vector2(50f, 30f);
			xOffset = 68f;

			UISlotsArea = new GameObject("SlotsArea");
			UISlotsArea.layer = 5;
			UISlotsArea.transform.SetParent(hud.m_healthPanel.transform);

			RectTransform slotsAreaTransform = UISlotsArea.AddComponent<RectTransform>();
			slotsAreaTransform.anchorMin = new Vector2(1f, 1f);
			slotsAreaTransform.anchorMax = new Vector2(1f, 1f);
			slotsAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset);
			slotsAreaTransform.sizeDelta = UISlotsAreaSize;
			UISlotsArea.transform.localScale = Vector3.one;

			Sprite slotsBackgroundSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == "InputFieldBackground");

			Image slotsAreaBackground = UISlotsArea.AddComponent<Image>();
			slotsAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
			slotsAreaBackground.sprite = slotsBackgroundSprite;
			slotsAreaBackground.type = Image.Type.Sliced;

			UISlotsText = CreateTextObject("SlotsText", UISlotsArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-4f, 0f), UISlotsAreaSize);
			UISlotsEmojiTMP = CreateTMPTextObject("SlotsEmojiTMP", UISlotsArea, Color.green, UIEmojiFontName, UITextFontSize + 4, TextAlignmentOptions.MidlineLeft, new Vector2(4f, 0f), UISlotsAreaSize, log);
		}
	}
}