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

			internal NotificationType Type;
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

			string localizedMessage = Localization.instance.Localize(message);

			if (amount != 0)
			{
				if (localizedMessage.StartsWith("Dropped "))
				{
					amount = -Mathf.Abs(amount);
				}
				else if (localizedMessage.StartsWith("Added "))
				{
					amount = Mathf.Abs(amount);
				}
			}

			NotificationType notificationType = NotificationType.Info;
			string notificationKey = null;

			if (amount != 0 && icon != null)
			{
				if (localizedMessage.StartsWith("Added ") || localizedMessage.StartsWith("Dropped "))
				{
					notificationType = NotificationType.Item;
					notificationKey = localizedMessage;
				}
			}

			log.Info($"Intercepted TopLeft notification '{localizedMessage}' through {originalMethod.Name}. Amount: {amount}, Icon: {(icon != null ? icon.name : "none")}.");

			AddGenericNotification(localizedMessage, icon, amount, false, notificationType, notificationKey);
		}

		private static void AddNotification(NotificationType type, string key, string message, Sprite icon, int amount)
		{
			if (notificationRoot == null)
			{
				log.Warn("Could not show notification because the notification stack has not been created.");
				return;
			}

			if (type == NotificationType.Item && !string.IsNullOrEmpty(key))
			{
				NotificationEntry existingNotification = notifications.Find(notification =>
					notification.Type == NotificationType.Item &&
					notification.Key == key &&
					!notification.Leaving &&
					notification.Root != null);

				if (existingNotification != null)
				{
					existingNotification.Amount += amount;
					existingNotification.Lifetime = ConfigManager.EffectiveNotificationDuration;

					if (existingNotification.AmountText != null)
					{
						existingNotification.AmountText.text = existingNotification.Amount > 0
							? $"+{existingNotification.Amount}"
							: existingNotification.Amount.ToString();
					}

					log.Info($"Merged item notification '{message}'. New amount: {existingNotification.Amount}.");

					return;
				}
			}

			int uiTextFontSize = 16;
			string uiTextFontName = "AveriaSansLibre-Bold";

			GameObject root = new GameObject("Notification");
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

			CreateStyledBorder("Border", root, style.BorderType, style.BorderCapAsset, style.BorderColor, style.BorderCapSize, style.BorderOuterColor, style.BorderInnerColor, style.BorderOuterWidth, style.BorderInnerWidth, log);

			Image iconImage = null;

			if (icon != null)
			{
				iconImage = CreateUIImageObject("Icon", root, Vector2.zero, new Vector2(IconSize, IconSize));
				iconImage.sprite = icon;
				iconImage.preserveAspect = true;
				iconImage.raycastTarget = false;

				RectTransform iconRect = iconImage.rectTransform;
				SetRectAnchors(iconRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f));
				iconRect.anchoredPosition = new Vector2(19f, 0f);
			}

			Text messageText = CreateTextObject("Message", root, Color.white, uiTextFontName, uiTextFontSize, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
			messageText.text = message;
			messageText.raycastTarget = false;

			RectTransform messageRect = messageText.rectTransform;
			SetRectAnchors(messageRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f));
			messageRect.offsetMin = new Vector2(icon != null ? 38f : 10f, 0f);
			messageRect.offsetMax = new Vector2(amount != 0 ? -50f : -10f, 0f);

			Text amountText = null;

			if (amount != 0)
			{
				amountText = CreateTextObject("Amount", root, Color.white, uiTextFontName, uiTextFontSize, TextAnchor.MiddleRight, Vector2.zero, Vector2.zero);
				amountText.text = amount > 0 ? $"+{amount}" : amount.ToString();
				amountText.raycastTarget = false;

				RectTransform amountRect = amountText.rectTransform;
				SetRectAnchors(amountRect, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f));
				amountRect.anchoredPosition = new Vector2(-8f, 0f);
				amountRect.sizeDelta = new Vector2(44f, 0f);
			}

			NotificationEntry notification = new NotificationEntry
			{
				Root = root,
				Rect = rect,
				Layout = layout,
				CanvasGroup = canvasGroup,
				Icon = iconImage,
				MessageText = messageText,
				AmountText = amountText,
				Type = type,
				Key = key,
				Amount = amount,
				Lifetime = ConfigManager.EffectiveNotificationDuration,
				Progress = 1f,
				Leaving = false
			};

			notifications.Add(notification);

			TrimNotifications();

			log.Info($"Added {type} notification: '{message}' x{amount}.");
		}

		private static void AddGenericNotification(string message, Sprite icon = null, int amount = 0, bool localize = true, NotificationType type = NotificationType.Info, string key = null)
		{
			if (notificationRoot == null)
			{
				log.Warn("Could not show notification because the notification stack has not been created.");
				return;
			}

			string displayMessage = localize ? Localization.instance.Localize(message) : message;

			AddNotification(type, key, displayMessage, icon, amount);
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