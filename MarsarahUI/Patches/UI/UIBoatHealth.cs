using MarsarahUI.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIBoatHealth : UIController
	{
		private static readonly LogManager log = new LogManager("UI Boat Health", LogManager.LogLevel.Warning);

		private const float HealthAreaWidth = 146f;
		private const float HealthAreaHeight = 22f;
		private const float HealthBarWidth = 105f;
		private const float HealthBarHeight = 9f;
		private const float HealthOffsetBelowSpeed = -27f;

		private static GameObject UIHealthArea;
		private static RectTransform healthAreaTransform;
		private static RectTransform healthFillTransform;
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

			EnsureUI(parent);
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
			Color healthColor = GetColorFromHealth(health);

			if (healthFillTransform != null)
			{
				healthFillTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, HealthBarWidth * health);
			}

			if (healthFillImage != null) healthFillImage.color = healthColor;

			if (healthPercentText != null)
			{
				healthPercentText.text = $"{Mathf.RoundToInt(health * 100f)}%";
				healthPercentText.color = healthColor;
			}

			UIHealthArea.SetActive(true);
		}

		private static Color GetColorFromHealth(float health)
		{
			if (health > 0.6f) return new Color(0.427f, 0.745f, 0.412f);
			if (health > 0.3f) return new Color(0.890f, 0.702f, 0.298f);
			return new Color(0.851f, 0.325f, 0.310f);
		}

		private static void EnsureUI(GameObject parent)
		{
			if (UIHealthArea != null && UIHealthArea.transform.parent == parent.transform) return;

			UIHealthArea = new GameObject("BoatHealthArea");
			UIHealthArea.layer = 5;
			UIHealthArea.transform.SetParent(parent.transform, false);

			healthAreaTransform = UIHealthArea.AddComponent<RectTransform>();
			healthAreaTransform.anchorMin = new Vector2(0.5f, 0.5f);
			healthAreaTransform.anchorMax = new Vector2(0.5f, 0.5f);
			healthAreaTransform.anchoredPosition = Vector2.zero;
			healthAreaTransform.sizeDelta = new Vector2(HealthAreaWidth, HealthAreaHeight);
			healthAreaTransform.localScale = Vector3.one;

			float barCenterX = -HealthAreaWidth * 0.5f + HealthBarWidth * 0.5f;
			Image barBackground = CreateUIImageObject("BoatHealthBackground", UIHealthArea, new Vector2(barCenterX, 0f), new Vector2(HealthBarWidth, HealthBarHeight));
			barBackground.color = new Color(0.12f, 0.12f, 0.12f, 0.78f);
			barBackground.raycastTarget = false;

			healthFillImage = CreateUIImageObject("BoatHealthFill", barBackground.gameObject, Vector2.zero, new Vector2(HealthBarWidth, HealthBarHeight));
			healthFillImage.raycastTarget = false;
			healthFillTransform = healthFillImage.rectTransform;
			healthFillTransform.anchorMin = new Vector2(0f, 0.5f);
			healthFillTransform.anchorMax = new Vector2(0f, 0.5f);
			healthFillTransform.pivot = new Vector2(0f, 0.5f);
			healthFillTransform.anchoredPosition = Vector2.zero;

			// The percentage occupies the remaining space to the right of the bar.
			float percentCenterX = HealthBarWidth * 0.5f;
			healthPercentText = CreateTextObject("BoatHealthPercent", UIHealthArea, Color.white, "AveriaSansLibre-Bold", 15, TextAnchor.MiddleRight, new Vector2(percentCenterX, 0f), new Vector2(HealthAreaWidth - HealthBarWidth, HealthAreaHeight));
			healthPercentText.raycastTarget = false;

			UIHealthArea.SetActive(false);
			log.Info("Created boat health indicator.");
		}
	}
}
