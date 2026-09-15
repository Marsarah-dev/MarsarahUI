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

		public static void UpdateUIPositions()
		{
			UISkillProgress.UpdatePosition();
		}
	}
}