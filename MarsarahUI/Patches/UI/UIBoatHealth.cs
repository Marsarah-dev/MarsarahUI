
using MarsarahUI.Managers;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIBoatHealth : UIController
	{
		private static readonly LogManager log = new LogManager("UI Boat Health", LogManager.LogLevel.Warning);

		private const float HealthAreaWidth = UIBoatSpeed.BoatAreaWidth;
		private const float HealthAreaHeight = 20f;
		private const float HealthBarWidth = 46f;
		private const float HealthBarHeight = 11f;
		private const float HealthBarRightInset = 4f;
		private const float HealthOffsetBelowSpeed = -27f;
		private const float CriticalHealthThreshold = 0.3f;

		private const float BoatIconX = -23f;
		private const float BoatIconSize = 24f;

		private static GameObject UIHealthArea;
		private static RectTransform healthAreaTransform;
		private static RectTransform healthFillTransform;
		private static Image boatIcon;
		private static Image healthFillImage;
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

				healthAreaTransform.anchoredPosition = new Vector2(0f, yOffset);
			}

			float health = Mathf.Clamp01(cachedWear.GetHealthPercentage());
			float damageLevel = Mathf.Clamp01((1f - health) / (1f - CriticalHealthThreshold));
			Color fillColor = UIStyleManager.GetWeightFillColor(damageLevel);

			if (healthFillImage != null)
			{
				if (healthFillImage.type == Image.Type.Filled)
				{
					healthFillImage.fillAmount = health;
				}
				else if (healthFillTransform != null)
				{
					healthFillTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, HealthBarWidth * health);
				}

				healthFillImage.color = fillColor;
			}

			UIHealthArea.SetActive(true);
		}

		private static void CreateUI(GameObject parent)
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

			boatIcon = CreateUIImageObject("BoatHealthIcon", UIHealthArea, new Vector2(BoatIconX, 0f), new Vector2(BoatIconSize, BoatIconSize));
			boatIcon.preserveAspect = true;
			boatIcon.raycastTarget = false;

			float barCenterX = HealthAreaWidth * 0.5f - HealthBarWidth * 0.5f - HealthBarRightInset;

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

			UIHealthArea.SetActive(false);

			UIStyleManager.StyleChanged -= ApplyBoatIconStyle;
			UIStyleManager.StyleChanged += ApplyBoatIconStyle;

			ApplyBoatIconStyle();

			log.Info("Created boat health indicator.");
		}

		private static void ApplyBoatIconStyle()
		{
			if (boatIcon == null) return;

			string iconName = UIStyleManager.Current.BoatIcon;
			boatIcon.sprite = IconManager.LoadHudIcon(iconName);
			boatIcon.preserveAspect = true;

			if (boatIcon.sprite == null)
			{
				log.Warn($"Could not load boat icon '{iconName}'.");
			}
		}
	}
}
