
using System;
using System.Collections.Generic;

namespace MarsarahUI.Patches.Logic
{
	internal static class BiomePriority
	{
		// Alternates are listed from highest to lowest priority.
		// Unknown alternates always have the lowest priority.
		private static readonly Dictionary<string, string[]> biomePriorities = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
		{
			{ "Meadows", new[]
			{
				"Dark",
				"Birch",
				"Peaceful",
				"Mushroom",
				"Raspberry",
				"Smalltree",
				"Menhir",
				"Dandelion"
			} },

			{ "BlackForest", new[]
			{
				"Troll",
				"Root",
				"Ruin",
				"Mushroom",
				"Blueberry",
				"Kalhygge",
				"Rock",
				"Pinetree"
			} },

			{ "Swamp", new[]
			{
				"Abomination",
				"Bat",
				"Bones",
				"Bog",
				"Hut"
			} },

			{ "Mountain", new[]
			{
				"Drake",
				"Wolf",
				"Fortress"
			} },

			{ "Plains", new[]
			{
				"Death",
				"Goblin",
				"Lox",
				"Bones",
				"Menhir"
			} },

			{ "Mistlands", new[]
			{
				"Swords",
				"Trees",
				"Hare",
				"BroodSwarm",
				"Rockless"
			} }
		};

		internal static int GetAlternatePriority(string biome, string alternateName)
		{
			if (string.IsNullOrWhiteSpace(biome) || string.IsNullOrWhiteSpace(alternateName))
				return int.MaxValue;

			if (!biomePriorities.TryGetValue(biome, out string[] alternates))
				return int.MaxValue;

			string normalizedName = NormalizeName(alternateName);
			string normalizedBiome = NormalizeName(biome);

			for (int i = 0; i < alternates.Length; i++)
			{
				string modifier = NormalizeName(alternates[i]);

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
