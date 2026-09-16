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

		internal static GameObject CreateThreePartBackground(string objectName, GameObject parent, string spriteName, Color color, float sourceEndWidth, float renderedEndWidth, LogManager specificLog)
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
			background.transform.SetSiblingIndex(1);

			RectTransform backgroundRect = background.AddComponent<RectTransform>();
			backgroundRect.anchorMin = Vector2.zero;
			backgroundRect.anchorMax = Vector2.one;
			backgroundRect.offsetMin = Vector2.zero;
			backgroundRect.offsetMax = Vector2.zero;
			backgroundRect.localScale = Vector3.one;

			LayoutElement backgroundLayout = background.AddComponent<LayoutElement>();
			backgroundLayout.ignoreLayout = true;

			CreateThreePartBackgroundPart("Left", background, leftSprite, color, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(renderedEndWidth, 0f));
			CreateThreePartBackgroundPart("Center", background, centerSprite, color, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, renderedEndWidth);
			CreateThreePartBackgroundPart("Right", background, rightSprite, color, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(renderedEndWidth, 0f));

			return background;
		}

		private static void CreateThreePartBackgroundPart(string objectName, GameObject parent, Sprite sprite, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, float horizontalInset = 0f)
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
			image.color = color;
			image.raycastTarget = false;
		}

		private static GameObject CreateSideCapsBorder(string objectName, GameObject parent, string spriteName, Color color, Vector2 capSize, LogManager specificLog)
		{
			Sprite sprite = IconManager.LoadHudIcon(spriteName);
			float horizontalInset = 3f;

			if (sprite == null)
			{
				specificLog.Warn($"Could not load HUD border cap '{spriteName}'.");
				return null;
			}

			GameObject border = new GameObject(objectName);
			border.layer = 5;
			border.transform.SetParent(parent.transform, false);
			border.transform.SetSiblingIndex(1);

			RectTransform borderRect = border.AddComponent<RectTransform>();
			borderRect.anchorMin = Vector2.zero;
			borderRect.anchorMax = Vector2.one;
			borderRect.offsetMin = Vector2.zero;
			borderRect.offsetMax = Vector2.zero;
			borderRect.localScale = Vector3.one;

			LayoutElement borderLayout = border.AddComponent<LayoutElement>();
			borderLayout.ignoreLayout = true;

			CreateSideCap("Left", border, sprite, color, capSize, false, horizontalInset);
			CreateSideCap("Right", border, sprite, color, capSize, true, horizontalInset);

			return border;
		}

		private static void CreateSideCap(string objectName, GameObject parent, Sprite sprite, Color color, Vector2 capSize, bool mirror, float horizontalInset)
		{
			GameObject cap = new GameObject(objectName);
			cap.layer = 5;
			cap.transform.SetParent(parent.transform, false);

			RectTransform rect = cap.AddComponent<RectTransform>();

			if (mirror)
			{
				rect.anchorMin = new Vector2(1f, 0.5f);
				rect.anchorMax = new Vector2(1f, 0.5f);
				rect.pivot = new Vector2(0.5f, 0.5f);
				rect.anchoredPosition = new Vector2(-horizontalInset, 0f);
				rect.localScale = new Vector3(-1f, 1f, 1f);
			}
			else
			{
				rect.anchorMin = new Vector2(0f, 0.5f);
				rect.anchorMax = new Vector2(0f, 0.5f);
				rect.pivot = new Vector2(0.5f, 0.5f);
				rect.anchoredPosition = new Vector2(horizontalInset, 0f);
				rect.localScale = Vector3.one;
			}

			rect.sizeDelta = capSize;

			Image image = cap.AddComponent<Image>();
			image.sprite = sprite;
			image.type = Image.Type.Simple;
			image.color = color;
			image.preserveAspect = true;
			image.raycastTarget = false;
		}

		internal static GameObject ReplaceStyledBackground(GameObject currentBackground, string objectName, GameObject parent, InfoRailBackgroundType backgroundType, Color backgroundColor, string vanillaSpriteName, LogManager specificLog)
		{
			if (currentBackground != null)
			{
				UnityEngine.Object.Destroy(currentBackground);
			}

			return CreateStyledBackground(objectName, parent, backgroundType, backgroundColor, vanillaSpriteName, specificLog);
		}

		internal static GameObject ReplaceStyledBorder(GameObject currentBorder, string objectName, GameObject parent, InfoRailBorderType borderType, string spriteName, string capSpriteName, Color borderColor, Vector2 capSize, float sourceEndWidth, float renderedEndWidth, Color borderOuterColor, Color borderInnerColor, float borderOuterWidth, float borderInnerWidth, LogManager specificLog)
		{
			if (currentBorder != null)
			{
				UnityEngine.Object.Destroy(currentBorder);
			}

			return CreateStyledBorder(objectName, parent, borderType, spriteName, capSpriteName, borderColor, capSize, sourceEndWidth, renderedEndWidth, borderOuterColor, borderInnerColor, borderOuterWidth, borderInnerWidth, specificLog);
		}

		internal static GameObject CreateStyledBackground(string objectName, GameObject parent, InfoRailBackgroundType backgroundType, Color backgroundColor, string vanillaSpriteName, LogManager specificLog)
		{
			switch (backgroundType)
			{
				case InfoRailBackgroundType.None:
					return null;

				case InfoRailBackgroundType.UnityImage:
					return CreateUnityBackground(objectName, parent, backgroundColor);

				case InfoRailBackgroundType.VanillaSlicedSprite:
					return CreateVanillaSlicedBackground(objectName, parent, vanillaSpriteName, backgroundColor, specificLog);

				default:
					specificLog.Warn($"Unsupported information rail background type '{backgroundType}'.");
					return null;
			}
		}

		private static GameObject CreateUnityBackground(string objectName, GameObject parent, Color color)
		{
			GameObject background = CreateBackgroundObject(objectName, parent, 0);

			Image image = background.AddComponent<Image>();
			image.color = color;
			image.raycastTarget = false;

			return background;
		}

		private static GameObject CreateVanillaSlicedBackground(string objectName, GameObject parent, string spriteName, Color color, LogManager specificLog)
		{
			GameObject background = CreateBackgroundObject(objectName, parent, 0);

			Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(foundSprite => foundSprite.name == spriteName);

			if (sprite == null)
			{
				specificLog.Warn($"Could not find vanilla UI sprite '{spriteName}'.");
			}

			Image image = background.AddComponent<Image>();
			image.sprite = sprite;
			image.type = Image.Type.Sliced;
			image.color = color;
			image.raycastTarget = false;

			return background;
		}

		internal static GameObject CreateStyledBorder(string objectName, GameObject parent, InfoRailBorderType borderType, string spriteName, string capSpriteName, Color borderColor, Vector2 capSize, float sourceEndWidth, float renderedEndWidth, Color borderOuterColor, Color borderInnerColor, float borderOuterWidth, float borderInnerWidth, LogManager specificLog)
		{
			switch (borderType)
			{
				case InfoRailBorderType.None:
					return null;

				case InfoRailBorderType.SideCapsSprite:
					return CreateSideCapsBorder(objectName, parent, capSpriteName, borderColor, capSize, specificLog);

				case InfoRailBorderType.ThreePartSprite:
					return CreateThreePartBackground(objectName, parent, spriteName, borderColor, sourceEndWidth, renderedEndWidth, specificLog);

				case InfoRailBorderType.UnityBorder:
					return CreateUnityBorder(objectName, parent, borderOuterColor, borderColor, borderInnerColor, borderOuterWidth, borderInnerWidth);

				default:
					specificLog.Warn($"Unsupported information rail border type '{borderType}'.");
					return null;
			}
		}

		private static GameObject CreateBackgroundObject(string objectName, GameObject parent, int siblingIndex)
		{
			GameObject background = new GameObject(objectName);
			background.layer = 5;
			background.transform.SetParent(parent.transform, false);
			background.transform.SetSiblingIndex(siblingIndex);

			RectTransform rect = background.AddComponent<RectTransform>();
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			rect.localScale = Vector3.one;

			LayoutElement layout = background.AddComponent<LayoutElement>();
			layout.ignoreLayout = true;

			return background;
		}

		private static GameObject CreateUnityBorder(string objectName, GameObject parent, Color outerColor, Color mainColor, Color innerColor, float outerWidth, float innerWidth)
		{
			GameObject border = new GameObject(objectName);
			border.layer = 5;
			border.transform.SetParent(parent.transform, false);
			border.transform.SetSiblingIndex(1);

			RectTransform borderRect = border.AddComponent<RectTransform>();
			borderRect.anchorMin = Vector2.zero;
			borderRect.anchorMax = Vector2.one;
			borderRect.offsetMin = Vector2.zero;
			borderRect.offsetMax = Vector2.zero;
			borderRect.localScale = Vector3.one;

			LayoutElement layout = border.AddComponent<LayoutElement>();
			layout.ignoreLayout = true;

			CreateUnityBorderLayer("Outer", border, outerColor, outerWidth, 0f);
			CreateUnityBorderLayer("Main", border, mainColor, 1f, 1f);
			CreateUnityBorderLayer("Inner", border, innerColor, innerWidth, 2f);

			return border;
		}

		private static void CreateUnityBorderLayer(string objectName, GameObject parent, Color color, float width, float inset)
		{
			GameObject layer = new GameObject(objectName);
			layer.layer = 5;
			layer.transform.SetParent(parent.transform, false);

			RectTransform layerRect = layer.AddComponent<RectTransform>();
			layerRect.anchorMin = Vector2.zero;
			layerRect.anchorMax = Vector2.one;
			layerRect.offsetMin = Vector2.zero;
			layerRect.offsetMax = Vector2.zero;
			layerRect.localScale = Vector3.one;

			CreateUnityBorderLine("Top", layer, color, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(inset, -inset - width), new Vector2(-inset, -inset));
			CreateUnityBorderLine("Bottom", layer, color, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(inset, inset), new Vector2(-inset, inset + width));
			CreateUnityBorderLine("Left", layer, color, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(inset, inset), new Vector2(inset + width, -inset));
			CreateUnityBorderLine("Right", layer, color, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-inset - width, inset), new Vector2(-inset, -inset));
		}

		private static void CreateUnityBorderLine(string objectName, GameObject parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
		{
			GameObject line = new GameObject(objectName);
			line.layer = 5;
			line.transform.SetParent(parent.transform, false);

			RectTransform rect = line.AddComponent<RectTransform>();
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.offsetMin = offsetMin;
			rect.offsetMax = offsetMax;
			rect.localScale = Vector3.one;

			Image image = line.AddComponent<Image>();
			image.color = color;
			image.raycastTarget = false;
		}
	}
}