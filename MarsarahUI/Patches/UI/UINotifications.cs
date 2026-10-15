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

		private static int pendingItemMessagesToSuppress;

		private enum NotificationType
		{
			Item,
			Skill,
			Info
		}

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

		[HarmonyPatch]
		private static class MessageHudShowMessagePatch
		{
			private static IEnumerable<System.Reflection.MethodBase> TargetMethods()
			{
				foreach (System.Reflection.MethodInfo method in AccessTools.GetDeclaredMethods(typeof(MessageHud)))
				{
					if (method.Name == "ShowMessage")
					{
						yield return method;
					}
				}
			}

			private static bool Prefix(System.Reflection.MethodBase __originalMethod, object[] __args)
			{
				if (ConfigManager.EffectiveNotificationModeChoice == ConfigManager.NotificationMode.Vanilla)
				{
					return true;
				}

				if (__args == null || __args.Length < 2)
				{
					return true;
				}

				if (!(__args[0] is MessageHud.MessageType type))
				{
					return true;
				}

				if (type != MessageHud.MessageType.TopLeft)
				{
					return true;
				}

				if (!(__args[1] is string message) || string.IsNullOrEmpty(message))
				{
					return true;
				}

				HandleTopLeftMessage(__originalMethod, __args, message);

				return false;
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
				pendingItemMessagesToSuppress++;
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

		private static void HandleTopLeftMessage(System.Reflection.MethodBase originalMethod, object[] args, string message)
		{
			if (pendingItemMessagesToSuppress > 0)
			{
				pendingItemMessagesToSuppress--;
				log.Info($"Suppressed vanilla TopLeft item message '{message}' because a rich item notification was already created.");
				return;
			}

			int amount = 0;
			Sprite icon = null;

			for (int i = 2; i < args.Length; i++)
			{
				if (args[i] is int intValue)
				{
					amount = intValue;
				}
				else if (args[i] is Sprite spriteValue)
				{
					icon = spriteValue;
				}
			}

			log.Info($"Intercepted TopLeft notification '{message}' through {originalMethod.Name}. Amount: {amount}, Icon: {(icon != null ? icon.name : "none")}.");

			AddGenericNotification(message, icon, amount);
		}

		private static void AddGenericNotification(string message, Sprite icon = null, int amount = 0)
		{
			if (notificationRoot == null)
			{
				log.Warn("Could not show notification because the notification stack has not been created.");
				return;
			}

			string localizedMessage = Localization.instance.Localize(message);

			AddNotification(NotificationType.Info, null, localizedMessage, icon, amount);
		}

		private static void AddItemNotification(ItemDrop.ItemData item, int amount)
		{
			if (item == null || item.m_shared == null)
			{
				log.Warn("Could not create item notification because item data was invalid.");
				return;
			}

			string itemName = Localization.instance.Localize(item.m_shared.m_name);

			AddNotification(NotificationType.Item, item.m_shared.m_name, itemName, item.GetIcon(), amount);
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