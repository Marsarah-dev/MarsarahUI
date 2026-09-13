using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;

namespace MarsarahUI.Patches.UI
{
	internal class UILoadscreenTips
	{
		private static readonly LogManager log = new LogManager("UI Loadscreen", LogManager.LogLevel.Warning);

		private static string loadingTipString;
		private static string localizationLanguage;
		private static readonly List<string> loadingTipStrings = new List<string>
		{
			"Weapons and armor can be crafted and upgraded using Workbenches or Forges.",
			"Eating food increases your health and stamina pools. Try to balance your three food slots accordingly.",
			"Crafting stations can be upgraded by building extensions in their proximity.",
			"There are four principal crafting types: woodworking, masonry, smithing, and cooking.",
			"Enemies won't spawn near player built structures, such as crafting tables and camp fires.",
			"Sitting is the safest way to travel while on a boat.",
			"Upon your arrival in Valheim, try to find stones and branches on the ground to build your first weapon.",
			"Venturing outside at night is more dangerous. Be prepared if you plan to go out during that time.",
			"Hunting deer with a ranged weapon provides a good source of meat and skin.",
			"Build a rudimentary shelter as soon as possible, to take refuge during your first nights. It can be used as a temporary home until you find a good spot for a better one.",
			"Different enemies are resistant to different damage types. If you see grey damage indicators when attacking, try changing the weapon.",
			"Claiming a bed after building it will set your spawn point to its location when dying.",
			"Attacking with knives and bows while sneaking confers a large damage bonus.",
			"Birds are a good source of feathers. They will stay close to the ground while the weather is bad.",
			"Weapons, armor, and tools can be repaired for free at a Workbench or Forge by interacting with the hammer icon while crafting.",
			"Some resources require better tools to harvest them.",
			"Being rested helps to regenerate stamina and health faster.",
			"Yellow damage numbers indicate a weakness, grey numbers indicate a resistance.",
			"Use the hoe in combination with the pickaxe to flatten the ground. This will make it much easier to construct buildings.",
			"Build a cart to haul heavy goods over longer distances. Carts cannot go through portals.",
			"Boats are controlled by interacting with the steering oar.",
			"Defeating a Forsaken will drop items that will give you a general idea of what to do next.",
			"Cooking meat over a fire or eating fruit is a good way to get fed. Later on, they can be cooked using a cauldron.",
			"You can filter map pins by type by right clicking on them. (DPad Right)",
			"If no arrow has been manually selected when using a bow, arrows are automatically selected in ascending order beginning from the leftmost column and going right.",
			"Build a Fermenter to craft different potions from base meads. The right potion used at the right time and place can be the difference between life and death.",
			"Killing bee nests can provide the means to make your own honey.",
			"The more comfort items are installed in your home (e.g., fire, rugs, furniture, etc), the longer the rested bonus will be.",
			"When fighting a Forsaken, make sure you bring your best gear, food, and potions.",
			"All build pieces require a certain crafting station nearby (e.g., Workbench, Forge, Stonecutter). If you want to build outside their range, either move them closer or build new ones.",
			"Building paths and roads can make carrying material with a cart a lot easier.",
			"Item stacks can be split using SHIFT + Left Click in chests or inventory.",
			"Item stacks can be fully transferred to/from containers by dragging or pressing CTRL + Left Click.",
			"If you run out of stamina while swimming, you will drown.",
			"The HUD can be toggled with the CTRL + F3 keys.",
			"Parrying with a shield or weapon means blocking at the very last moment of an enemy attack before hitting you. If done right, the enemy will be staggered.",
			"Don't be afraid to retreat from a fight. It is better to come back to it prepared than naked.",
			"Sheathing or unsheathing equipped weapons and shields can be done by pressing R. Walking and running with unsheathed weapons will slow you down.",
			"Old buildings can be found throughout Valheim. They can be either renovated for shelter, or torn down for resources.",
			"Some animals can be tamed.",
			"Sitting next to a lit fire in any open space provides a level 1 rested bonus.",
			"Holding the interact key on a container will place matching item stacks from your inventory into it.",
			"The rested effect also increases skill experience gain, making it worthwhile to stay rested while exploring, fighting, and gathering.",
			"Your rested duration is determined by the highest comfort level you've recently rested at, so improving your home has benefits even after you leave it.",
			"Blocking is more effective when you have enough health to withstand the incoming attack. Strong enemies can stagger you through an insufficient block.",
			"Portals cannot normally transport metals or ores, but processed equipment made from those materials can pass through them.",
			"Different shields are suited to different playstyles. Bucklers reward well-timed parries, while tower shields favor stronger regular blocking.",
			"Enemy stars greatly increase their health and damage, but starred enemies also provide more resources when defeated.",
			"Sneaking uses stamina while moving near enemies. The eye indicator shows how visible you currently are.",
			"You can mark locations on the map and name your pins. Marking crypts, caves, resources, and unfinished areas can save a lot of searching later.",
			"Hold the Shift key while building to disable snapping temporarily, allowing pieces to be positioned more freely."
		};

		[HarmonyPatch(typeof(Localization), "SetupLanguage")]
		class LoadingTips_Patch
		{
			private static void Postfix(Localization __instance, string language)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				localizationLanguage = language;
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class Loadscreens_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (ConfigManager.EffectiveBetterLoadingTipsEnabled)
				{
					loadingTipString = loadingTipStrings[UnityEngine.Random.Range(0, loadingTipStrings.Count)];
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "UpdateBlackScreen")]
		private static class ExtraLoadingTipsUpdate_Patch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;
				if (!ConfigManager.EffectiveBetterLoadingTipsEnabled || localizationLanguage != "English") return;

				if (string.IsNullOrEmpty(loadingTipString))
				{
					loadingTipString = loadingTipStrings[UnityEngine.Random.Range(0, loadingTipStrings.Count)];
				}

				__instance.m_loadingTip.text = loadingTipString;
			}
		}
	}
}
