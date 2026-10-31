
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahUI.Managers
{
	internal static class PortalBiomeImageManager
	{
		private static readonly LogManager log = new LogManager("Portal Biome Images", LogManager.LogLevel.Info);

		private const string BiomeImageResourcePrefix = "MarsarahUI.Assets.Portals.Biomes.";

		private sealed class AlternateImage
		{
			internal readonly string Name;
			internal readonly string FileName;

			internal AlternateImage(string name, string fileName)
			{
				Name = name;
				FileName = fileName;
			}
		}

		private sealed class BiomeImageSet
		{
			internal readonly string DefaultFileName;
			internal readonly AlternateImage[] Alternates;

			internal BiomeImageSet(string defaultFileName, params AlternateImage[] alternates)
			{
				DefaultFileName = defaultFileName;
				Alternates = alternates;
			}
		}

		// Each biome has one default image followed by its alternate images in priority order.
		// Lantern and Bones Mistlands deliberately have no artwork entries.
		private static readonly Dictionary<string, BiomeImageSet> biomeImages = new Dictionary<string, BiomeImageSet>
		{
			{ "Meadows", new BiomeImageSet("Meadows_01.jpg",
				new AlternateImage("Dark", "Meadows_04_Dark.jpg"),
				new AlternateImage("Birch", "Meadows_02_Birch.jpg"),
				new AlternateImage("Menhir", "Meadows_08_Menhir.jpg"),
				new AlternateImage("Peaceful", "Meadows_05_Peaceful.jpg"),
				new AlternateImage("Smalltree", "Meadows_07_Smalltree.jpg"),
				new AlternateImage("Raspberry", "Meadows_06_Raspberry.jpg"),
				new AlternateImage("Mushroom", "Meadows_09_Mushroom.jpg"),
				new AlternateImage("Dandelion", "Meadows_03_Dandelion.jpg")) },

			{ "BlackForest", new BiomeImageSet("BlackForest_01.jpg",
				new AlternateImage("Troll", "BlackForest_08_Troll.jpg"),
				new AlternateImage("Root", "BlackForest_06_Root.jpg"),
				new AlternateImage("Ruin", "BlackForest_07_Ruin.jpg"),
				new AlternateImage("Kalhygge", "BlackForest_03_Kalhygge.jpg"),
				new AlternateImage("Rock", "BlackForest_05_Rock.jpg"),
				new AlternateImage("Blueberry", "BlackForest_02_Blueberry.jpg"),
				new AlternateImage("Mushroom", "BlackForest_09_Mushroom.jpg"),
				new AlternateImage("Pinetree", "BlackForest_04_Pine.jpg")) },

			{ "Swamp", new BiomeImageSet("Swamp_01.jpg",
				new AlternateImage("Abomination", "Swamp_02_Abomination.jpg"),
				new AlternateImage("Bat", "Swamp_03_Bat.jpg"),
				new AlternateImage("Bones", "Swamp_06_Bones.jpg"),
				new AlternateImage("Bog", "Swamp_04_Bog.jpg"),
				new AlternateImage("Hut", "Swamp_05_Hut.jpg")) },

			{ "Mountain", new BiomeImageSet("Mountain_01.jpg",
				new AlternateImage("Drake", "Mountain_02_Drake.jpg"),
				new AlternateImage("Wolf", "Mountain_04_Wolf.jpg"),
				new AlternateImage("Fortress", "Mountain_03_Fortress.jpg")) },

			{ "Plains", new BiomeImageSet("Plains_01.jpg",
				new AlternateImage("Death", "Plains_02_Death.jpg"),
				new AlternateImage("Goblin", "Plains_03_Goblin.jpg"),
				new AlternateImage("Lox", "Plains_04_Lox.jpg"),
				new AlternateImage("Bones", "Plains_05_Bones.jpg"),
				new AlternateImage("Menhir", "Plains_06_Menhir.jpg")) },

			{ "Mistlands", new BiomeImageSet("Mistlands_01.jpg",
				new AlternateImage("Swords", "Mistlands_05_Swords.jpg"),
				new AlternateImage("Trees", "Mistlands_06_Trees.jpg"),
				new AlternateImage("Hare", "Mistlands_03_Hare.jpg"),
				new AlternateImage("BroodSwarm", "Mistlands_02_BroodSwarm.jpg"),
				new AlternateImage("Rockless", "Mistlands_04_Rockless.jpg")) },

			{ "AshLands", new BiomeImageSet("Ashlands_01.jpg") },
			{ "DeepNorth", new BiomeImageSet("DeepNorth_01.jpg") },
			{ "Ocean", new BiomeImageSet("Ocean_01.jpg") }
		};

		// Called once when a portal session starts. IconManager caches every loaded sprite.
		internal static Sprite ResolveSprite(string biome, string altBiomes)
		{
			if (string.IsNullOrEmpty(biome) || !biomeImages.TryGetValue(biome, out BiomeImageSet imageSet))
			{
				log.Warn($"No portal artwork configured for biome '{biome}'. Using placeholder circle.");
				return null;
			}

			if (!string.IsNullOrWhiteSpace(altBiomes) && !altBiomes.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				string[] activeModifiers = altBiomes.Split(',');
				string normalizedBiome = NormalizeName(biome);

				foreach (AlternateImage alternate in imageSet.Alternates)
				{
					if (!IsActive(alternate.Name, normalizedBiome, activeModifiers)) continue;

					Sprite alternateSprite = LoadSprite(alternate.FileName);

					if (alternateSprite != null)
					{
						log.Info($"Portal artwork selected | Biome: {biome} | Modifier: {alternate.Name} | Image: {alternate.FileName}");
						return alternateSprite;
					}

					log.Warn($"Portal image '{alternate.FileName}' is unavailable. Checking the next eligible modifier.");
				}
			}

			Sprite defaultSprite = LoadSprite(imageSet.DefaultFileName);

			if (defaultSprite != null)
			{
				log.Info($"Portal artwork selected | Biome: {biome} | Image: {imageSet.DefaultFileName}");
			}
			else
			{
				log.Warn($"Default portal image '{imageSet.DefaultFileName}' is unavailable. Using placeholder circle.");
			}

			return defaultSprite;
		}

		private static Sprite LoadSprite(string fileName)
		{
			return IconManager.LoadEmbeddedIcon(BiomeImageResourcePrefix + fileName);
		}

		private static bool IsActive(string modifier, string normalizedBiome, string[] activeModifiers)
		{
			string normalizedModifier = NormalizeName(modifier);
			string fullModifierName = normalizedModifier + normalizedBiome;

			foreach (string activeModifier in activeModifiers)
			{
				string normalizedActive = NormalizeName(activeModifier);

				if (normalizedActive == normalizedModifier || normalizedActive == fullModifierName)
				{
					return true;
				}
			}

			return false;
		}

		internal static int GetAlternatePriority(string biome, string alternateName)
		{
			if (!biomeImages.TryGetValue(biome, out BiomeImageSet imageSet))
				return int.MaxValue;

			string normalizedName = NormalizeName(alternateName);
			string normalizedBiome = NormalizeName(biome);

			for (int i = 0; i < imageSet.Alternates.Length; i++)
			{
				string modifier = NormalizeName(imageSet.Alternates[i].Name);

				if (normalizedName == modifier || normalizedName == modifier + normalizedBiome)
					return i;
			}

			return int.MaxValue;
		}

		private static string NormalizeName(string name)
		{
			if (string.IsNullOrEmpty(name)) return string.Empty;

			char[] characters = new char[name.Length];
			int count = 0;

			foreach (char character in name)
			{
				if (!char.IsLetterOrDigit(character)) continue;

				characters[count++] = char.ToLowerInvariant(character);
			}

			return new string(characters, 0, count);
		}
	}
}
