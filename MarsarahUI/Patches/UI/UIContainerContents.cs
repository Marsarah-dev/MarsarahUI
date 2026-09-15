using MarsarahUI.Managers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static MarsarahUI.Managers.ConfigManager;

namespace MarsarahUI.Patches.UI
{
	internal class UIContainerContents : UIController
	{
		private static readonly LogManager log = new LogManager("UI Container Contents", LogManager.LogLevel.Warning);

		internal static string GetContainerInventoryList(Container container, Inventory inventory)
		{
			if (ConfigManager.EffectiveContainerContentsChoice == ContainerContentsMode.Off) return "";
			if (CompatibilityManager.TweaksIsContainerSealed(container)) return "This chest is sealed.";
			if (ConfigManager.EffectiveContainerContentsChoice != ContainerContentsMode.Text) return "";

			Dictionary<string, int> itemCounts = new Dictionary<string, int>();

			foreach (ItemDrop.ItemData item in inventory.GetAllItems())
			{
				if (item?.m_shared == null) continue;

				string itemName = Localization.instance.Localize(item.m_shared.m_name);

				if (!itemCounts.ContainsKey(itemName))
				{
					itemCounts[itemName] = 0;
				}

				itemCounts[itemName] += item.m_stack;
			}

			if (itemCounts.Count == 0) return "";

			StringBuilder stringBuilder = new StringBuilder();
			int shown = 0;
			int total = itemCounts.Count;

			foreach (KeyValuePair<string, int> item in itemCounts)
			{
				if (shown >= 10) break;

				string countColored = UIDetailedHovers.PaintTextIfEnabled(item.Value.ToString(), Color.yellow);
				string nameColored = UIDetailedHovers.PaintTextIfEnabled(item.Key, Color.gray);

				stringBuilder.AppendLine($"{countColored} {nameColored}");
				shown++;
			}

			if (total > 10)
			{
				string plus = UIDetailedHovers.PaintTextIfEnabled("+", Color.yellow);
				string number = UIDetailedHovers.PaintTextIfEnabled((total - 10).ToString(), Color.yellow);
				string others = UIDetailedHovers.PaintTextIfEnabled(" Others", Color.gray);

				stringBuilder.AppendLine($"{plus}{number}{others}");
			}

			return stringBuilder.ToString().TrimEnd();
		}
	}
}