using HarmonyLib;
using MarsarahUI.Managers;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahUI.Patches.UI
{
	internal class UIController
	{
		private static readonly LogManager log = new LogManager("UI Controller", LogManager.LogLevel.Warning);

		internal static bool ShowUI = true;
		internal static bool ShowPlayerList = true;

		internal static readonly Color InfoValueColor = new Color(0.88f, 0.87f, 0.82f);

		// Needed since the Unity 6 update because LiberationSans is no longer available as the default TMP font.
		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class HudAwakePatch
		{
			private static void Prefix()
			{
				TMP_FontAsset notoEmoji = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(font => font.name == "NotoEmoji-Regular SDF");

				if (notoEmoji != null)
				{
					TMP_Settings.defaultFontAsset = notoEmoji;
					log.Info("Set default TMP font asset to NotoEmoji-Regular SDF.");
				}
				else
				{
					log.Warn("NotoEmoji font not found.");
				}
			}
		}

		public static void UpdateUIDisplay()
		{
			if (Input.GetKeyDown(KeyCode.Insert))
			{
				ShowUI = !ShowUI;
			}

			if (Input.GetKeyDown(KeyCode.Home))
			{
				ShowPlayerList = !ShowPlayerList;
			}
		}

		public static Text CreateTextObject(string name, GameObject parent, Color textColor, string fontName, int fontSize, TextAnchor alignment, Vector2 position, Vector2 sizeDelta)
		{
			GameObject textObject = new GameObject(name);
			textObject.layer = 5;
			textObject.transform.SetParent(parent.transform, false);

			RectTransform textTransform = textObject.AddComponent<RectTransform>();
			textTransform.anchoredPosition = position;
			textTransform.sizeDelta = sizeDelta;
			textTransform.localScale = Vector3.one;

			Text text = textObject.AddComponent<Text>();
			text.color = textColor;
			text.font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(font => font.name == fontName);
			text.fontSize = fontSize;
			text.alignment = alignment;

			Outline outline = textObject.AddComponent<Outline>();
			outline.effectColor = Color.black;
			outline.effectDistance = new Vector2(1f, -1f);
			outline.useGraphicAlpha = true;
			outline.useGUILayout = true;

			return text;
		}

		public static TextMeshProUGUI CreateTMPTextObject(string name, GameObject parent, Color textColor, string fontName, int fontSize, TextAlignmentOptions alignment, Vector2 position, Vector2 sizeDelta, LogManager specificLog)
		{
			GameObject textObject = new GameObject(name);
			textObject.layer = 5;
			textObject.transform.SetParent(parent.transform, false);

			RectTransform rectTransform = textObject.AddComponent<RectTransform>();
			rectTransform.anchoredPosition = position;
			rectTransform.sizeDelta = sizeDelta;
			rectTransform.localScale = Vector3.one;

			TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
			tmpText.color = textColor;
			tmpText.font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(font => font.name == fontName);
			tmpText.fontSize = fontSize;
			tmpText.alignment = alignment;
			tmpText.text = "";

			if (tmpText.font != null)
			{
				tmpText.fontMaterial = tmpText.fontMaterial != null ? new Material(tmpText.fontMaterial) : tmpText.font.material;

				if (tmpText.fontMaterial.HasProperty(ShaderUtilities.ID_OutlineWidth) && tmpText.fontMaterial.HasProperty(ShaderUtilities.ID_OutlineColor))
				{
					tmpText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.125f);
					tmpText.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
				}

				tmpText.havePropertiesChanged = true;
				tmpText.SetAllDirty();
				tmpText.ForceMeshUpdate();
			}
			else
			{
				specificLog.Warn($"Font material for '{fontName}' is null or font is missing. Skipping outline setup.");
			}

			return tmpText;
		}

		public static Image CreateUIImageObject(string name, GameObject parent, Vector2 position, Vector2 sizeDelta)
		{
			GameObject iconObject = new GameObject(name);
			iconObject.layer = 5;
			iconObject.transform.SetParent(parent.transform, false);

			RectTransform rectTransform = iconObject.AddComponent<RectTransform>();
			rectTransform.anchoredPosition = position;
			rectTransform.sizeDelta = sizeDelta;
			rectTransform.localScale = Vector3.one;

			Image image = iconObject.AddComponent<Image>();
			image.color = Color.white;

			return image;
		}

		internal static GameObject CreateThreePartBackground(string objectName, GameObject parent, string spriteName, float sourceEndWidth, float renderedEndWidth, LogManager specificLog)
		{
			Sprite source = IconManager.LoadHudIcon(spriteName);

			if (source == null)
			{
				specificLog.Warn($"Could not load HUD background '{spriteName}'.");
				return null;
			}

			Texture2D texture = source.texture;
			float textureWidth = texture.width;
			float textureHeight = texture.height;

			if (sourceEndWidth <= 0f || textureWidth <= sourceEndWidth * 2f)
			{
				specificLog.Warn($"Invalid end width {sourceEndWidth} for HUD background '{spriteName}' ({textureWidth}x{textureHeight}).");
				return null;
			}

			Rect leftRect = new Rect(0f, 0f, sourceEndWidth, textureHeight);
			Rect centerRect = new Rect(sourceEndWidth, 0f, textureWidth - sourceEndWidth * 2f, textureHeight);
			Rect rightRect = new Rect(textureWidth - sourceEndWidth, 0f, sourceEndWidth, textureHeight);

			Sprite leftSprite = IconManager.CreateSpriteSection(source, leftRect);
			Sprite centerSprite = IconManager.CreateSpriteSection(source, centerRect);
			Sprite rightSprite = IconManager.CreateSpriteSection(source, rightRect);

			GameObject background = new GameObject(objectName);
			background.layer = 5;
			background.transform.SetParent(parent.transform, false);
			background.transform.SetAsFirstSibling();

			RectTransform backgroundRect = background.AddComponent<RectTransform>();
			backgroundRect.anchorMin = Vector2.zero;
			backgroundRect.anchorMax = Vector2.one;
			backgroundRect.offsetMin = Vector2.zero;
			backgroundRect.offsetMax = Vector2.zero;
			backgroundRect.localScale = Vector3.one;

			LayoutElement backgroundLayout = background.AddComponent<LayoutElement>();
			backgroundLayout.ignoreLayout = true;

			CreateThreePartBackgroundPart("Left", background, leftSprite, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(renderedEndWidth, 0f));
			CreateThreePartBackgroundPart("Center", background, centerSprite, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, renderedEndWidth);
			CreateThreePartBackgroundPart("Right", background, rightSprite, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(renderedEndWidth, 0f));

			return background;
		}

		private static void CreateThreePartBackgroundPart(string objectName, GameObject parent, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, float horizontalInset = 0f)
		{
			GameObject part = new GameObject(objectName);
			part.layer = 5;
			part.transform.SetParent(parent.transform, false);

			RectTransform rect = part.AddComponent<RectTransform>();
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = pivot;
			rect.localScale = Vector3.one;

			if (anchorMin.x != anchorMax.x)
			{
				rect.offsetMin = new Vector2(horizontalInset, 0f);
				rect.offsetMax = new Vector2(-horizontalInset, 0f);
			}
			else
			{
				rect.anchoredPosition = Vector2.zero;
				rect.sizeDelta = sizeDelta;
			}

			Image image = part.AddComponent<Image>();
			image.sprite = sprite;
			image.type = Image.Type.Simple;
			image.color = Color.white;
			image.raycastTarget = false;
		}
	}
}