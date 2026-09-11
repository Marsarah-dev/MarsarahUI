using HarmonyLib;
using MarsarahUI.Managers;
using TMPro;
using UnityEngine;

namespace MarsarahUI.Patches.UI
{
	internal class UIShowOwnedResources
	{
		private static readonly LogManager log = new LogManager("UI Show Owned Resources", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupRequirement))]
		private static class ShowOwnedResources_Patch
		{
			private static void Postfix(Transform elementRoot, Piece.Requirement req, Player player, bool craft, int quality, int craftMultiplier)
			{
				if (!ConfigManager.EffectiveShowOwnedResources) return;
				if (CompatibilityManager.CraftFromContainersLoaded) return;
				if (req?.m_resItem == null || player == null || elementRoot == null) return;

				int requiredAmount = req.GetAmount(quality) * craftMultiplier;
				int ownedAmount = player.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name);

				Transform resourceAmountTransform = elementRoot.Find("res_amount");
				if (resourceAmountTransform == null) return;

				TMP_Text resourceAmountText = resourceAmountTransform.GetComponent<TMP_Text>();
				if (resourceAmountText == null) return;

				resourceAmountText.text = $"{requiredAmount}/{ownedAmount}";

				bool noCost = (!craft && ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoBuildCost)) || (craft && ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost));

				if (ownedAmount < requiredAmount && !noCost)
				{
					resourceAmountText.color = Mathf.Sin(Time.time * 10f) > 0f ? Color.red : Color.white;
				}
				else
				{
					resourceAmountText.color = Color.white;
				}
			}
		}
	}
}