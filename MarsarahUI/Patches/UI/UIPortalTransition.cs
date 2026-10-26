
using HarmonyLib;
using MarsarahUI.Managers;
using UnityEngine;
using System.Collections.Generic;

namespace MarsarahUI.Patches.UI
{
	internal class UIPortalTransition
	{
		private static readonly LogManager log = new LogManager("UI Portal Transition", LogManager.LogLevel.Info);

		private static Hud lastHud;
		private static bool hasLastState;

		private static bool lastTeleporting;
		private static bool lastBlackScreenVisible;
		private static bool lastTeleportProgressVisible;
		private static bool lastLoadingProgressVisible;
		private static bool lastLoadingImageVisible;

		[HarmonyPatch(typeof(Hud), "UpdateBlackScreen")]
		private static class PortalTransition_HUDPatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				Player player = Player.m_localPlayer;

				bool teleporting = player != null && player.IsTeleporting();

				CanvasGroup blackScreen = __instance.m_loadingScreen;

				float blackScreenAlpha = blackScreen != null ? blackScreen.alpha : 0f;

				bool blackScreenVisible = blackScreen != null &&
										  blackScreen.gameObject.activeInHierarchy &&
										  blackScreenAlpha > 0.01f;

				bool teleportProgressVisible = __instance.m_teleportingProgress != null &&
											   __instance.m_teleportingProgress.activeInHierarchy;

				bool loadingProgressVisible = __instance.m_loadingProgress != null &&
											  __instance.m_loadingProgress.activeInHierarchy;

				bool loadingImageVisible = __instance.m_loadingImage != null &&
										   __instance.m_loadingImage.gameObject.activeInHierarchy &&
										   __instance.m_loadingImage.enabled;

				// Reset the tracked state when the HUD is recreated.
				if (lastHud != __instance)
				{
					lastHud = __instance;
					hasLastState = false;
				}

				// Log only when one of the monitored states changes.
				if (hasLastState &&
					lastTeleporting == teleporting &&
					lastBlackScreenVisible == blackScreenVisible &&
					lastTeleportProgressVisible == teleportProgressVisible &&
					lastLoadingProgressVisible == loadingProgressVisible &&
					lastLoadingImageVisible == loadingImageVisible)
				{
					return;
				}

				hasLastState = true;
				lastTeleporting = teleporting;
				lastBlackScreenVisible = blackScreenVisible;
				lastTeleportProgressVisible = teleportProgressVisible;
				lastLoadingProgressVisible = loadingProgressVisible;
				lastLoadingImageVisible = loadingImageVisible;

				log.Info(
					$"Teleporting: {teleporting} | " +
					$"Black Screen: {blackScreenVisible} (Alpha: {blackScreenAlpha:F2}) | " +
					$"Teleport Progress: {teleportProgressVisible} | " +
					$"Loading Progress: {loadingProgressVisible} | " +
					$"Loading Image: {loadingImageVisible}"
				);
			}
		}


		[HarmonyPatch(typeof(Player), "TeleportTo")]
		private static class PortalTransition_TeleportToPatch
		{
			private static void Postfix(Player __instance, Vector3 pos, bool distantTeleport, bool __result)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null || __instance != Player.m_localPlayer) return;

				try
				{
					WorldGenerator generator = WorldGenerator.instance;

					string sourceBiome = __instance.GetCurrentBiome().ToString();

					string destinationBiome = generator != null
						? generator.GetBiome(pos).ToString()
						: "Unknown";

					string destinationAltBiomes = "None";

					if (generator != null)
					{
						BiomeSector sector = generator.GetBiomeSector(pos, true);

						if (sector != null && sector.AltBiomes != null)
						{
							List<string> altBiomeNames = new List<string>();

							foreach (AltBiome altBiome in sector.AltBiomes)
							{
								if (altBiome != null && !string.IsNullOrEmpty(altBiome.m_name))
								{
									altBiomeNames.Add(altBiome.m_name);
								}
							}

							if (altBiomeNames.Count > 0)
							{
								destinationAltBiomes = string.Join(", ", altBiomeNames);
							}
						}
					}

					log.Info(
						$"Teleport Started | " +
						$"Accepted: {__result} | " +
						$"Source: {sourceBiome} | " +
						$"Destination: {destinationBiome} | " +
						$"Alt Biome: {destinationAltBiomes} | " +
						$"Position: {pos} | " +
						$"Distant: {distantTeleport}"
					);
				}
				catch (System.Exception ex)
				{
					log.Error($"Failed to inspect teleport destination: {ex}");
				}
			}
		}

	}
}
