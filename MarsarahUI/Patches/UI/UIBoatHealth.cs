using MarsarahUI.Managers;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace MarsarahUI.Patches.UI
{
	internal class UIBoatHealth : UIController
	{
		private static readonly LogManager log = new LogManager("UI Boat Health", LogManager.LogLevel.Warning);

		private const float HealthAreaWidth = 72f;
		private const float HealthAreaHeight = 20f;
		private const float HealthBarWidthFull = 30f;
		private const float HealthBarWidthDamaged = 36f;
		private const float HealthBarHeight = 9f;
		private const float HealthOffsetBelowSpeed = -27f;
		private const float CriticalHealthThreshold = 0.3f;

		private static GameObject UIHealthArea;
		private static RectTransform healthAreaTransform;
		private static RectTransform healthFillTransform;
		private static RectTransform healthBackgroundTransform;
		private static Image healthFillImage;
		private static Text healthPercentText;
		private static bool warnedMissingHealth;
		private static Ship cachedShip;
		private static WearNTear cachedWear;

		internal static void UpdateUI(GameObject parent, Ship ship, bool enabled, bool showSpeed)
		{
			if (!enabled || parent == null || ship == null)
			{
				if (UIHealthArea != null) UIHealthArea.SetActive(false);
				return;
			}

			CreateUI(parent);
			if (UIHealthArea == null) return;

			if (cachedShip != ship || cachedWear == null)
			{
				cachedShip = ship;
				cachedWear = ship.GetComponent<WearNTear>();
				if (cachedWear == null) cachedWear = ship.GetComponentInChildren<WearNTear>();
			}

			if (cachedWear == null)
			{
				UIHealthArea.SetActive(false);
				if (!warnedMissingHealth)
				{
					log.Warn($"The controlled ship '{ship.gameObject.name}' has no WearNTear component; boat health cannot be displayed.");
					warnedMissingHealth = true;
				}
				return;
			}

			warnedMissingHealth = false;

			if (healthAreaTransform != null)
			{
				bool useCompactLayout = ConfigManager.EffectiveStatusEffectsUnderMinimap && !Game.m_noMap;
				float yOffset = useCompactLayout && !showSpeed ? 0f : HealthOffsetBelowSpeed;

				healthAreaTransform.anchoredPosition = new Vector2(-(HealthAreaWidth - UIBoatSpeed.BoatAreaWidth) * 0.5f, yOffset);
			}

			float health = Mathf.Clamp01(cachedWear.GetHealthPercentage());
			int healthPercentage = Mathf.RoundToInt(health * 100f);
			float barWidth = healthPercentage == 100 ? HealthBarWidthFull : HealthBarWidthDamaged;

			if (healthBackgroundTransform != null)
			{
				healthBackgroundTransform.anchoredPosition = new Vector2(-HealthAreaWidth * 0.5f + barWidth * 0.5f, 0f);
				healthBackgroundTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barWidth);
			}

			if (healthPercentText != null)
			{
				RectTransform percentTransform = healthPercentText.rectTransform;
				percentTransform.anchoredPosition = new Vector2(barWidth * 0.5f, 0f);
				percentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, HealthAreaWidth - barWidth);
			}

			float damageLevel = Mathf.Clamp01((1f - health) / (1f - CriticalHealthThreshold));
			Color fillColor = UIStyleManager.GetWeightFillColor(damageLevel);

			bool useDynamicColors = UIStyleManager.Current.WeightFillColorMode == UIColorMode.WeightGradient;
			Color textColor = useDynamicColors ? fillColor : UIStyleManager.Current.ValueTextColor;

			if (healthFillImage != null)
			{
				if (healthFillTransform != null)
				{
					float fillWidth = healthFillImage.type == Image.Type.Filled ? barWidth : barWidth * health;
					healthFillTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillWidth);
				}

				if (healthFillImage.type == Image.Type.Filled)
				{
					healthFillImage.fillAmount = health;
				}

				healthFillImage.color = fillColor;
			}

			if (healthPercentText != null)
			{
				healthPercentText.text = $"{healthPercentage}%";
				healthPercentText.color = textColor;
			}

			UIHealthArea.SetActive(true);
		}

		private static void CreateUI(GameObject parent)
		{
			if (UIHealthArea != null && UIHealthArea.transform.parent == parent.transform) return;

			int uiTextFontSize = 16;
			string uiTextFontName = "AveriaSansLibre-Bold";

			UIHealthArea = new GameObject("BoatHealthArea");
			UIHealthArea.layer = 5;
			UIHealthArea.transform.SetParent(parent.transform, false);

			healthAreaTransform = UIHealthArea.AddComponent<RectTransform>();
			healthAreaTransform.anchorMin = new Vector2(0.5f, 0.5f);
			healthAreaTransform.anchorMax = new Vector2(0.5f, 0.5f);
			healthAreaTransform.anchoredPosition = Vector2.zero;
			healthAreaTransform.sizeDelta = new Vector2(HealthAreaWidth, HealthAreaHeight);
			healthAreaTransform.localScale = Vector3.one;

			float barCenterX = -HealthAreaWidth * 0.5f + HealthBarWidthDamaged * 0.5f;
			Image barBackground = CreateUIImageObject("BoatHealthBackground", UIHealthArea, new Vector2(barCenterX, 0f), new Vector2(HealthBarWidthDamaged, HealthBarHeight));
			healthBackgroundTransform = barBackground.rectTransform;
			barBackground.color = new Color(0.12f, 0.12f, 0.12f, 0.78f);
			barBackground.raycastTarget = false;

			healthFillImage = CreateUIImageObject("BoatHealthFill", barBackground.gameObject, Vector2.zero, new Vector2(HealthBarWidthDamaged, HealthBarHeight));
			healthFillImage.raycastTarget = false;
			healthFillTransform = healthFillImage.rectTransform;
			healthFillTransform.anchorMin = new Vector2(0f, 0.5f);
			healthFillTransform.anchorMax = new Vector2(0f, 0.5f);
			healthFillTransform.pivot = new Vector2(0f, 0.5f);
			healthFillTransform.anchoredPosition = Vector2.zero;

			Sprite fillSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == "bar_monster_hp_5");

			if (fillSprite != null)
			{
				healthFillImage.sprite = fillSprite;
				healthFillImage.type = Image.Type.Filled;
				healthFillImage.fillMethod = Image.FillMethod.Horizontal;
				healthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
				healthFillImage.fillAmount = 1f;
			}
			else
			{
				log.Warn("Could not find the vanilla HP bar sprite. Using the default fill.");
			}

			// The percentage occupies the remaining space to the right of the bar.
			float percentCenterX = HealthBarWidthDamaged * 0.5f;
			healthPercentText = CreateTextObject("BoatHealthPercent", UIHealthArea, Color.white, uiTextFontName, uiTextFontSize, TextAnchor.MiddleRight, new Vector2(percentCenterX, 0f), new Vector2(HealthAreaWidth - HealthBarWidthDamaged, HealthAreaHeight));
			healthPercentText.raycastTarget = false;

			UIHealthArea.SetActive(false);
			log.Info("Created boat health indicator.");
		}
	}
}
