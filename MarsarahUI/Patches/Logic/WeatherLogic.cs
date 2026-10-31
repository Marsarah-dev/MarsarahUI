using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using MarsarahUI.Managers;

namespace MarsarahUI.Patches.Logic
{
	internal static class WeatherLogic
	{
		private static readonly LogManager log = new LogManager("Weather Logic", LogManager.LogLevel.Warning);

		private const bool EnableFutureWeatherLogging = false;

		private static EnvSetup _lastForecastEnv = null;
		private static long _lastForecastPeriod = -1;
		private static string _lastEnvironmentSignature = string.Empty;
		private static readonly MethodInfo selectWeightedEnvironmentMethod = AccessTools.Method(typeof(EnvMan), "SelectWeightedEnvironment");

		private static readonly Dictionary<(Heightmap.Biome, string), string> weatherIcons = new Dictionary<(Heightmap.Biome, string), string>()
		{
			// Meadows
			{ (Heightmap.Biome.Meadows, "Clear"), "Clear"},
			{ (Heightmap.Biome.Meadows, "Rain"), "Rain"},
			{ (Heightmap.Biome.Meadows, "Misty"), "Misty"},
			{ (Heightmap.Biome.Meadows, "ThunderStorm"), "Thunderstorm"},
			{ (Heightmap.Biome.Meadows, "LightRain"), "LightRain"},
			{ (Heightmap.Biome.Meadows, "Heath clear"), "Clear" },   // Seasons
			{ (Heightmap.Biome.Meadows, "SwampRain"), "LightRain" }, // Seasons
			{ (Heightmap.Biome.Meadows, "Snow"), "Snow" },           // Seasons
			{ (Heightmap.Biome.Meadows, "SnowStorm"), "Snowstorm" }, // Seasons

			// BlackForest
			{ (Heightmap.Biome.BlackForest, "DeepForest Mist"), "Clear" },
			{ (Heightmap.Biome.BlackForest, "Rain"), "Rain" },
			{ (Heightmap.Biome.BlackForest, "Misty"), "Misty" },
			{ (Heightmap.Biome.BlackForest, "ThunderStorm"), "Thunderstorm" },
			{ (Heightmap.Biome.BlackForest, "Clear"), "Clear" },         // Seasons
			{ (Heightmap.Biome.BlackForest, "LightRain"), "LightRain" }, // Seasons
			{ (Heightmap.Biome.BlackForest, "SwampRain"), "LightRain" }, // Seasons
			{ (Heightmap.Biome.BlackForest, "Snow"), "Snow" },           // Seasons
			{ (Heightmap.Biome.BlackForest, "SnowStorm"), "Snowstorm" }, // Seasons

			// Swamp
			{ (Heightmap.Biome.Swamp, "SwampRain"), "LightRain" },
			{ (Heightmap.Biome.Swamp, "ThunderStorm"), "Thunderstorm" }, // Seasons
			{ (Heightmap.Biome.Swamp, "Snow"), "Snow" },                 // Seasons
			{ (Heightmap.Biome.Swamp, "SnowStorm"), "Snowstorm" },       // Seasons

			// Mountain
			{ (Heightmap.Biome.Mountain, "SnowStorm"), "Snowstorm" },
			{ (Heightmap.Biome.Mountain, "Snow"), "Snow" },
			{ (Heightmap.Biome.Mountain, "Clear"), "Clear" }, // Seasons

			// Plains
			{ (Heightmap.Biome.Plains, "Heath clear"), "Clear" },
			{ (Heightmap.Biome.Plains, "Misty"), "Misty" },
			{ (Heightmap.Biome.Plains, "LightRain"), "LightRain" },
			{ (Heightmap.Biome.Plains, "Rain"), "Rain" },                 // Seasons
			{ (Heightmap.Biome.Plains, "ThunderStorm"), "Thunderstorm" }, // Seasons
			{ (Heightmap.Biome.Plains, "SwampRain"), "LightRain" },       // Seasons
			{ (Heightmap.Biome.Plains, "Snow"), "Snow" },                 // Seasons
			{ (Heightmap.Biome.Plains, "SnowStorm"), "Snowstorm" },       // Seasons

			// Mistlands
			{ (Heightmap.Biome.Mistlands, "Mistlands_clear"), "Clear" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_rain"), "Rain" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_thunder"), "Thunderstorm" },
			{ (Heightmap.Biome.Mistlands, "Heath clear"), "Clear" },     // Seasons
			{ (Heightmap.Biome.Mistlands, "DeepForest Mist"), "Clear" }, // Seasons
			{ (Heightmap.Biome.Mistlands, "SwampRain"), "LightRain" },   // Seasons
			{ (Heightmap.Biome.Mistlands, "Snow"), "Snow" },             // Seasons
			{ (Heightmap.Biome.Mistlands, "SnowStorm"), "Snowstorm" },   // Seasons

