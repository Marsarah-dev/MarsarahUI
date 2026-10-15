using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UINotifications : UIController
	{
		private static readonly LogManager log = new LogManager("UI Notifications", LogManager.LogLevel.Info);

		private static GameObject notificationRoot;
		private static RectTransform notificationRootRect;
		private static VerticalLayoutGroup notificationLayout;

		private static readonly List<NotificationEntry> notifications = new List<NotificationEntry>();

		private const float AnimationDuration = 0.2f;
		private const float SlideDistance = 20f;
		private const float NotificationSpacing = 4f;
		private const float NotificationWidth = 260f;
		private const float NotificationHeight = 36f;
		private const float IconSize = 28f;

		private class NotificationEntry
		{
			internal GameObject Root;
			internal RectTransform Rect;
			internal LayoutElement Layout;
			internal CanvasGroup CanvasGroup;
			internal Image Icon;
			internal Text MessageText;
			internal Text AmountText;

			internal string Key;
			internal int Amount;
			internal float Lifetime;
			internal float Progress;
			internal bool Leaving;
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class NotificationHudAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				CreateUI(__instance);
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		private static class NotificationHudUpdatePatch
		{
			private static void Postfix()
			{
				if (notificationRoot == null) return;

				UpdateNotifications();
			}
		}

		[HarmonyPatch(typeof(Character), "Message")]
		private static class CharacterMessagePatch
		{
			private static bool Prefix(Character __instance, MessageHud.MessageType type, string msg)
			{
				if (ConfigManager.EffectiveNotificationModeChoice == ConfigManager.NotificationMode.Vanilla)
				{
					return true;
				}

				if (__instance != Player.m_localPlayer)
				{
					return true;
				}

				if (type != MessageHud.MessageType.TopLeft)
				{
					return true;
				}

				if (string.IsNullOrEmpty(msg))
				{
					return true;
				}

				if (msg.StartsWith("$msg_dropped "))
				{
					return false;
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(Character), nameof(Character.ShowPickupMessage))]
		private static class PickupMessagePatch
		{
			private static bool Prefix(Character __instance, ItemDrop.ItemData item, int amount)
			{
				if (ConfigManager.EffectiveNotificationModeChoice == ConfigManager.NotificationMode.Vanilla)
				{
					return true;
				}

				if (__instance != Player.m_localPlayer)
				{
					return true;
				}

				if (item == null || item.m_shared == null)
				{
					log.Warn("Pickup notification received without valid item data. Falling back to vanilla notification.");
					return true;
				}

				AddLootNotification(item, amount);

				return false;
			}
		}

		[HarmonyPatch(typeof(Humanoid), "DropItem", new System.Type[] { typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int) })]
		private static class HumanoidDropItemPatch
		{
			private static void Prefix(Humanoid __instance, Inventory inventory, ItemDrop.ItemData item, int amount)
			{
				if (ConfigManager.EffectiveNotificationModeChoice == ConfigManager.NotificationMode.Vanilla)
				{
					return;
				}

				if (__instance != Player.m_localPlayer)
				{
					return;
				}

				if (item == null || item.m_shared == null)
				{
					log.Warn("Dropped item notification received without valid item data.");
					return;
				}

				AddRemovedNotification(item, amount);
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (notificationRoot != null) return;

			notificationRoot = new GameObject("NotificationStack");
			notificationRoot.layer = 5;
			notificationRoot.transform.SetParent(hud.m_rootObject.transform, false);

			notificationRootRect = notificationRoot.AddComponent<RectTransform>();
			notificationRootRect.anchorMin = new Vector2(0f, 1f);
			notificationRootRect.anchorMax = new Vector2(0f, 1f);
			notificationRootRect.pivot = new Vector2(0f, 1f);

			notificationLayout = notificationRoot.AddComponent<VerticalLayoutGroup>();
			notificationLayout.spacing = NotificationSpacing;
			notificationLayout.childAlignment = TextAnchor.UpperLeft;
			notificationLayout.childControlWidth = false;
			notificationLayout.childControlHeight = false;
			notificationLayout.childForceExpandWidth = false;
			notificationLayout.childForceExpandHeight = false;

			ContentSizeFitter sizeFitter = notificationRoot.AddComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			ApplyNotificationTransform();

			UIStyleManager.StyleChanged -= ApplyStyle;
			UIStyleManager.StyleChanged += ApplyStyle;

			RefreshFromConfig();

			log.Info("Created notification stack.");
		}

		private static void UpdateNotifications()
		{
			bool enhanced = ConfigManager.EffectiveNotificationModeChoice == ConfigManager.NotificationMode.Enhanced;

			if (notificationRoot.activeSelf != enhanced && ShowUI)
			{
				notificationRoot.SetActive(enhanced);
			}

			if (!enhanced || !ShowUI)
			{
				if (notificationRoot.activeSelf)
				{
					notificationRoot.SetActive(false);
				}

				return;
			}

			if (!notificationRoot.activeSelf)
			{
				notificationRoot.SetActive(true);
			}

			for (int i = notifications.Count - 1; i >= 0; i--)
			{
				NotificationEntry notification = notifications[i];

				if (notification.Root == null)
				{
					notifications.RemoveAt(i);
					continue;
				}

				notification.Lifetime -= Time.deltaTime;

				if (notification.Lifetime <= 0f)
				{
					Object.Destroy(notification.Root);
					notifications.RemoveAt(i);
				}
			}
		}

		internal static void RefreshFromConfig()
		{
			if (notificationRoot == null) return;

			bool enhanced = ConfigManager.EffectiveNotificationModeChoice == ConfigManager.NotificationMode.Enhanced;

			if (!enhanced)
			{
				ClearNotifications();
			}

			notificationRoot.SetActive(enhanced && ShowUI);

			if (enhanced)
			{
				TrimNotifications();
			}

			log.Info($"Notification mode changed to {ConfigManager.EffectiveNotificationModeChoice}.");
		}

		internal static void ApplyNotificationTransform()
		{
			if (notificationRootRect == null) return;

			float scale = ConfigManager.EffectiveNotificationScale / 100f;

			notificationRootRect.anchoredPosition = ConfigManager.EffectiveNotificationPosition;
			notificationRootRect.localScale = new Vector3(scale, scale, 1f);

			log.Info($"Applied notification transform: position {ConfigManager.EffectiveNotificationPosition}, scale {ConfigManager.EffectiveNotificationScale}%.");
		}

		private static void ApplyStyle()
		{
			if (notificationRoot == null) return;

			// Notification cards will be restyled here once entries exist.
		}

		private static void ClearNotifications()
		{
			for (int i = notifications.Count - 1; i >= 0; i--)
			{
				NotificationEntry notification = notifications[i];

				if (notification.Root != null)
				{
					Object.Destroy(notification.Root);
				}
			}

			notifications.Clear();
		}

		private static void AddItemNotification(ItemDrop.ItemData item, int amount)
		{
			if (notificationRoot == null)
			{
				log.Warn("Could not show loot notification because the notification stack has not been created.");
				return;
			}

			int uiTextFontSize = 16;
			string uiTextFontName = "AveriaSansLibre-Bold";

			GameObject root = new GameObject("LootNotification");
			root.layer = 5;
			root.transform.SetParent(notificationRoot.transform, false);

			RectTransform rect = root.AddComponent<RectTransform>();
			rect.sizeDelta = new Vector2(NotificationWidth, NotificationHeight);
			rect.localScale = Vector3.one;

			LayoutElement layout = root.AddComponent<LayoutElement>();
			layout.preferredWidth = NotificationWidth;
			layout.preferredHeight = NotificationHeight;
			layout.minWidth = NotificationWidth;
			layout.minHeight = NotificationHeight;

			CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;

			UIStyleDefinition style = UIStyleManager.Current;

			CreateStyledBackground("Background", root, style.BackgroundType, style.BackgroundColor, style.VanillaBackgroundSprite, log);

			CreateStyledBorder(
				"Border",
				root,
				style.BorderType,
				style.BorderCapAsset,
				style.BorderColor,
				style.BorderCapSize,
				style.BorderOuterColor,
				style.BorderInnerColor,
				style.BorderOuterWidth,
				style.BorderInnerWidth,
				log);

			Image icon = CreateUIImageObject("Icon", root, Vector2.zero, new Vector2(IconSize, IconSize));
			icon.sprite = item.GetIcon();
			icon.preserveAspect = true;
			icon.raycastTarget = false;

			RectTransform iconRect = icon.rectTransform;
			SetRectAnchors(iconRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f));
			iconRect.anchoredPosition = new Vector2(19f, 0f);

			string itemName = Localization.instance.Localize(item.m_shared.m_name);

			Text messageText = CreateTextObject("ItemName", root, Color.white, uiTextFontName, uiTextFontSize, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);

			messageText.text = itemName;
			messageText.raycastTarget = false;

			RectTransform messageRect = messageText.rectTransform;
			SetRectAnchors(messageRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f));
			messageRect.offsetMin = new Vector2(38f, 0f);
			messageRect.offsetMax = new Vector2(-50f, 0f);

			Text amountText = CreateTextObject("Amount", root, Color.white, uiTextFontName, uiTextFontSize, TextAnchor.MiddleRight, Vector2.zero, Vector2.zero);

			amountText.text = amount > 0 ? $"+{amount}" : amount.ToString();
			amountText.raycastTarget = false;

			RectTransform amountRect = amountText.rectTransform;
			SetRectAnchors(amountRect, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f));
			amountRect.anchoredPosition = new Vector2(-8f, 0f);
			amountRect.sizeDelta = new Vector2(44f, 0f);

			NotificationEntry notification = new NotificationEntry
			{
				Root = root,
				Rect = rect,
				Layout = layout,
				CanvasGroup = canvasGroup,
				Icon = icon,
				MessageText = messageText,
				AmountText = amountText,
				Key = item.m_shared.m_name,
				Amount = amount,
				Lifetime = ConfigManager.EffectiveNotificationDuration,
				Progress = 1f,
				Leaving = false
			};

			notifications.Add(notification);

			TrimNotifications();

			log.Info($"Added item notification: {item.m_shared.m_name} x{amount}.");
		}

		private static void AddLootNotification(ItemDrop.ItemData item, int amount)
		{
			AddItemNotification(item, amount);
		}

		private static void AddRemovedNotification(ItemDrop.ItemData item, int amount)
		{
			AddItemNotification(item, -amount);
		}

		private static void SetRectAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
		{
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = pivot;
		}

		private static void TrimNotifications()
		{
			int maxVisible = Mathf.Max(1, ConfigManager.EffectiveNotificationMaxVisible);

			while (notifications.Count > maxVisible)
			{
				NotificationEntry oldest = notifications[0];

				if (oldest.Root != null)
				{
					Object.Destroy(oldest.Root);
				}

				notifications.RemoveAt(0);
			}
		}
	}
}