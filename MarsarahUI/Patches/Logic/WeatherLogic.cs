using System;
using System.Collections.Generic;

namespace MarsarahUI.Patches.Logic
{
	internal static class WeatherLogic
	{
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
	}
}