			// AshLands
			{ (Heightmap.Biome.AshLands, "Ashlands_ashrain"), "AshRain" },
			{ (Heightmap.Biome.AshLands, "Ashlands_misty"), "AshMist" },
			{ (Heightmap.Biome.AshLands, "Ashlands_CinderRain"), "AshCinderRain" },
			{ (Heightmap.Biome.AshLands, "Ashlands_storm"), "AshStorm" },

			// DeepNorth
			{ (Heightmap.Biome.DeepNorth, "Twilight_SnowStorm"), "Snowstorm" },
			{ (Heightmap.Biome.DeepNorth, "Twilight_Snow"), "Snow" },
			{ (Heightmap.Biome.DeepNorth, "Twilight_Clear"), "Clear" },

			// Ocean
			{ (Heightmap.Biome.Ocean, "Clear"), "Clear" },
			{ (Heightmap.Biome.Ocean, "Rain"), "Rain" },
			{ (Heightmap.Biome.Ocean, "LightRain"), "LightRain" },
			{ (Heightmap.Biome.Ocean, "Misty"), "Misty" },
			{ (Heightmap.Biome.Ocean, "ThunderStorm"), "Thunderstorm" },
			{ (Heightmap.Biome.Ocean, "Ashlands_SeaStorm"), "AshStorm" },
			{ (Heightmap.Biome.Ocean, "Snow"), "Snow" },           // Seasons
			{ (Heightmap.Biome.Ocean, "SnowStorm"), "Snowstorm" }  // Seasons
		};

		internal static bool TryGetIconKey(Heightmap.Biome biome, string normalizedWeatherName, out string iconKey)
		{
			return weatherIcons.TryGetValue((biome, normalizedWeatherName), out iconKey);
		}

		internal static string NormalizeSeasonSuffix(string env)
		{
			// Remove seasonal suffixes
			env = env.Replace(" Summer", "")
					 .Replace(" Fall", "")
					 .Replace(" Winter", "");

			return env;
		}

		internal static string NormalizeWeatherName(Heightmap.Biome biome, string env)
		{
			// Seasonality "warm" snow variants
			if (env == "WarmSnow")
				return "Snow";

			if (env == "WarmSnowStorm")
				return "SnowStorm";

			// Seasons: winter "rain" behaves like snow in most biomes
			if (env.EndsWith(" Winter", StringComparison.Ordinal))
			{
				bool isSnowInsteadOfRain =
					biome != Heightmap.Biome.Mountain &&
					biome != Heightmap.Biome.AshLands &&
					biome != Heightmap.Biome.DeepNorth;

				if (isSnowInsteadOfRain)
				{
					if (env.StartsWith("LightRain", StringComparison.Ordinal) || env.StartsWith("Rain", StringComparison.Ordinal))
					{
						// LightRain Winter / Rain Winter → light snow
						return "Snow";
					}

					if (env.StartsWith("ThunderStorm", StringComparison.Ordinal))
					{
						// ThunderStorm Winter → heavy snow
						return "SnowStorm";
					}
				}
			}

			// Seasons: Swamp Summer special case
			// Handle before stripping the suffix, because NormalizeSeasonSuffix would
			// turn "Swamp Summer" into just "Swamp".
			if (env == "Swamp Summer")
				env = "SwampRain";

			// Strip generic seasonal suffixes (" Summer", " Fall", " Winter")
			env = NormalizeSeasonSuffix(env);

			// Twilight variants
			// DeepNorth uses Twilight_* as its canonical environment names in vanilla so we don’t normalize there.
			if (biome != Heightmap.Biome.DeepNorth)
			{
				if (env == "Twilight_Snow")
					env = "Snow";
				else if (env == "Twilight_SnowStorm")
					env = "SnowStorm";
				else if (env == "Twilight_Clear")
					env = "Clear";
			}

			return env;
		}

