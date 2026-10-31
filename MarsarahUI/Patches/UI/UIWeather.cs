using HarmonyLib;
using MarsarahUI.Managers;
using UnityEngine;
using UnityEngine.UI;
using MarsarahUI.Patches.Logic;

namespace MarsarahUI.Patches.UI
{
	internal class UIWeather : UIController
	{
		private static readonly LogManager log = new LogManager("UI Weather", LogManager.LogLevel.Warning);

		// UI data
		private static string UIForecastTimer;

		// UI elements
		private static Text UINextWeatherTimerText = null;
		private static Image UIForecastIcon = null;

		// Throttle
		private const float ForecastUpdateInterval = 1f;
		private static float nextForecastUpdateTime;

		[HarmonyPatch(typeof(EnvMan), "Update")]
		class Weather_EnvManPatch
		{
			private static void Prefix(EnvMan __instance, ref EnvSetup ___m_currentEnv, ref long ___m_environmentPeriod, ref double ___m_totalSeconds)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;
				if (!ConfigManager.EffectiveShowWeatherForecast) return;
				if (Player.m_localPlayer == null || WorldGenerator.instance == null) return;
				if (!ShouldShowWeatherUI()) return;

				if (Time.unscaledTime < nextForecastUpdateTime) return;

				nextForecastUpdateTime = Time.unscaledTime + ForecastUpdateInterval;

				UpdateForecastData(__instance, ___m_currentEnv, ___m_environmentPeriod, ___m_totalSeconds);
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		class Weather_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (ConfigManager.EffectiveShowWeatherForecast)
				{
					CreateUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		class Weather_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (!ConfigManager.EffectiveShowWeatherForecast)
				{
					if (UINextWeatherTimerText != null && UINextWeatherTimerText.enabled)
					{
						UINextWeatherTimerText.enabled = false;
					}

					if (UIForecastIcon != null)
					{
						if (UIForecastIcon.enabled)
						{
							UIForecastIcon.enabled = false;
						}

						if (UIForecastIcon.sprite != null)
						{
							UIForecastIcon.sprite = null;
						}
					}

					return;
				}

				CreateUI(__instance);

				bool showWeatherUI = ShouldShowWeatherUI();

				if (UIForecastIcon != null)
				{
					UIForecastIcon.enabled = showWeatherUI && UIForecastIcon.sprite != null;
				}

				if (UINextWeatherTimerText != null)
				{
					UINextWeatherTimerText.enabled = showWeatherUI;

					if (showWeatherUI)
					{
						UINextWeatherTimerText.text = UIForecastTimer;
						UINextWeatherTimerText.color = Color.white;
					}
				}
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UIForecastIcon != null && UINextWeatherTimerText != null)
				return; // UI already exists

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			Vector2 UIWeatherAreaSize = new Vector2(200f, 40f); // wider, since it holds all
			Vector2 UIWeatherAreaPos = new Vector2(-130f, -220f); // bottom-right corner

			// Parent container for the widget
			GameObject UIWeatherWidgetArea = new GameObject("WeatherWidgetArea");
			UIWeatherWidgetArea.layer = 5;
			UIWeatherWidgetArea.transform.SetParent(hud.m_rootObject.transform);
			RectTransform widgetTransform = UIWeatherWidgetArea.AddComponent<RectTransform>();
			widgetTransform.anchorMin = new Vector2(1f, 1f);
			widgetTransform.anchorMax = new Vector2(1f, 1f);
			widgetTransform.anchoredPosition = UIWeatherAreaPos;
			widgetTransform.sizeDelta = UIWeatherAreaSize;
			UIWeatherWidgetArea.transform.localScale = Vector3.one;

			// --- Forecast icon ---
			UIForecastIcon = CreateUIImageObject("ForecastWeatherIcon", UIWeatherWidgetArea, new Vector2(25f, 0f), new Vector2(30f, 30f));

			// --- Timer text ---
			UINextWeatherTimerText = CreateTextObject("WeatherTimerTMP", UIWeatherWidgetArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleLeft, new Vector2(80f, 0f), new Vector2(80f, 30f));
		}

		private static void UpdateForecastData(EnvMan envMan, EnvSetup currentEnv, long currentEnvironmentPeriod, double totalSeconds)
		{
			if (envMan == null || currentEnv == null || Player.m_localPlayer == null)
			{
				UIForecastTimer = "--:--";
				return;
			}

			Vector3 position = Player.m_localPlayer.transform.position;

			WeatherLogic.ForecastResult? result = WeatherLogic.CalculateForecast(envMan, currentEnv, currentEnvironmentPeriod, totalSeconds, position);

			if (!result.HasValue)
			{
				log.Warn("Could not determine biome sector for weather forecast.");
				UIForecastTimer = "--:--";
				return;
			}

			WeatherLogic.ForecastResult forecast = result.Value;
			Sprite iconSprite = null;

			if (WeatherLogic.TryGetIconKey(forecast.Biome, forecast.NormalizedWeatherName, out string iconKey))
			{
				string resourcePath = $"MarsarahUI.Assets.Icons.Weather.{iconKey}.png";
				iconSprite = IconManager.LoadEmbeddedIcon(resourcePath);

				if (iconSprite == null && forecast.HasUpcomingChange && forecast.HasChanged)
				{
					log.Warn($"Failed to load weather icon '{resourcePath}'.");
				}
			}
			else if (forecast.HasUpcomingChange && forecast.HasChanged)
			{
				log.Warn($"No weather icon mapping for biome '{forecast.Biome}' and environment '{forecast.NormalizedWeatherName}'.");
			}

			UIForecastTimer = forecast.Timer;

			if (UIForecastIcon != null)
			{
				UIForecastIcon.sprite = iconSprite;
				UIForecastIcon.enabled = iconSprite != null;
			}
		}

		private static bool ShouldShowWeatherUI()
		{
			if (!ShowUI) return false;
			if (Game.m_noMap) return true;

			return Minimap.instance != null &&
				   Minimap.instance.m_mapSmall != null &&
				   Minimap.instance.m_mapSmall.activeInHierarchy;
		}
	}
}