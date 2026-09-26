using HarmonyLib;
using MarsarahUI.Managers;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIAshlandsHeatLevel : UIController
	{
		private static readonly LogManager log = new LogManager("UI Ashlands Heat", LogManager.LogLevel.Warning);

		private static Image heatBarFill;
		private static Image heatBarBackground;
		private static GameObject UIHeatBarArea;
		private static Text heatBarText;
		private static TextMeshProUGUI heatBarEmojiTMP;

		private static readonly AccessTools.FieldRef<Player, float> lavaHeatLevelRef = AccessTools.FieldRefAccess<Player, float>("m_lavaHeatLevel");
		private static readonly AccessTools.FieldRef<Player, float> ashlandsOceanHeatLevelRef =	AccessTools.FieldRefAccess<Player, float>("m_ashlandsOceanHeatLevel");


		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class HeatLevel_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (ConfigManager.EffectiveShowHeatLevelInAshlands)
				{
					CreateUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class HeatLevel_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (!ConfigManager.EffectiveShowHeatLevelInAshlands)
				{
					if (UIHeatBarArea != null)
					{
						UIHeatBarArea.SetActive(false);
					}

					return;
				}

				CreateUI(__instance);

				Player player = Player.m_localPlayer;
				if (player == null || heatBarFill == null) return;

				float heatThreshold = player.m_heatLevelFirstDamageThreshold;
				if (heatThreshold <= 0f) return;

				float lavaHeatLevel = lavaHeatLevelRef(player);
				float ashlandsOceanHeatLevel = ashlandsOceanHeatLevelRef(player);

				bool inAshlandsWater = player.InWater() && WorldGenerator.GetAshlandsOceanGradient(player.transform.position) >= 0f;

				float currentHeat = inAshlandsWater	? Mathf.Max(lavaHeatLevel, ashlandsOceanHeatLevel) : lavaHeatLevel;

				bool shouldBeVisible = ShowUI && !IsUIHidden() && !IsLoadScreenActive(__instance);

				if (UIHeatBarArea.activeSelf != shouldBeVisible)
				{
					UIHeatBarArea.SetActive(shouldBeVisible);
				}

				if (!shouldBeVisible) return;

				heatBarFill.fillAmount = Mathf.Clamp01(currentHeat / heatThreshold);

				float heatPercent = heatBarFill.fillAmount;
				heatBarText.text = $"{heatPercent * 100f:0}%";

				Color startColor = Color.yellow;
				Color midColor = new Color(1f, 0.549019f, 0f);
				Color endColor = Color.red;

				Color barColor = heatPercent < 0.5f
					? Color.Lerp(startColor, midColor, heatPercent * 2f)
					: Color.Lerp(midColor, endColor, (heatPercent - 0.5f) * 2f);

				if (heatPercent > 0.8f)
				{
					barColor.a = 0.6f + 0.4f * Mathf.Sin(Time.time * 4f);
				}
				else
				{
					barColor.a = 0.8f;
				}

				heatBarFill.color = barColor;

				bool hasHeat = heatBarFill.fillAmount > 0f;

				heatBarFill.enabled = hasHeat;
				heatBarBackground.enabled = hasHeat;
				heatBarText.enabled = hasHeat;
				heatBarEmojiTMP.enabled = hasHeat;
				heatBarEmojiTMP.text = hasHeat ? "🔥" : "";
			}

			private static bool IsLoadScreenActive(Hud hud)
			{
				return Hud.instance && hud.m_loadingScreen && hud.m_loadingScreen.gameObject.activeSelf;
			}

			private static bool IsUIHidden()
			{
				return Hud.IsUserHidden();
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UIHeatBarArea != null) return;

			Vector2 UIHeatAreaSize = new Vector2(200f, 25f);
			int UITextFontSize = 13;
			int UIEmojiFontSize = 10;
			string UITextFontName = "AveriaSansLibre-Bold";
			string UIEmojiFontName = "NotoEmoji-Regular SDF";

			UIHeatBarArea = new GameObject("HeatBar");
			UIHeatBarArea.layer = 5;
			UIHeatBarArea.transform.SetParent(hud.m_rootObject.transform.parent, false);

			RectTransform heatAreaTransform = UIHeatBarArea.AddComponent<RectTransform>();
			heatAreaTransform.anchorMin = new Vector2(0.5f, 0.5f);
			heatAreaTransform.anchorMax = new Vector2(0.5f, 0.5f);
			heatAreaTransform.pivot = new Vector2(0.5f, 0.5f);
			heatAreaTransform.anchoredPosition = new Vector2(0f, 350f);
			heatAreaTransform.sizeDelta = UIHeatAreaSize;
			UIHeatBarArea.transform.localScale = Vector3.one;

			GameObject heatBackgroundArea = new GameObject("HeatBarBackground");
			heatBackgroundArea.transform.SetParent(UIHeatBarArea.transform, false);

			heatBarBackground = heatBackgroundArea.AddComponent<Image>();

			RectTransform backgroundRect = heatBackgroundArea.GetComponent<RectTransform>();
			backgroundRect.anchorMin = Vector2.zero;
			backgroundRect.anchorMax = Vector2.one;
			backgroundRect.offsetMin = Vector2.zero;
			backgroundRect.offsetMax = Vector2.zero;

			heatBarBackground.color = new Color(0f, 0f, 0f, 0.4f);
			heatBarBackground.enabled = false;

			GameObject fillArea = new GameObject("HeatBarFill");
			fillArea.transform.SetParent(UIHeatBarArea.transform, false);

			heatBarFill = fillArea.AddComponent<Image>();

			RectTransform fillRect = fillArea.GetComponent<RectTransform>();
			fillRect.anchorMin = Vector2.zero;
			fillRect.anchorMax = Vector2.one;
			fillRect.offsetMin = new Vector2(3f, 3f);
			fillRect.offsetMax = new Vector2(-3f, -3f);

			Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(tempSprite => tempSprite.name == "bar_monster_hp_5");

			heatBarFill.sprite = sprite;
			heatBarFill.type = Image.Type.Filled;
			heatBarFill.fillMethod = Image.FillMethod.Horizontal;
			heatBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
			heatBarFill.fillAmount = 0f;
			heatBarFill.enabled = false;

			heatBarText = CreateTextObject("HeatText", UIHeatBarArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, Vector2.zero, UIHeatAreaSize);
			heatBarEmojiTMP = CreateTMPTextObject("HeatEmojiTMP", UIHeatBarArea, Color.red, UIEmojiFontName, UIEmojiFontSize, TextAlignmentOptions.MidlineRight, new Vector2(-2f, 0f), UIHeatAreaSize, log);
		}
	}
}