using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static MarsarahUI.Managers.ConfigManager;
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

		// Cache
		private static EnvSetup _lastForecastEnv = null;
		private static long _lastForecastPeriod = -1;
		private static string _lastEnvironmentSignature = string.Empty;
		private static readonly MethodInfo selectWeightedEnvironmentMethod = AccessTools.Method(typeof(EnvMan), "SelectWeightedEnvironment");

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

			// Player position → biome context
			Vector3 position = Player.m_localPlayer.transform.position;
			BiomeSector biomeSector = WorldGenerator.instance != null ? WorldGenerator.instance.GetBiomeSector(position, false) : null;

			if (biomeSector == null)
			{
				log.Warn("Could not determine biome sector for weather forecast.");
				UIForecastTimer = "--:--";
				return;
			}

			Heightmap.Biome currentBiome = biomeSector.Biome;

			bool isAshlands = WorldGenerator.IsAshlands(position.x, position.z);
			bool isDeepNorth = WorldGenerator.IsDeepnorth(position.x, position.z);

			// Track changes to the resolved weather list so diagnostics can also
			// react to runtime weight changes such as Clearer Weather.
			bool availableEnvironmentsChanged = HasAvailableEnvironmentsChanged(envMan, biomeSector);

			EnvSetup forecastEnv = null;
			long forecastPeriod = -1;

			// Look ahead up to 50 periods
			for (int i = 1; i <= 50; i++)
			{
				long periodToCheck = currentEnvironmentPeriod + i;
				EnvSetup nextEnv = GetEnvironment(periodToCheck, biomeSector, isAshlands, isDeepNorth);

				if (nextEnv != null && nextEnv.m_name != currentEnv.m_name)
				{
					forecastEnv = nextEnv;
					forecastPeriod = periodToCheck;
					break;
				}
			}

			if (forecastEnv == null)
			{
				// No upcoming weather change within 50 periods
				// → show the current environment icon and a neutral timer ("--:--")
				Sprite iconSpriteCurrent = null;
				string currentNameNormalized = WeatherLogic.NormalizeWeatherName(currentBiome, currentEnv.m_name);

				if (WeatherLogic.TryGetIconKey(currentBiome, currentNameNormalized, out string iconKeyCurrent))
				{
					string resourcePath = $"MarsarahUI.Assets.Icons.Weather.{iconKeyCurrent}.png";
					iconSpriteCurrent = IconManager.LoadEmbeddedIcon(resourcePath);
				}

				if (UIForecastIcon != null)
				{
					UIForecastIcon.sprite = iconSpriteCurrent;
					UIForecastIcon.enabled = iconSpriteCurrent != null;
				}

				UIForecastTimer = "--:--";
				return;
			}

			string normalizedForecastName = WeatherLogic.NormalizeWeatherName(currentBiome, forecastEnv.m_name);
			string timerStr = GetNextWeatherTimer(forecastPeriod, totalSeconds);
			bool forecastChanged = availableEnvironmentsChanged || forecastEnv != _lastForecastEnv || forecastPeriod != _lastForecastPeriod;

			// Pick icon
			Sprite iconSprite = null;

			if (WeatherLogic.TryGetIconKey(currentBiome, normalizedForecastName, out string iconKey))
			{
				string resourcePath = $"MarsarahUI.Assets.Icons.Weather.{iconKey}.png";
				iconSprite = IconManager.LoadEmbeddedIcon(resourcePath);

				if (iconSprite == null && forecastChanged)
				{
					log.Warn($"Failed to load weather icon '{resourcePath}'.");
				}
			}
			else if (forecastChanged)
			{
				log.Warn($"No weather icon mapping for biome '{currentBiome}' and environment '{normalizedForecastName}'.");
			}

			// Assign globals
			UIForecastTimer = timerStr;

			// Update UI element
			if (UIForecastIcon != null)
			{
				UIForecastIcon.sprite = iconSprite;
				UIForecastIcon.enabled = iconSprite != null;
			}

			// Debug: print the next 50 deterministic weather periods.
			/*if (forecastChanged)
			{				
				string normalizedCurrentName = NormalizeWeatherName(currentBiome, currentEnv.m_name);
								
				StringBuilder sequenceLog = new StringBuilder();
				sequenceLog.AppendLine("Next 50 forecast environments:");

				for (int i = 1; i <= 50; i++)
				{
					long periodToCheck = currentEnvironmentPeriod + i;
					EnvSetup nextEnv = GetEnvironment(periodToCheck, biomeSector, isAshlands, isDeepNorth);

					if (nextEnv == null)
					{
						sequenceLog.AppendLine($"  +{i,2} → null (normalized: -)");
						continue;
					}

					string rawName = nextEnv.m_name;
					string normalizedNextName = NormalizeWeatherName(currentBiome, rawName);

					sequenceLog.AppendLine($"  +{i,2} → {rawName} (normalized: {normalizedNextName})");
				}

				log.Info(sequenceLog.ToString());
				log.Info($"Current weather: {currentEnv.m_name} (normalized: {normalizedCurrentName}), biome={currentBiome}");
				log.Info($"Next forecast: {forecastEnv.m_name} (normalized: {normalizedForecastName}), ETA {timerStr}");
			}*/

			_lastForecastEnv = forecastEnv;
			_lastForecastPeriod = forecastPeriod;
		}

		private static bool HasAvailableEnvironmentsChanged(EnvMan envMan, BiomeSector biomeSector)
		{
			List<EnvEntry> availableEnvironments = envMan.GetAvailableEnvironments(biomeSector);

			if (availableEnvironments == null)
				return false;

			StringBuilder signature = new StringBuilder();
			signature.Append($"{biomeSector.Biome}|");

			foreach (EnvEntry entry in availableEnvironments)
			{
				if (entry?.m_env == null)
					continue;

				signature.Append($"{entry.m_env.m_name}:{entry.m_weight}:{entry.m_ashlandsOverride}:{entry.m_deepnorthOverride}|");
			}

			string currentSignature = signature.ToString();

			if (currentSignature == _lastEnvironmentSignature)
				return false;

			_lastEnvironmentSignature = currentSignature;
			return true;
		}

		private static EnvSetup GetEnvironment(long period, BiomeSector biomeSector, bool isAshlands, bool isDeepNorth)
		{
			// Save RNG state
			UnityEngine.Random.State state = UnityEngine.Random.state;

			// Deterministic seed
			UnityEngine.Random.InitState((int)period);

			EnvSetup result = null;

			try
			{
				EnvMan envMan = EnvMan.instance;

				if (envMan != null)
				{
					// Get all the available weathers for the given biome sector
					List<EnvEntry> availableEnvironments = envMan.GetAvailableEnvironments(biomeSector);

					if (availableEnvironments != null && availableEnvironments.Count > 0)
					{
						// From the list of available weathers, select one based on weights
						if (selectWeightedEnvironmentMethod != null)
						{
							result = selectWeightedEnvironmentMethod.Invoke(envMan, new object[] { availableEnvironments }) as EnvSetup;
						}

						// Apply Ashlands / DeepNorth overrides
						foreach (EnvEntry entry in availableEnvironments)
						{
							if (entry == null)
								continue;

							if (entry.m_ashlandsOverride && isAshlands)
								result = entry.m_env;

							if (entry.m_deepnorthOverride && isDeepNorth)
								result = entry.m_env;
						}
					}
				}
			}
			finally
			{
				// Restore RNG state
				UnityEngine.Random.state = state;
			}

			return result;
		}

		// Returns a timer string until the given forecast period occurs.
		private static string GetNextWeatherTimer(long forecastPeriod, double totalSecondsToNow)
		{
			EnvMan envMan = EnvMan.instance;
			if (envMan == null) return "";

			// Period length in seconds (derived from EnvMan constant)
			float periodLength = envMan.m_environmentDuration;
			double forecastTime = forecastPeriod * periodLength;

			if (forecastTime <= totalSecondsToNow)
				return "0:00";

			double secondsLeft = forecastTime - totalSecondsToNow;
			TimeSpan ts = TimeSpan.FromSeconds(secondsLeft);

			if (ts.TotalHours >= 1.0)
				return $"{(int)ts.TotalHours}:{ts.Minutes:D2}h";

			return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
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