using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahUI.Patches.UI
{
	internal class UISummonCounter
	{
		private static readonly LogManager log = new LogManager("UI Summon Counter", LogManager.LogLevel.Warning);

		internal static int NumSummons { get; private set; }

		[HarmonyPatch(typeof(Player), "Update")]
		private static class SummonCounterPlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (___m_localPlayer == null) return;
				if (!ConfigManager.EffectiveShowSummonCounter) return;
				if (!UIController.ShowUI) return;

				List<Character> allCharacters = Character.GetAllCharacters();
				int numSummonedSkeletons = 0;

				foreach (Character character in allCharacters)
				{
					if (!character.IsTamed()) continue;
					if (!character.name.Contains("Skeleton_Friendly")) continue;

					MonsterAI monsterAI = character.GetComponent<MonsterAI>();
					if (monsterAI == null) continue;

					GameObject followTarget = monsterAI.GetFollowTarget();
					if (followTarget == null) continue;

					Player targetPlayer = followTarget.GetComponent<Player>();
					if (targetPlayer != ___m_localPlayer) continue;

					numSummonedSkeletons++;
				}

				NumSummons = numSummonedSkeletons;
			}
		}
	}
}