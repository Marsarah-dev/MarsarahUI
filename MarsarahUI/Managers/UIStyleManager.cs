using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahUI.Managers
{
	internal enum InfoRailBackgroundType
	{
		None,
		UnityImage,
		VanillaSlicedSprite
	}

	internal enum InfoRailBorderType
	{
		None,
		ThreePartSprite
	}

	internal enum InfoRailColorMode
	{
		Fixed,
		WeightGradient,
		SlotsGradient,
		EnemyCountGradient,
		SummonCountGradient
	}

	internal sealed class UIStyleDefinition
	{
		internal InfoRailColorMode WeightFillColorMode;
		internal InfoRailColorMode SlotsTextColorMode;
		internal InfoRailColorMode EnemyTextColorMode;
		internal InfoRailColorMode SummonTextColorMode;

		internal Color ToughEnemyTextColor;
		internal Color BossTextColor;
		internal Color NeutralTextColor;
		internal Color SkillTextColor;
		internal Color SummonTextColor;

		// Background
		internal InfoRailBackgroundType BackgroundType;
		internal string VanillaBackgroundSprite;
		internal Color BackgroundColor;

		// Border
		internal InfoRailBorderType BorderType;
		internal string RailBorderAsset;
		internal string SummonBorderAsset;
		internal Color BorderColor;

		internal string WeightIcon;
		internal string SlotsIcon;
		internal string EnemyIcon;
		internal string ToughEnemyIcon;
		internal string BossIcon;
		internal string NeutralIcon;
		internal string SummonIcon;

		internal Color ValueTextColor;
		internal Color WeightFillColor;
		internal Color SeparatorColor;

		internal float RailHeight;
		internal float RailSourceEndWidth;
		internal float RailEndWidth;
		internal RectOffset RailPadding;

		internal float SeparatorWidth;
		internal float SeparatorLineWidth;
		internal float SeparatorHeight;

		internal Vector2 SummonSize;
		internal float SummonSourceEndWidth;
		internal float SummonEndWidth;
	}

	internal static class UIStyleManager
	{
		private static readonly LogManager log = new LogManager("UI Style Manager", LogManager.LogLevel.Warning);

		internal static event Action StyleChanged;

		private static readonly UIStyleDefinition style1 = new UIStyleDefinition
		{
			BackgroundType = InfoRailBackgroundType.VanillaSlicedSprite,
			VanillaBackgroundSprite = "InputFieldBackground",
			BackgroundColor = new Color(0f, 0f, 0f, 0.4f),

			BorderType = InfoRailBorderType.None,
			BorderColor = Color.white,

			WeightIcon = "Style1.Weight",
			SlotsIcon = "Style1.Slots",
			EnemyIcon = "Style1.Enemy",
			ToughEnemyIcon = "Style1.ToughEnemy",
			BossIcon = "Style1.Boss",
			NeutralIcon = "Style1.Neutral",
			SummonIcon = "Style1.Summon",

			ValueTextColor = Color.white,
			WeightFillColor = Color.green,
			SeparatorColor = new Color(1f, 1f, 1f, 0.2f),

			RailHeight = 30f,
			RailPadding = new RectOffset(7, 7, 3, 3),

			SeparatorWidth = 7f,
			SeparatorLineWidth = 1f,
			SeparatorHeight = 18f,

			SummonSize = new Vector2(49f, 30f),

			// Special colors
			WeightFillColorMode = InfoRailColorMode.WeightGradient,
			SlotsTextColorMode = InfoRailColorMode.SlotsGradient,
			EnemyTextColorMode = InfoRailColorMode.EnemyCountGradient,
			SummonTextColorMode = InfoRailColorMode.SummonCountGradient,

			ToughEnemyTextColor = new Color(1f, 0.549019f, 0f),
			BossTextColor = new Color(0.75f, 0.4f, 1f),
			NeutralTextColor = new Color(1f, 0.75f, 0.2f),
			SkillTextColor = new Color(1f, 0.75f, 0.2f),
			SummonTextColor = Color.white,
		};

		private static readonly UIStyleDefinition style3 = new UIStyleDefinition
		{
			BackgroundType = InfoRailBackgroundType.None,
			BackgroundColor = Color.clear,

			BorderType = InfoRailBorderType.ThreePartSprite,
			RailBorderAsset = "Style3.RailBorder",
			SummonBorderAsset = "Style3.RailBorder",
			BorderColor = Color.white,

			WeightIcon = "Style3.Weight",
			SlotsIcon = "Style3.Slots",
			EnemyIcon = "Style3.Enemy",
			ToughEnemyIcon = "Style3.ToughEnemy",
			BossIcon = "Style3.Boss",
			NeutralIcon = "Style3.Neutral",
			SummonIcon = "Style3.Summon",

			ValueTextColor = new Color(0.88f, 0.87f, 0.82f),
			WeightFillColor = new Color(0.333f, 0.357f, 0.369f),
			SeparatorColor = new Color(1f, 1f, 1f, 0.4f),

			RailHeight = 34f,
			RailSourceEndWidth = 12f,
			RailEndWidth = 12f,
			RailPadding = new RectOffset(7, 7, 3, 3),

			SeparatorWidth = 7f,
			SeparatorLineWidth = 1f,
			SeparatorHeight = 18f,

			SummonSize = new Vector2(49f, 34f),
			SummonSourceEndWidth = 12f,
			SummonEndWidth = 12f,

			// Colors
			WeightFillColorMode = InfoRailColorMode.Fixed,
			SlotsTextColorMode = InfoRailColorMode.Fixed,
			EnemyTextColorMode = InfoRailColorMode.Fixed,
			SummonTextColorMode = InfoRailColorMode.Fixed,

			ToughEnemyTextColor = new Color(0.88f, 0.87f, 0.82f),
			BossTextColor = new Color(0.88f, 0.87f, 0.82f),
			NeutralTextColor = new Color(0.88f, 0.87f, 0.82f),
			SkillTextColor = new Color(0.88f, 0.87f, 0.82f),
			SummonTextColor = new Color(0.88f, 0.87f, 0.82f),
		};

		private static readonly Dictionary<ConfigManager.InfoRailStyle, UIStyleDefinition> styles = new Dictionary<ConfigManager.InfoRailStyle, UIStyleDefinition>
		{
			{ ConfigManager.InfoRailStyle.Style1, style1 },
			{ ConfigManager.InfoRailStyle.Style3, style3 }
		};

		private static ConfigEntry<ConfigManager.InfoRailStyle> styleConfig;

		internal static UIStyleDefinition Current { get; private set; } = style3;

		internal static void Initialize(ConfigEntry<ConfigManager.InfoRailStyle> config)
		{
			if (styleConfig != null)
			{
				styleConfig.SettingChanged -= OnStyleSettingChanged;
			}

			styleConfig = config;

			if (styleConfig != null)
			{
				styleConfig.SettingChanged += OnStyleSettingChanged;
			}

			RefreshStyle(false);
		}

		private static void OnStyleSettingChanged(object sender, EventArgs e)
		{
			RefreshStyle(true);
		}

		private static void RefreshStyle(bool notify)
		{
			ConfigManager.InfoRailStyle selectedStyle = styleConfig?.Value ?? ConfigManager.InfoRailStyle.Style3;

			if (!styles.TryGetValue(selectedStyle, out UIStyleDefinition style))
			{
				log.Warn($"UI style '{selectedStyle}' is not implemented yet. Falling back to Style3.");
				style = style3;
			}

			Current = style;

			if (notify)
			{
				StyleChanged?.Invoke();
			}
		}

		internal static Color GetWeightFillColor(float weightPercent)
		{
			switch (Current.WeightFillColorMode)
			{
				case InfoRailColorMode.WeightGradient:
					return GetUsageGradientColor(weightPercent);

				default:
					return Current.WeightFillColor;
			}
		}

		internal static Color GetSlotsTextColor(float slotsUsedPercent)
		{
			switch (Current.SlotsTextColorMode)
			{
				case InfoRailColorMode.SlotsGradient:
					return GetUsageGradientColor(Mathf.Clamp01(slotsUsedPercent / 100f));

				default:
					return Current.ValueTextColor;
			}
		}

		internal static Color GetEnemyTextColor(int enemyCount)
		{
			switch (Current.EnemyTextColorMode)
			{
				case InfoRailColorMode.EnemyCountGradient:
					return GetCountGradientColor(enemyCount);

				default:
					return Current.ValueTextColor;
			}
		}

		private static Color GetUsageGradientColor(float percent)
		{
			Color green = Color.green;
			Color yellow = Color.yellow;
			Color orange = new Color(1f, 0.549019f, 0f);

			percent = Mathf.Clamp01(percent);

			if (percent <= 0.33f)
			{
				return green;
			}

			if (percent <= 0.66f)
			{
				return Color.Lerp(green, yellow, (percent - 0.33f) / 0.33f);
			}

			if (percent < 1f)
			{
				return Color.Lerp(yellow, orange, (percent - 0.66f) / 0.34f);
			}

			return Color.red;
		}

		private static Color GetCountGradientColor(int count)
		{
			if (count < 3) return Color.green;
			if (count < 5) return Color.yellow;
			if (count < 7) return new Color(1f, 0.549019f, 0f);

			return Color.red;
		}

		internal static Color GetSummonTextColor(int summonCount)
		{
			switch (Current.SummonTextColorMode)
			{
				case InfoRailColorMode.SummonCountGradient:
					return GetSummonCountColor(summonCount);

				default:
					return Current.SummonTextColor;
			}
		}

		private static Color GetSummonCountColor(int count)
		{
			if (count < 2) return new Color(1f, 0.549019f, 0f);
			if (count == 2 || count == 3) return Color.yellow;
			if (count >= 4) return Color.green;

			return Current.SummonTextColor;
		}
	}
}