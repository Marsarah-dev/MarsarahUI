using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ZNet;

namespace MarsarahUI.Patches.UI
{
	internal class UIOnlinePlayers : UIController
	{
		private static readonly LogManager log = new LogManager("UI Online Players", LogManager.LogLevel.Warning);

		private static readonly List<PlayerInfo> playerInfoList = new List<PlayerInfo>();

		private const int NumOnlinePlayerSlots = 21;
		private const float UIPartyPlayerTextDistanceV = -25f;

		private static GameObject UIPartyArea;
		private static readonly List<Text> UIPlayerTexts = new List<Text>();

		private static bool wasEnabled;

		[HarmonyPatch(typeof(ZNet), "Update")]
		private static class OnlinePartyIndicator_Patch
		{
			private static void Prefix(ref List<PlayerInfo> ___m_players)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (!ConfigManager.EffectiveShowOnlinePlayers) return;
				if (___m_players == null) return;

				playerInfoList.Clear();
				playerInfoList.AddRange(___m_players);
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class OnlinePlayers_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				if (!ConfigManager.EffectiveShowOnlinePlayers)
				{
					if (wasEnabled)
					{
						if (UIPartyArea != null && UIPartyArea.activeSelf)
						{
							UIPartyArea.SetActive(false);
						}

						HidePlayerTexts();
						wasEnabled = false;

						log.Info("Online players UI hidden.");
					}

					return;
				}

				wasEnabled = true;

				CreateUI(__instance);

				if (Player.m_localPlayer == null || UIPartyArea == null) return;

				int numPlayersTotal = playerInfoList.Count;

				bool isUIHidden = Hud.IsUserHidden();
				bool isLoadScreenActive = IsLoadScreenActive(__instance);
				bool chatVisible = Chat.instance != null && Chat.instance.IsChatDialogWindowVisible();

				bool shouldShowArea =
					ShowUI &&
					!isUIHidden &&
					!isLoadScreenActive &&
					numPlayersTotal > 1;

				if (UIPartyArea.activeSelf != shouldShowArea)
				{
					UIPartyArea.SetActive(shouldShowArea);
				}

				if (!shouldShowArea)
				{
					HidePlayerTexts();
					return;
				}

				int numPlayersToFit = Mathf.Min(numPlayersTotal, NumOnlinePlayerSlots - 1);

				bool shouldShowHeader = !chatVisible;
				bool shouldShowPlayerList = ShowPlayerList && !chatVisible;

				UIPlayerTexts[0].enabled = shouldShowHeader;
				UIPlayerTexts[0].color = Color.green;
				UIPlayerTexts[0].text = $"Online: {numPlayersTotal}";

				for (int i = 1; i < NumOnlinePlayerSlots; i++)
				{
					if (i <= numPlayersToFit)
					{
						UIPlayerTexts[i].enabled = shouldShowPlayerList;
						UIPlayerTexts[i].color = Color.white;
						UIPlayerTexts[i].text = playerInfoList[i - 1].m_name;
					}
					else
					{
						UIPlayerTexts[i].enabled = false;
						UIPlayerTexts[i].text = "";
					}
				}
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UIPartyArea != null &&
				UIPlayerTexts.Count == NumOnlinePlayerSlots &&
				UIPlayerTexts.All(text => text != null))
			{
				return;
			}

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			Vector2 UIPartyAreaSize = new Vector2(120f, 30f);

			UIPartyArea = new GameObject("PartyArea");
			UIPartyArea.SetActive(false);
			UIPartyArea.layer = 5;
			UIPartyArea.transform.SetParent(hud.m_rootObject.transform.parent, false);

			RectTransform partyAreaTransform = UIPartyArea.AddComponent<RectTransform>();
			partyAreaTransform.anchorMin = new Vector2(1f, 0f);
			partyAreaTransform.anchorMax = new Vector2(1f, 0f);
			partyAreaTransform.pivot = new Vector2(1f, 0f);
			partyAreaTransform.anchoredPosition = new Vector2(-20f, 70f);
			partyAreaTransform.sizeDelta = UIPartyAreaSize;

			UIPartyArea.transform.localScale = Vector3.one;

			UIPlayerTexts.Clear();

			for (int i = 0; i < NumOnlinePlayerSlots; i++)
			{
				Vector2 anchoredPosition = new Vector2(0f, -UIPartyPlayerTextDistanceV * i);

				Text playerText = CreateTextObject($"PartyText_{i}", UIPartyArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, anchoredPosition, UIPartyAreaSize);

				playerText.enabled = false;
				UIPlayerTexts.Add(playerText);
			}
		}

		private static void HidePlayerTexts()
		{
			foreach (Text playerText in UIPlayerTexts)
			{
				if (playerText != null)
				{
					playerText.enabled = false;
				}
			}
		}

		private static bool IsLoadScreenActive(Hud hud)
		{
			return Hud.instance && hud.m_loadingScreen && hud.m_loadingScreen.gameObject.activeSelf;
		}
	}
}