		// Forecast helpers
		internal readonly struct ForecastResult
		{
			internal readonly Heightmap.Biome Biome;
			internal readonly string NormalizedWeatherName;
			internal readonly string Timer;
			internal readonly bool HasUpcomingChange;
			internal readonly bool HasChanged;

			internal ForecastResult(Heightmap.Biome biome, string normalizedWeatherName, string timer, bool hasUpcomingChange, bool hasChanged)
			{
				Biome = biome;
				NormalizedWeatherName = normalizedWeatherName;
				Timer = timer;
				HasUpcomingChange = hasUpcomingChange;
				HasChanged = hasChanged;
			}
		}

		internal static ForecastResult? CalculateForecast(EnvMan envMan, EnvSetup currentEnv, long currentEnvironmentPeriod, double totalSeconds, Vector3 position)
		{
			BiomeSector biomeSector = WorldGenerator.instance != null ? WorldGenerator.instance.GetBiomeSector(position, false) : null;

			if (biomeSector == null)
				return null;

			Heightmap.Biome currentBiome = biomeSector.Biome;

			bool isAshlands = WorldGenerator.IsAshlands(position.x, position.z);
			bool isDeepNorth = WorldGenerator.IsDeepnorth(position.x, position.z);

			// Track runtime changes to available weather weights.
			bool availableEnvironmentsChanged = HasAvailableEnvironmentsChanged(envMan, biomeSector);

			EnvSetup forecastEnv = null;
			long forecastPeriod = -1;

			// Look ahead up to 50 periods.
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
				// No upcoming change within 50 periods.
				return new ForecastResult(
					currentBiome,
					NormalizeWeatherName(currentBiome, currentEnv.m_name),
					"--:--",
					false,
					false);
			}

			string normalizedForecastName = NormalizeWeatherName(currentBiome, forecastEnv.m_name);
			string timer = GetNextWeatherTimer(forecastPeriod, totalSeconds);

			bool forecastChanged = availableEnvironmentsChanged || forecastEnv != _lastForecastEnv || forecastPeriod != _lastForecastPeriod;

			if (EnableFutureWeatherLogging && forecastChanged)
			{
				LogFutureWeatherSequence(currentEnvironmentPeriod, biomeSector, isAshlands, isDeepNorth, currentBiome, currentEnv, forecastEnv, timer);
			}

			_lastForecastEnv = forecastEnv;
			_lastForecastPeriod = forecastPeriod;

			return new ForecastResult(
				currentBiome,
				normalizedForecastName,
				timer,
				true,
				forecastChanged);
		}

		private static void LogFutureWeatherSequence(long currentEnvironmentPeriod, BiomeSector biomeSector, bool isAshlands, bool isDeepNorth, Heightmap.Biome currentBiome, EnvSetup currentEnv, EnvSetup forecastEnv, string timer)
		{
			string normalizedCurrentName = NormalizeWeatherName(currentBiome, currentEnv.m_name);
			string normalizedForecastName = NormalizeWeatherName(currentBiome, forecastEnv.m_name);

			StringBuilder sequenceLog = new StringBuilder();
			sequenceLog.AppendLine($"Next 50 forecast environments for {currentBiome}:");

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
			log.Info($"Next forecast: {forecastEnv.m_name} (normalized: {normalizedForecastName}), ETA {timer}");
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
	}
}