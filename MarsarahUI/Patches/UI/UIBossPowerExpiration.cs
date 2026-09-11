using HarmonyLib;
using MarsarahUI.Managers;

namespace MarsarahUI.Patches.UI
{
	internal class UIBossPowerExpiration
	{
		private static readonly LogManager log = new LogManager("UI Boss Power Expire", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.Stop))]
		private static class StatusEffectStopPatch
		{
			private static void Postfix(StatusEffect __instance)
			{
				if (!ConfigManager.EffectiveShowBossExpirationMessage) return;

				Character character = __instance.m_character;
				if (character == null || !character.IsPlayer() || !character.IsOwner()) return;

				string statusEffectName = __instance.name;

				if (statusEffectName.Contains("GP_"))
				{
					string powerName = Localization.instance.Localize(__instance.m_name);

					log.Info($"Forsaken Power expired: {powerName}");
					character.Message(MessageHud.MessageType.Center, $"{powerName} Power Expired");
				}
			}
		}
	}
}