using HarmonyLib;
using MarsarahUI.Managers;

namespace MarsarahUI.Patches.UI
{
	internal class UIInventoryWeightAndSlots
	{
		private static readonly LogManager log = new LogManager("UI Inventory", LogManager.LogLevel.Warning);

		internal static float CurrentWeight { get; private set; }
		internal static float MaxWeight { get; private set; }
		internal static float FreeSlots { get; private set; }
		internal static float SlotsUsedPercent { get; private set; }

		[HarmonyPatch(typeof(Player), "Update")]
		private static class InventoryWeightAndSlotsPlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_localPlayer == null) return;
				if (ConfigManager.EffectiveInventoryDisplayChoice == ConfigManager.InventoryDisplayMode.Off) return;

				Inventory inventory = ___m_localPlayer.GetInventory();

				CurrentWeight = inventory.GetTotalWeight();
				MaxWeight = ___m_localPlayer.GetMaxCarryWeight();
				FreeSlots = inventory.GetEmptySlots();
				SlotsUsedPercent = inventory.SlotsUsedPercentage();
			}
		}
	}
}