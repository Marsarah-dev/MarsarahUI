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

		[HarmonyPatch(typeof(MessageHud), "ShowPickupMessage")]
		private static class PickupMessagePatch
		{
			private static bool Prefix()
			{
				if (ConfigManager.EffectiveNotificationModeChoice == ConfigManager.NotificationMode.Vanilla)
				{
					return true;
				}

				// Convert vanilla pickup into our notification here.

				return false;
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
			if (ConfigManager.EffectiveNotificationModeChoice != ConfigManager.NotificationMode.Enhanced)
			{
				return;
			}

			// Notification lifetime and animation handling will go here.
		}

		internal static void RefreshFromConfig()
		{
			if (notificationRoot == null) return;

			bool enhanced = ConfigManager.EffectiveNotificationModeChoice == ConfigManager.NotificationMode.Enhanced;

			if (!enhanced)
			{
				ClearNotifications();
			}

			notificationRoot.SetActive(enhanced);

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
	}
}