using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahUI.Managers
{
	internal static class UIStyleManager
	{
		private static readonly LogManager log = new LogManager("UI Style Manager", LogManager.LogLevel.Warning);

		internal static event Action StyleChanged;

		private static readonly UIStyleDefinition style3 = new UIStyleDefinition
		{
			RailBackgroundAsset = "RailBorder",
			SummonBackgroundAsset = "RailBorder",

			WeightIcon = "Weight",
			SlotsIcon = "Slots",
			EnemyIcon = "Enemy",
			ToughEnemyIcon = "ToughEnemy",
			BossIcon = "Boss",
			NeutralIcon = "Neutral",
			SummonIcon = "Summon",

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
			SummonEndWidth = 12f
		};

		private static readonly Dictionary<ConfigManager.InfoRailStyle, UIStyleDefinition> styles = new Dictionary<ConfigManager.InfoRailStyle, UIStyleDefinition>
		{
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
	}

	internal sealed class UIStyleDefinition
	{
		internal string RailBackgroundAsset;
		internal string SummonBackgroundAsset;

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
}