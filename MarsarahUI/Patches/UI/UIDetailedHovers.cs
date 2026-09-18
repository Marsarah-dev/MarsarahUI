using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Reflection;
using UnityEngine;
using static MarsarahUI.Managers.ConfigManager;

namespace MarsarahUI.Patches.UI
{
	internal class UIDetailedHovers
	{
		private static readonly LogManager log = new LogManager("UI Detailed Hover Info", LogManager.LogLevel.Warning);

		private static readonly MethodInfo GetHoneyLevelMethod = typeof(Beehive).GetMethod("GetHoneyLevel", BindingFlags.NonPublic | BindingFlags.Instance);

		private static readonly MethodInfo GetTimeSincePlantedMethod = typeof(Plant).GetMethod("TimeSincePlanted", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetGrowTimeMethod = typeof(Plant).GetMethod("GetGrowTime", BindingFlags.NonPublic | BindingFlags.Instance);

		private static readonly MethodInfo GetFermenterStatusMethod = typeof(Fermenter).GetMethod("GetStatus", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetFermenterContentNameMethod = typeof(Fermenter).GetMethod("GetContentName", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetFermentationTimeMethod = typeof(Fermenter).GetMethod("GetFermentationTime", BindingFlags.NonPublic | BindingFlags.Instance);

		private static readonly MethodInfo GetSlotMethod = typeof(CookingStation).GetMethod("GetSlot", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetItemConversionMethod = typeof(CookingStation).GetMethod("GetItemConversion", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetHoverTextMethod = typeof(CookingStation).GetMethod("HoverText", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetCSFuelMethod = typeof(CookingStation).GetMethod("GetFuel", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo OnHoverFuelSwitchMethod = typeof(CookingStation).GetMethod("OnHoverFuelSwitch", BindingFlags.NonPublic | BindingFlags.Instance);

		private static readonly MethodInfo GetQueueSizeMethod = typeof(Smelter).GetMethod("GetQueueSize", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetFuelMethod = typeof(Smelter).GetMethod("GetFuel", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetBakeTimerMethod = typeof(Smelter).GetMethod("GetBakeTimer", BindingFlags.NonPublic | BindingFlags.Instance);

		private static readonly MethodInfo ContainerCheckAccessMethod = AccessTools.Method(typeof(Container), "CheckAccess", new Type[] { typeof(long) });

		[HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
		internal static class DetailedHoverContainer_Patch
		{
			private static void Postfix(Container __instance, Inventory ___m_inventory, ref string __result)
			{
				if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off) return;
				if (!HasContainerAccess(__instance)) return;
				if (___m_inventory == null || ___m_inventory.NrOfItems() == 0) return;

				__result = GetContainerHover(__instance, ___m_inventory);
			}

			private static string GetContainerHover(Container container, Inventory inventory)
			{
				ContainerHoverMode containerMode = ConfigManager.EffectiveContainerHoverModeChoice;

				int max = inventory.GetWidth() * inventory.GetHeight();
				string containerText = "";

				switch (containerMode)
				{
					case ContainerHoverMode.CurrentPerMax:
						int used = inventory.NrOfItems();
						string usedPerMaxText = $"{used}/{max}";
						containerText = PaintTextIfEnabled(usedPerMaxText, GetInventoryRatioColor(used, max));
						break;

					case ContainerHoverMode.AmountOfFreeSlots:
						int emptySlots = inventory.GetEmptySlots();
						string emptySlotsText = emptySlots.ToString();
						containerText = $"Free Slots: {PaintTextIfEnabled(emptySlotsText, GetInventoryEmptySlotsColor(emptySlots, max))}";
						break;

					case ContainerHoverMode.Percent:
						float usedPercentRaw = inventory.SlotsUsedPercentage();
						float usedPercentNormalized = usedPercentRaw / 100f;
						string usedPercentText = $"{usedPercentRaw}%";
						containerText = PaintTextIfEnabled(usedPercentText, GetPercentColorInverted(usedPercentNormalized));
						break;
				}

				string localizedName = Localization.instance.Localize(container.m_name);
				string localizedOpen = Localization.instance.Localize("$piece_container_open");
				string localizedStack = Localization.instance.Localize("$msg_stackall_hover");
				string useKeyColored = $"[{PaintText(Localization.instance.Localize("$KEY_Use"), Color.yellow)}]";
				string containerItemsLine = UIContainerContents.GetContainerInventoryList(container, inventory);

				return containerItemsLine != ""
					? $"{localizedName} ({containerText})\n{useKeyColored} {localizedOpen} {localizedStack}\n\n{containerItemsLine}"
					: $"{localizedName} ({containerText})\n{useKeyColored} {localizedOpen} {localizedStack}";
			}

			private static Color GetInventoryRatioColor(int used, int max)
			{
				float fill = max > 0 ? (float)used / max : 0f;

				return fill < 0.5f
					? Color.Lerp(Color.green, Color.yellow, fill / 0.5f)
					: Color.Lerp(Color.yellow, Color.red, (fill - 0.5f) / 0.5f);
			}

			private static Color GetInventoryEmptySlotsColor(int empty, int max)
			{
				float fill = max > 0 ? (float)empty / max : 0f;

				return fill < 0.5f
					? Color.Lerp(Color.red, Color.yellow, fill / 0.5f)
					: Color.Lerp(Color.yellow, Color.green, (fill - 0.5f) / 0.5f);
			}
		}

		[HarmonyPatch(typeof(Beehive), nameof(Beehive.GetHoverText))]
		internal static class DetailedHoverBeehive_Patch
		{
			private static bool Prefix(Beehive __instance, ZNetView ___m_nview, ref string __result)
			{
				if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off) return true;

				if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, flash: false))
				{
					__result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
					return false;
				}

				__result = GetBeehiveHover(__instance, ___m_nview);
				return false;
			}

			private static string GetBeehiveHover(Beehive beehive, ZNetView nview)
			{
				string name = Localization.instance.Localize(beehive.m_name);

				if (GetHoneyLevelMethod == null || nview?.GetZDO() == null)
				{
					log.Warn("Beehive reflection methods not found.");
					return name;
				}

				int honeyLevel = (int)GetHoneyLevelMethod.Invoke(beehive, null);
				float produced = nview.GetZDO().GetFloat("product");
				float remaining = beehive.m_secPerUnit - produced;

				BeeHoverMode beeMode = ConfigManager.EffectiveBeehiveHoverModeChoice;
				string progressText = "";

				switch (beeMode)
				{
					case BeeHoverMode.Percent:
						float honeyPercent = Mathf.Clamp01(produced / beehive.m_secPerUnit);
						progressText = PaintTextIfEnabled($"{honeyPercent:0%}", GetPercentColor(honeyPercent));
						break;

					case BeeHoverMode.RemainingTime:
						progressText = PaintTextIfEnabled(FormatTime(remaining), Color.cyan);
						break;

					case BeeHoverMode.PercentAndTime:
						float combinedPercent = Mathf.Clamp01(produced / beehive.m_secPerUnit);
						string percentColored = PaintTextIfEnabled($"{combinedPercent:0%}", GetPercentColor(combinedPercent));
						string timeColored = PaintTextIfEnabled(FormatTime(remaining), Color.cyan);
						progressText = $"{percentColored} - {timeColored}";
						break;
				}

				string productName = Localization.instance.Localize(beehive.m_honeyItem.m_itemData.m_shared.m_name);
				Color honeyColor = GetHoneyColor(honeyLevel, beehive.m_maxHoney);
				string productColored = PaintTextIfEnabled(productName, honeyColor);
				string honeyCountColored = PaintTextIfEnabled("x" + honeyLevel, honeyColor);
				string useKeyColored = $"[{PaintText(Localization.instance.Localize("$KEY_Use"), Color.yellow)}]";

				if (honeyLevel == beehive.m_maxHoney)
				{
					return $"{name} ( {productColored} {honeyCountColored} )\n{useKeyColored} {Localization.instance.Localize("$piece_beehive_extract")}";
				}

				if (honeyLevel > 0)
				{
					return $"{name} ( {progressText}, {productColored} {honeyCountColored} )\n{useKeyColored} {Localization.instance.Localize("$piece_beehive_extract")}";
				}

				string emptyText = PaintTextIfEnabled(Localization.instance.Localize("$piece_container_empty"), GetEmptyColor());

				return $"{name} ( {progressText}, {emptyText} )\n{useKeyColored} {Localization.instance.Localize("$piece_beehive_check")}";
			}

			private static Color GetHoneyColor(int honeyLevel, int maxHoney)
			{
				float fill = maxHoney > 0 ? (float)honeyLevel / maxHoney : 0f;

				return fill < 0.5f
					? Color.Lerp(Color.red, Color.yellow, fill / 0.5f)
					: Color.Lerp(Color.yellow, Color.green, (fill - 0.5f) / 0.5f);
			}

			private static Color GetEmptyColor()
			{
				return new Color(1f, 0.4f, 0f);
			}
		}

		[HarmonyPatch(typeof(Plant), nameof(Plant.GetHoverText))]
		internal static class DetailedHoverPlant_Patch
		{
			private static bool Prefix(Plant __instance, ref string __result)
			{
				if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off) return true;

				if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, flash: false))
				{
					__result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
					return false;
				}

				__result = GetPlantHover(__instance);
				return false;
			}

			private static string GetPlantHover(Plant plant)
			{
				string name = Localization.instance.Localize(plant.m_name);

				if (GetTimeSincePlantedMethod == null || GetGrowTimeMethod == null)
				{
					log.Warn("Plant reflection methods not found.");
					return name;
				}

				PlantHoverMode plantMode = ConfigManager.EffectivePlantHoverModeChoice;

				double age = (double)GetTimeSincePlantedMethod.Invoke(plant, null);
				float growTime = (float)GetGrowTimeMethod.Invoke(plant, null);

				string growthLine = "";

				switch (plantMode)
				{
					case PlantHoverMode.Percent:
						float growthPercent = Mathf.Clamp01((float)(age / growTime));
						growthLine = PaintTextIfEnabled($"{growthPercent:0%}", GetPercentColor(growthPercent));
						break;

					case PlantHoverMode.RemainingTime:
						float remaining = Mathf.Max(0f, growTime - (float)age);
						growthLine = PaintTextIfEnabled(FormatTime(remaining), Color.cyan);
						break;

					case PlantHoverMode.PercentAndTime:
						float combinedPercent = Mathf.Clamp01((float)(age / growTime));
						string percentColored = PaintTextIfEnabled($"{combinedPercent:0%}", GetPercentColor(combinedPercent));

						float combinedRemaining = Mathf.Max(0f, growTime - (float)age);
						string timeColored = PaintTextIfEnabled(FormatTime(combinedRemaining), Color.cyan);

						growthLine = $"{percentColored} - {timeColored}";
						break;
				}

				string useKeyColored = $"[{PaintText(Localization.instance.Localize("$KEY_Use"), Color.yellow)}]";

				if (age >= growTime)
				{
					return $"{name} ( {Localization.instance.Localize("$hud_ready")} )\n{useKeyColored} {Localization.instance.Localize("$inventory_pickup")}";
				}

				return $"{name} ( {growthLine} )";
			}
		}

		[HarmonyPatch(typeof(Fermenter), nameof(Fermenter.GetHoverText))]
		internal static class DetailedHoverFermenter_Patch
		{
			private static bool Prefix(Fermenter __instance, bool ___m_exposed, ref string __result)
			{
				if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off) return true;

				if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, flash: false))
				{
					__result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
					return false;
				}

				string hoverText = GetFermenterHover(__instance, ___m_exposed);

				if (hoverText == null) return true;

				__result = hoverText;
				return false;
			}

			private static string GetFermenterHover(Fermenter fermenter, bool exposed)
			{
				string name = Localization.instance.Localize(fermenter.m_name);

				if (GetFermenterStatusMethod == null ||
					GetFermenterContentNameMethod == null ||
					GetFermentationTimeMethod == null)
				{
					log.Warn("Fermenter reflection methods not found.");
					return null;
				}

				object statusObject = GetFermenterStatusMethod.Invoke(fermenter, null);
				if (statusObject == null) return null;

				string statusName = Enum.GetName(GetFermenterStatusMethod.ReturnType, statusObject);

				switch (statusName)
				{
					case "Fermenting":
						string contentName = Localization.instance.Localize((string)GetFermenterContentNameMethod.Invoke(fermenter, null));
						string localizedExposed = Localization.instance.Localize("$piece_fermenter_exposed");
						string localizedFermenting = Localization.instance.Localize("$piece_fermenter_fermenting");

						if (exposed)
						{
							return $"{name} ( {contentName} )\n{localizedExposed}";
						}

						double timePassed = (double)GetFermentationTimeMethod.Invoke(fermenter, null);
						float totalTime = fermenter.m_fermentationDuration;

						switch (ConfigManager.EffectiveFermenterHoverModeChoice)
						{
							case FermenterHoverMode.Percent:
								float percent = Mathf.Clamp01((float)(timePassed / totalTime));
								return $"{name} ( {contentName} )\n{localizedFermenting}: {PaintTextIfEnabled($"{percent:0%}", GetPercentColor(percent))}";

							case FermenterHoverMode.RemainingTime:
								float remaining = Mathf.Max(0f, totalTime - (float)timePassed);
								return $"{name} ( {contentName} )\n{localizedFermenting}: {PaintTextIfEnabled(FormatTime(remaining), Color.cyan)}";

							case FermenterHoverMode.PercentAndTime:
								float combinedPercent = Mathf.Clamp01((float)(timePassed / totalTime));
								string percentColored = PaintTextIfEnabled($"{combinedPercent:0%}", GetPercentColor(combinedPercent));

								float combinedRemaining = Mathf.Max(0f, totalTime - (float)timePassed);
								string timeColored = PaintTextIfEnabled(FormatTime(combinedRemaining), Color.cyan);

								return $"{name} ( {contentName} )\n{localizedFermenting}: {percentColored} - {timeColored}";
						}

						break;

					case "Ready":
						string readyContentName = (string)GetFermenterContentNameMethod.Invoke(fermenter, null);
						string useKeyColored = $"[{PaintText(Localization.instance.Localize("$KEY_Use"), Color.yellow)}]";
						string localizedReady = PaintTextIfEnabled(Localization.instance.Localize("$piece_fermenter_ready"), Color.green);
						string localizedTap = Localization.instance.Localize("$piece_fermenter_tap");

						return Localization.instance.Localize($"{name} ( {localizedReady} )\n{readyContentName}\n{useKeyColored} {localizedTap}");
				}

				return null;
			}
		}

