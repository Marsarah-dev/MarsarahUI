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
		UnityBorder,
		SideCapsSprite
	}

	internal enum InfoRailColorMode
	{
		Fixed,
		WeightGradient,
		WarmWeightGradient,
		SlotsGradient,
		EnemyCountGradient,
		SummonCountGradient
	}

	internal sealed class UIStyleDefinition
	{
		// Background
		internal InfoRailBackgroundType BackgroundType;
		internal string VanillaBackgroundSprite;
		internal Color BackgroundColor;

		// Border
		internal InfoRailBorderType BorderType;
		internal InfoRailBorderType SummonBorderType;
		internal string BorderCapAsset;
		internal Color BorderColor;
		internal Vector2 BorderCapSize;
		internal Color BorderOuterColor;
		internal Color BorderInnerColor;
		internal float BorderOuterWidth;
		internal float BorderInnerWidth;

		// Icons
		internal string WeightIcon;
		internal string SlotsIcon;
		internal string EnemyIcon;
		internal string ToughEnemyIcon;
		internal string BossIcon;
		internal string NeutralIcon;
		internal string SummonIcon;

		// Rail
		internal float RailHeight;
		internal RectOffset RailPadding;

		// Separators
		internal float SeparatorWidth;
		internal float SeparatorLineWidth;
		internal float SeparatorHeight;
		internal Color SeparatorColor;
		internal bool UseSeparatorEdge;
		internal Color SeparatorEdgeColor;
		internal Color SeparatorHighlightColor;
		internal float SeparatorEdgeWidth;
		internal float SeparatorHighlightWidth;

		// Summon Display
		internal Vector2 SummonSize;

		// Colors
		internal Color ValueTextColor;
		internal Color WeightFillColor;
		internal Color ToughEnemyTextColor;
		internal Color BossTextColor;
		internal Color NeutralTextColor;
		internal Color SkillTextColor;
		internal Color SummonTextColor;

		// Color Modes
		internal InfoRailColorMode WeightFillColorMode;
		internal InfoRailColorMode SlotsTextColorMode;
		internal InfoRailColorMode EnemyTextColorMode;
		internal InfoRailColorMode SummonTextColorMode;
	}

	internal static class UIStyleManager
	{
		private static readonly LogManager log = new LogManager("UI Style Manager", LogManager.LogLevel.Warning);

		internal static event Action StyleChanged;

		private static readonly UIStyleDefinition style1 = new UIStyleDefinition
		{
			// Background
			BackgroundType = InfoRailBackgroundType.VanillaSlicedSprite,
			VanillaBackgroundSprite = "InputFieldBackground",
			BackgroundColor = new Color(0f, 0f, 0f, 0.4f),

			// Border
			BorderType = InfoRailBorderType.None,
			SummonBorderType = InfoRailBorderType.None,
			BorderColor = Color.white,

			// Icons
			WeightIcon = "Style1.Weight",
			SlotsIcon = "Style1.Slots",
			EnemyIcon = "Style1.Enemy",
			ToughEnemyIcon = "Style1.ToughEnemy",
			BossIcon = "Style1.Boss",
			NeutralIcon = "Style1.Neutral",
			SummonIcon = "Style1.Summon",

			// Rail
			RailHeight = 30f,
			RailPadding = new RectOffset(7, 7, 3, 3),

			// Separators
			SeparatorWidth = 7f,
			SeparatorLineWidth = 1f,
			SeparatorHeight = 18f,
			SeparatorColor = new Color(1f, 1f, 1f, 0.2f),
			UseSeparatorEdge = false,

			// Summon Display
			SummonSize = new Vector2(49f, 30f),

			// Colors
			ValueTextColor = Color.white,
			WeightFillColor = Color.green,
			ToughEnemyTextColor = new Color(1f, 0.549019f, 0f),
			BossTextColor = new Color(0.75f, 0.4f, 1f),
			NeutralTextColor = new Color(1f, 0.75f, 0.2f),
			SkillTextColor = new Color(1f, 0.75f, 0.2f),
			SummonTextColor = Color.white,

			// Color Modes
			WeightFillColorMode = InfoRailColorMode.WeightGradient,
			SlotsTextColorMode = InfoRailColorMode.SlotsGradient,
			EnemyTextColorMode = InfoRailColorMode.EnemyCountGradient,
			SummonTextColorMode = InfoRailColorMode.SummonCountGradient
		};

		private static readonly UIStyleDefinition style2 = new UIStyleDefinition
		{
			// Background
			BackgroundType = InfoRailBackgroundType.VanillaSlicedSprite,
			VanillaBackgroundSprite = "InputFieldBackground",
			BackgroundColor = new Color(0f, 0f, 0f, 0.45f),

			// Border
			BorderType = InfoRailBorderType.SideCapsSprite,
			SummonBorderType = InfoRailBorderType.SideCapsSprite,
			BorderCapAsset = "Style2.BorderCap",
			BorderColor = Color.white,
			BorderCapSize = new Vector2(18f, 36f),

			// Icons
			WeightIcon = "Style2.Weight",
			SlotsIcon = "Style2.Slots",
			EnemyIcon = "Style2.Enemy",
			ToughEnemyIcon = "Style2.ToughEnemy",
			BossIcon = "Style2.Boss",
			NeutralIcon = "Style2.Neutral",
			SummonIcon = "Style2.Summon",

			// Rail
			RailHeight = 30f,
			RailPadding = new RectOffset(7, 7, 3, 3),

			// Separators
			SeparatorWidth = 7f,
			SeparatorLineWidth = 2f,
			SeparatorHeight = 22f,
			SeparatorColor = new Color(0.62f, 0.52f, 0.34f, 1f),
			UseSeparatorEdge = true,
			SeparatorEdgeColor = new Color(0.18f, 0.10f, 0.05f, 0.8f),
			SeparatorHighlightColor = new Color(0.95f, 0.72f, 0.35f, 0.85f),
			SeparatorEdgeWidth = 3f,
			SeparatorHighlightWidth = 1f,

			// Summon Display
			SummonSize = new Vector2(49f, 30f),

			// Colors
			ValueTextColor = Color.white,
			WeightFillColor = Color.green,
			ToughEnemyTextColor = new Color(1f, 0.549019f, 0f),
			BossTextColor = new Color(0.75f, 0.4f, 1f),
			NeutralTextColor = new Color(1f, 0.75f, 0.2f),
			SkillTextColor = new Color(1f, 0.75f, 0.2f),
			SummonTextColor = Color.white,

			// Color Modes
			WeightFillColorMode = InfoRailColorMode.WarmWeightGradient,
			SlotsTextColorMode = InfoRailColorMode.SlotsGradient,
			EnemyTextColorMode = InfoRailColorMode.EnemyCountGradient,
			SummonTextColorMode = InfoRailColorMode.SummonCountGradient
		};

		private static readonly UIStyleDefinition style3 = new UIStyleDefinition
		{
			// Background
			BackgroundType = InfoRailBackgroundType.VanillaSlicedSprite,
			VanillaBackgroundSprite = "InputFieldBackground",
			BackgroundColor = new Color(0f, 0f, 0f, 0.45f),

			// Border
			BorderType = InfoRailBorderType.UnityBorder,
			SummonBorderType = InfoRailBorderType.None,
			BorderColor = new Color(0.58f, 0.58f, 0.55f, 1f),
			BorderOuterColor = new Color(0.12f, 0.12f, 0.12f, 1f),
			BorderInnerColor = new Color(0.28f, 0.28f, 0.27f, 1f),
			BorderOuterWidth = 2f,
			BorderInnerWidth = 1f,

			// Icons
			WeightIcon = "Style3.Weight",
			SlotsIcon = "Style3.Slots",
			EnemyIcon = "Style3.Enemy",
			ToughEnemyIcon = "Style3.ToughEnemy",
			BossIcon = "Style3.Boss",
			NeutralIcon = "Style3.Neutral",
			SummonIcon = "Style3.Summon",

			// Rail
			RailHeight = 34f,
			RailPadding = new RectOffset(7, 7, 3, 3),

			// Separators
			SeparatorWidth = 7f,
			SeparatorLineWidth = 1f,
			SeparatorHeight = 18f,
			SeparatorColor = new Color(1f, 1f, 1f, 0.4f),
			UseSeparatorEdge = false,

			// Summon Display
			SummonSize = new Vector2(49f, 34f),

			// Colors
			ValueTextColor = new Color(0.88f, 0.87f, 0.82f),
			WeightFillColor = new Color(0.333f, 0.357f, 0.369f),
			ToughEnemyTextColor = new Color(0.88f, 0.87f, 0.82f),
			BossTextColor = new Color(0.88f, 0.87f, 0.82f),
			NeutralTextColor = new Color(0.88f, 0.87f, 0.82f),
			SkillTextColor = new Color(0.88f, 0.87f, 0.82f),
			SummonTextColor = new Color(0.88f, 0.87f, 0.82f),

			// Color Modes
			WeightFillColorMode = InfoRailColorMode.Fixed,
			SlotsTextColorMode = InfoRailColorMode.Fixed,
			EnemyTextColorMode = InfoRailColorMode.Fixed,
			SummonTextColorMode = InfoRailColorMode.Fixed
		};

		private static readonly Dictionary<ConfigManager.InfoRailStyle, UIStyleDefinition> styles = new Dictionary<ConfigManager.InfoRailStyle, UIStyleDefinition>
		{
			{ ConfigManager.InfoRailStyle.Style1, style1 },
			{ ConfigManager.InfoRailStyle.Style2, style2 },
			{ ConfigManager.InfoRailStyle.Style3, style3 }
		};

		private static ConfigEntry<ConfigManager.InfoRailStyle> styleConfig;

		internal static UIStyleDefinition Current { get; private set; } = style1;

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
			ConfigManager.InfoRailStyle selectedStyle = styleConfig?.Value ?? ConfigManager.InfoRailStyle.Style1;

			Current = styles[selectedStyle];

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

				case InfoRailColorMode.WarmWeightGradient:
					return GetWarmWeightGradientColor(weightPercent);

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

		private static Color GetWarmWeightGradientColor(float percent)
		{
			Color gold = new Color(0.85f, 0.65f, 0.22f);
			Color orange = new Color(1f, 0.45f, 0.08f);
			Color red = new Color(0.85f, 0.20f, 0.12f);

			percent = Mathf.Clamp01(percent);

			if (percent <= 0.66f)
			{
				return gold;
			}

			if (percent <= 0.90f)
			{
				return Color.Lerp(gold, orange, (percent - 0.66f) / 0.24f);
			}

			return Color.Lerp(orange, red, (percent - 0.90f) / 0.10f);
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