		[HarmonyPatch(typeof(CookingStation), "Awake")]
		internal static class CookingStation_AddFoodSwitchHoverPatch
		{
			private static void Postfix(CookingStation __instance)
			{
				if (__instance == null) return;

				if (__instance.m_addFoodSwitch != null &&
					__instance.m_addFoodSwitch.m_onHover == null &&
					GetHoverTextMethod != null)
				{
					__instance.m_addFoodSwitch.m_onHover = () =>
					{
						string vanillaText = Localization.instance.Localize(GetHoverTextMethod.Invoke(__instance, null) as string);

						if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off)
						{
							return vanillaText;
						}

						string hoverText = GetCookingStationHover(__instance);

						return !string.IsNullOrEmpty(hoverText)
							? hoverText
							: vanillaText;
					};
				}

				if (__instance.m_addFuelSwitch != null &&
					__instance.m_addFuelSwitch.m_onHover == null &&
					OnHoverFuelSwitchMethod != null &&
					GetCSFuelMethod != null)
				{
					__instance.m_addFuelSwitch.m_onHover = () =>
					{
						string vanillaText = Localization.instance.Localize(OnHoverFuelSwitchMethod.Invoke(__instance, null) as string);

						if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off)
						{
							return vanillaText;
						}

						float fuel = (float)GetCSFuelMethod.Invoke(__instance, null);
						float remainingSeconds = fuel * __instance.m_secPerFuel;

						if (remainingSeconds <= 0f)
						{
							return vanillaText;
						}

						return $"{vanillaText}\nTime Left: {PaintTextIfEnabled(FormatTime(remainingSeconds), Color.cyan)}";
					};
				}
			}
		}

		[HarmonyPatch(typeof(CookingStation), "GetHoverText")]
		internal static class CookingStationHoverPatch
		{
			private static bool Prefix(CookingStation __instance, ZNetView ___m_nview, ref string __result)
			{
				if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off) return true;
				if (___m_nview == null || !___m_nview.IsOwner()) return true;

				bool isOven = __instance.m_useFuel && !__instance.m_requireFire;
				if (isOven) return true;

				string hoverText = GetCookingStationHover(__instance);

				if (hoverText == null) return true;

				__result = hoverText;
				return false;
			}
		}

		private static string GetCookingStationHover(CookingStation station)
		{
			if (GetSlotMethod == null || GetItemConversionMethod == null)
			{
				log.Warn("CookingStation reflection methods not found.");
				return null;
			}

			string stationName = Localization.instance.Localize(station.m_name);
			string slotInfo = "";

			int activeSlots = 0;
			bool hasReadyItem = false;
			bool hasOvercookedItem = false;

			for (int i = 0; i < station.m_slots.Length; i++)
			{
				object[] arguments = { i, null, 0f, null, false };
				GetSlotMethod.Invoke(station, arguments);

				string itemName = arguments[1] as string;
				float cookedTime = (float)arguments[2];

				if (string.IsNullOrEmpty(itemName)) continue;

				if (itemName == station.m_overCookedItem?.name)
				{
					hasOvercookedItem = true;
					continue;
				}

				CookingStation.ItemConversion itemConversion = (CookingStation.ItemConversion)GetItemConversionMethod.Invoke(station, new object[] { itemName });
				if (itemConversion == null) continue;

				activeSlots++;

				if (cookedTime >= itemConversion.m_cookTime && cookedTime < itemConversion.m_cookTime * 2f)
				{
					hasReadyItem = true;
				}

				string displayText = BuildCookingSlotText(itemConversion, cookedTime, itemName);
				slotInfo += "\n" + displayText;
			}

			if (activeSlots == 0 && !hasOvercookedItem) return null;

			string useKeyColored = $"[{PaintText(Localization.instance.Localize("$KEY_Use"), Color.yellow)}]";
			string localizedCook = Localization.instance.Localize("$piece_cstand_cook");
			string localizedTake = Localization.instance.Localize("$piece_itemstand_take");

			if (hasReadyItem || hasOvercookedItem)
			{
				return $"{stationName}\n{useKeyColored} {localizedTake}{slotInfo}";
			}

			return activeSlots >= station.m_slots.Length
				? $"{stationName}{slotInfo}"
				: $"{stationName}\n{useKeyColored} {localizedCook} {slotInfo}";
		}

		private static string BuildCookingSlotText(CookingStation.ItemConversion conversion, float cookedTime, string currentItemName)
		{
			float cookTime = conversion.m_cookTime;
			float overCookTime = cookTime * 2f;

			GameObject currentPrefab = ObjectDB.instance.GetItemPrefab(currentItemName);

			string currentName = currentPrefab != null
				? Localization.instance.Localize(currentPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name)
				: currentItemName;

			switch (ConfigManager.EffectiveCookingStationHoverModeChoice)
			{
				case CookingStationHoverMode.Percent:
					if (cookedTime > cookTime)
					{
						float overcookPercent = Mathf.Clamp01((cookedTime - cookTime) / cookTime);
						return $"{currentName}: {PaintTextIfEnabled($"{overcookPercent:0%}", GetPercentColorInverted(overcookPercent))}";
					}

					float percent = Mathf.Clamp01(cookedTime / cookTime);
					return $"{currentName}: {PaintTextIfEnabled($"{percent:0%}", GetPercentColor(percent))}";

				case CookingStationHoverMode.RemainingTime:
					if (cookedTime > cookTime)
					{
						float overcookRemaining = Mathf.Max(0f, overCookTime - cookedTime);
						return $"{currentName}: {PaintTextIfEnabled(FormatTime(overcookRemaining), Color.red)}";
					}

					float remaining = Mathf.Max(0f, cookTime - cookedTime);
					return $"{currentName}: {PaintTextIfEnabled(FormatTime(remaining), Color.cyan)}";

				case CookingStationHoverMode.PercentAndTime:
					if (cookedTime > cookTime)
					{
						float overcookPercent = Mathf.Clamp01((cookedTime - cookTime) / cookTime);
						string overcookPercentColored = PaintTextIfEnabled($"{overcookPercent:0%}", GetPercentColorInverted(overcookPercent));

						float overcookRemaining = Mathf.Max(0f, overCookTime - cookedTime);
						string overcookTimeColored = PaintTextIfEnabled(FormatTime(overcookRemaining), Color.red);

						return $"{currentName}: {overcookPercentColored} - {overcookTimeColored}";
					}

					float combinedPercent = Mathf.Clamp01(cookedTime / cookTime);
					string percentColored = PaintTextIfEnabled($"{combinedPercent:0%}", GetPercentColor(combinedPercent));

					float combinedRemaining = Mathf.Max(0f, cookTime - cookedTime);
					string timeColored = PaintTextIfEnabled(FormatTime(combinedRemaining), Color.cyan);

					return $"{currentName}: {percentColored} - {timeColored}";
			}

			return null;
		}

		[HarmonyPatch(typeof(Smelter), "OnHoverAddOre")]
		internal static class SmelterHoverAddPatch
		{
			private static void Postfix(Smelter __instance, ref string __result)
			{
				if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off) return;

				__result = GetSmelterHover(__instance, __result);
			}
		}

		[HarmonyPatch(typeof(Smelter), "OnHoverAddFuel")]
		internal static class SmelterHoverFuelPatch
		{
			private static void Postfix(Smelter __instance, ref string __result)
			{
				if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off) return;

				__result = GetSmelterHover(__instance, __result);
			}
		}

		[HarmonyPatch(typeof(Smelter), "OnHoverEmptyOre")]
		internal static class SmelterHoverEmptyPatch
		{
			private static void Postfix(Smelter __instance, ref string __result)
			{
				if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off) return;

				__result = GetSmelterHover(__instance, __result);
			}
		}

		private static string GetSmelterHover(Smelter smelter, string result)
		{
			if (!smelter.IsActive()) return result;

			if (GetFuelMethod == null || GetQueueSizeMethod == null || GetBakeTimerMethod == null)
			{
				log.Warn("Smelter reflection methods not found.");
				return result;
			}

			float fuel = (float)GetFuelMethod.Invoke(smelter, null);
			int queueSize = (int)GetQueueSizeMethod.Invoke(smelter, null);
			float bakeTimer = (float)GetBakeTimerMethod.Invoke(smelter, null);

			if (queueSize <= 0) return result;

			float durationPerItem = smelter.m_secPerProduct;
			int fuelPerProduct = smelter.m_fuelPerProduct;

			int additionalItems;

			if (fuelPerProduct > 0)
			{
				int fullItemsFromFuel = Mathf.FloorToInt(fuel / fuelPerProduct);
				int maxAvailableAfterCurrent = Mathf.Max(0, queueSize - 1);

				additionalItems = Mathf.Clamp(fullItemsFromFuel, 0, maxAvailableAfterCurrent);
			}
			else
			{
				additionalItems = Mathf.Max(0, queueSize - 1);
			}

			float remainingCurrent = Mathf.Clamp(durationPerItem - bakeTimer, 0f, durationPerItem);
			float remainingSeconds = remainingCurrent + additionalItems * durationPerItem;

			float power = 1f;

			if (smelter.m_windmill != null)
			{
				power = Mathf.Max(smelter.m_windmill.GetPowerOutput(), 0.0001f);
				remainingSeconds /= power;
			}

			if (ConfigManager.EffectiveSmelterHoverModeChoice != SmelterHoverMode.RemainingTime)
			{
				return result;
			}

			string hover = "";

			if (smelter.m_windmill != null)
			{
				int percentPower = Mathf.RoundToInt(power * 100f);
				hover += $"\nWind: {PaintTextIfEnabled($"{percentPower}%", GetPercentColor(power))}";
			}

			hover += $"\n{PaintTextIfEnabled(FormatTime(remainingSeconds), Color.cyan)}";

			return Localization.instance.Localize($"{result} {hover}");
		}

		[HarmonyPatch(typeof(EggGrow), nameof(EggGrow.GetHoverText))]
		internal static class EggGrowHoverPatch
		{
			private static void Postfix(EggGrow __instance, ZNetView ___m_nview, ref string __result)
			{
				if (ConfigManager.EffectiveDetailedHoverInfoChoice == HoverInfoMode.Off) return;

				__result = GetEggHover(__instance, ___m_nview, __result);
			}

			private static string GetEggHover(EggGrow egg, ZNetView nview, string originalHover)
			{
				if (nview == null || !nview.IsValid()) return originalHover;

				ZDO zdo = nview.GetZDO();
				if (zdo == null) return originalHover;

				float growStart = zdo.GetFloat(ZDOVars.s_growStart);
				if (growStart <= 0f) return originalHover;

				double elapsed = ZNet.instance.GetTimeSeconds() - growStart;
				float growTime = egg.m_growTime;

				string creatureName = egg.m_grownPrefab?.GetComponent<Character>()?.m_name ??
					egg.m_grownPrefab?.name ??
					"Unknown";

				creatureName = Localization.instance.Localize(creatureName);

				if (elapsed >= growTime)
				{
					return $"{originalHover}\n{creatureName}: {PaintTextIfEnabled("Hatching soon", Color.green)}";
				}

				float percent = Mathf.Clamp01((float)(elapsed / growTime));
				float remaining = Mathf.Max(0f, growTime - (float)elapsed);

				string hover = "";

				switch (ConfigManager.EffectiveEggHoverModeChoice)
				{
					case EggHoverMode.RemainingTime:
						hover = $"{creatureName}: {PaintTextIfEnabled(FormatTime(remaining), Color.cyan)}";
						break;

					case EggHoverMode.Percent:
						hover = $"{creatureName}: {PaintTextIfEnabled($"{percent:0%}", GetPercentColor(percent))}";
						break;

					case EggHoverMode.PercentAndTime:
						string percentColored = PaintTextIfEnabled($"{percent:0%}", GetPercentColor(percent));
						string timeColored = PaintTextIfEnabled(FormatTime(remaining), Color.cyan);

						hover = $"{creatureName}: {percentColored} - {timeColored}";
						break;
				}

				return $"{originalHover}\n{hover}";
			}
		}

		private static string FormatTime(float seconds)
		{
			int totalSeconds = Mathf.FloorToInt(seconds);

			int hours = totalSeconds / 3600;
			int minutes = totalSeconds % 3600 / 60;
			int remainingSeconds = totalSeconds % 60;

			if (hours > 0)
			{
				return $"{hours}h {minutes}m {remainingSeconds}s";
			}

			if (minutes > 0)
			{
				return $"{minutes}m {remainingSeconds}s";
			}

			return $"{remainingSeconds}s";
		}

		private static Color GetPercentColor(float percent)
		{
			return percent < 0.5f
				? Color.Lerp(Color.red, Color.yellow, percent / 0.5f)
				: Color.Lerp(Color.yellow, Color.green, (percent - 0.5f) / 0.5f);
		}

		private static Color GetPercentColorInverted(float percent)
		{
			return percent < 0.5f
				? Color.Lerp(Color.green, Color.yellow, percent / 0.5f)
				: Color.Lerp(Color.yellow, Color.red, (percent - 0.5f) / 0.5f);
		}

		private static string PaintText(string text, Color color)
		{
			string hex = ColorUtility.ToHtmlStringRGBA(color);
			return $"<color=#{hex}>{text}</color>";
		}

		internal static string PaintTextIfEnabled(string text, Color color, bool bold = false)
		{
			if (ConfigManager.EffectiveDetailedHoverInfoChoice != HoverInfoMode.ColoredText)
			{
				return bold ? $"<b>{text}</b>" : text;
			}

			string colored = PaintText(text, color);

			return bold
				? $"<b>{colored}</b>"
				: colored;
		}

		private static bool HasContainerAccess(Container container)
		{
			if (container == null || Player.m_localPlayer == null) return false;

			if (ContainerCheckAccessMethod == null)
			{
				log.Warn("Could not find Container.CheckAccess.");
				return false;
			}

			return (bool)ContainerCheckAccessMethod.Invoke(container, new object[] { Player.m_localPlayer.GetPlayerID() });
		}
	}
}