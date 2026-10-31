using HarmonyLib;
using MarsarahUI.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using MarsarahUI.Patches.Logic;

namespace MarsarahUI.Patches.UI
{
	internal class UIPortalTransition : UIController
	{
		private static readonly LogManager log = new LogManager("UI Portal Transition", LogManager.LogLevel.Warning);

		private static Hud currentHud;
		private static CanvasGroup vanillaTeleportCanvasGroup;
		private static float vanillaTeleportOriginalAlpha = 1f;
		private static bool vanillaTeleportVisualsHidden;

		private static bool hasPendingDestination;
		private static bool portalSessionActive;

		private static string destinationBiome = string.Empty;
		private static string destinationAltBiomes = "None";
		private static Sprite destinationBiomeSprite;

		private static GameObject portalOverlayRoot;
		private static Image screenDimmer;
		private static Image portalBackplate;
		private static Image portalOuterRing;
		private static Image portalInnerImage;
		private static Text portalBiomeText;
		private static Text portalAltBiomeText;

		private static Sprite circleSprite;
		private static Sprite ringSprite;

		private const string PortalRingResource1 = "MarsarahUI.Assets.Portals.Rings.PortalRing_01.png";
		private const string PortalBackgroundResource = "MarsarahUI.Assets.Portals.Backgrounds.PortalBackground_01.png";

		private static Sprite portalRingSprite;
		private static Sprite portalBackgroundSprite;
		private static Image portalBackgroundImage;


		[HarmonyPatch(typeof(Player), "TeleportTo")]
		private static class PortalTransition_TeleportToPatch
		{
			private static void Postfix(Player __instance, Vector3 pos, bool distantTeleport, bool __result)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null || __instance != Player.m_localPlayer) return;
				if (!__result) return;
				if (!ConfigManager.EffectivePortalTransitionEnabled) return;

				try
				{
					WorldGenerator generator = WorldGenerator.instance;

					destinationBiome = generator != null ? generator.GetBiome(pos).ToString() : "Unknown";
					destinationAltBiomes = GetAltBiomesString(generator, pos);

					hasPendingDestination = true;

					log.Info(
						$"Portal destination cached | " +
						$"Biome: {destinationBiome} | " +
						$"Alt Biome: {destinationAltBiomes} | " +
						$"Position: {pos} | " +
						$"Distant: {distantTeleport}"
					);
				}
				catch (System.Exception ex)
				{
					log.Error($"Failed to cache portal destination: {ex}");
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		private static class PortalTransition_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;
				if (!ConfigManager.EffectivePortalTransitionEnabled) return;

				UpdateVanillaTeleportVisuals(__instance);
				CreateUI(__instance);
				HidePortalOverlay();
			}
		}

		[HarmonyPatch(typeof(Hud), "UpdateBlackScreen")]
		private static class PortalTransition_HUDPatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				UpdateVanillaTeleportVisuals(__instance);

				if (!ConfigManager.EffectivePortalTransitionEnabled)
				{
					DisablePortalTransition();
					return;
				}

				UpdatePortalOverlay(__instance);
			}
		}

		private static void UpdatePortalOverlay(Hud hud)
		{
			CreateUI(hud);

			bool teleportProgressVisible = hud.m_teleportingProgress != null && hud.m_teleportingProgress.activeInHierarchy;

			float blackScreenAlpha = hud.m_loadingScreen != null ? hud.m_loadingScreen.alpha : 0f;

			bool blackScreenVisible =
				hud.m_loadingScreen != null &&
				hud.m_loadingScreen.gameObject.activeInHierarchy &&
				blackScreenAlpha > 0.01f;

			if (!portalSessionActive && hasPendingDestination && teleportProgressVisible)
			{
				portalSessionActive = true;

				destinationBiomeSprite = PortalImageResolver.ResolveSprite(destinationBiome, destinationAltBiomes);

				log.Info($"Portal session started | Biome: {destinationBiome} | Alt Biome: {destinationAltBiomes}");
			}

			bool shouldShowOverlay = portalSessionActive && (teleportProgressVisible || blackScreenVisible);

			if (!shouldShowOverlay)
			{
				if (portalSessionActive && !teleportProgressVisible && !blackScreenVisible)
				{
					portalSessionActive = false;
					hasPendingDestination = false;
					destinationBiomeSprite = null;

					log.Info("Portal session ended.");
				}

				HidePortalOverlay();
				return;
			}

			ShowPortalOverlay(blackScreenAlpha);
		}

		private static void CreateUI(Hud hud)
		{
			if (portalOverlayRoot != null && currentHud == hud) return;
			if (hud == null || hud.m_loadingScreen == null) return;

			currentHud = hud;

			GameObject parentObject = hud.m_loadingScreen.gameObject;

			portalOverlayRoot = new GameObject("PortalTransitionOverlay");
			portalOverlayRoot.layer = 5;
			portalOverlayRoot.transform.SetParent(parentObject.transform, false);

			RectTransform rootRect = portalOverlayRoot.AddComponent<RectTransform>();
			rootRect.anchorMin = Vector2.zero;
			rootRect.anchorMax = Vector2.one;
			rootRect.offsetMin = Vector2.zero;
			rootRect.offsetMax = Vector2.zero;
			rootRect.localScale = Vector3.one;

			if (portalBackgroundSprite == null)
			{
				portalBackgroundSprite = IconManager.LoadEmbeddedIcon(PortalBackgroundResource);

				if (portalBackgroundSprite != null)
				{
					log.Info("Loaded portal background image: PortalBackground_01.png");
				}
				else
				{
					log.Warn("Could not load PortalBackground_01.png. Using black fallback.");
				}
			}

			portalBackgroundImage = CreateFullStretchImage("PortalBackground", portalOverlayRoot, portalBackgroundSprite, portalBackgroundSprite != null ? Color.white : Color.black);

			screenDimmer = CreateFullStretchImage("PortalDimmer", portalOverlayRoot, null, portalBackgroundSprite != null ? new Color(0f, 0f, 0f, 0.25f) : new Color(0f, 0f, 0f, 0.92f));

			GameObject portalContainer = new GameObject("PortalContainer");
			portalContainer.layer = 5;
			portalContainer.transform.SetParent(portalOverlayRoot.transform, false);

			RectTransform containerRect = portalContainer.AddComponent<RectTransform>();
			containerRect.anchorMin = new Vector2(0.5f, 0.5f);
			containerRect.anchorMax = new Vector2(0.5f, 0.5f);
			containerRect.pivot = new Vector2(0.5f, 0.5f);
			containerRect.anchoredPosition = new Vector2(0f, 0f);
			containerRect.sizeDelta = new Vector2(700f, 700f);
			containerRect.localScale = Vector3.one * 1.7f;

			portalBackplate = CreateCenteredImage("PortalBackplate", portalContainer, GetCircleSprite(), new Vector2(0f, 0f), new Vector2(520f, 520f), new Color(0.04f, 0.04f, 0.04f, 0.95f));

			GameObject maskObject = new GameObject("PortalInnerMask");
			maskObject.layer = 5;
			maskObject.transform.SetParent(portalContainer.transform, false);

			RectTransform maskRect = maskObject.AddComponent<RectTransform>();
			maskRect.anchorMin = new Vector2(0.5f, 0.5f);
			maskRect.anchorMax = new Vector2(0.5f, 0.5f);
			maskRect.pivot = new Vector2(0.5f, 0.5f);
			maskRect.anchoredPosition = Vector2.zero;
			maskRect.sizeDelta = new Vector2(360f, 360f);
			maskRect.localScale = Vector3.one;

			Image maskImage = maskObject.AddComponent<Image>();
			maskImage.sprite = GetCircleSprite();
			maskImage.color = Color.white;
			maskImage.raycastTarget = false;

			Mask mask = maskObject.AddComponent<Mask>();
			mask.showMaskGraphic = false;

			portalInnerImage = CreateCenteredImage("PortalInnerImage", maskObject, GetCircleSprite(), Vector2.zero, new Vector2(390f, 390f), Color.white);

			if (portalRingSprite == null)
			{
				portalRingSprite = IconManager.LoadEmbeddedIcon(PortalRingResource1);

				if (portalRingSprite != null)
				{
					log.Info("Loaded portal ring image: PortalRing_01.png");
				}
				else
				{
					log.Warn("Could not load PortalRing_01.png. Using procedural ring.");
				}
			}

			portalOuterRing = CreateCenteredImage("PortalOuterRing", portalContainer, portalRingSprite != null ? portalRingSprite : GetRingSprite(), Vector2.zero, new Vector2(630f, 630f), Color.white);

			GameObject textArea = new GameObject("PortalTextArea");
			textArea.layer = 5;
			textArea.transform.SetParent(portalOverlayRoot.transform, false);

			RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
			textAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
			textAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
			textAreaRect.pivot = new Vector2(0.5f, 0.5f);
			textAreaRect.anchoredPosition = new Vector2(0f, -500f);
			textAreaRect.sizeDelta = new Vector2(700f, 90f);
			textAreaRect.localScale = Vector3.one;

			portalBiomeText = CreateTextObject("PortalBiomeText", textArea, Color.white, "AveriaSansLibre-Bold", 26, TextAnchor.MiddleCenter, new Vector2(0f, 18f), new Vector2(700f, 40f));
			portalAltBiomeText = CreateTextObject("PortalAltBiomeText", textArea, new Color(0.8f, 0.9f, 1f, 1f), "AveriaSansLibre-Bold", 18, TextAnchor.MiddleCenter, new Vector2(0f, -16f), new Vector2(700f, 30f));

			portalOverlayRoot.SetActive(false);

			log.Info("Portal transition UI skeleton created.");
		}

		private static void ShowPortalOverlay(float blackScreenAlpha)
		{
			if (portalOverlayRoot == null) return;

			if (!portalOverlayRoot.activeSelf)
			{
				portalOverlayRoot.SetActive(true);
			}

			float overlayAlpha = Mathf.Clamp01(Mathf.Max(blackScreenAlpha, 0.85f));

			if (screenDimmer != null)
			{
				screenDimmer.color = portalBackgroundSprite != null
					? new Color(0f, 0f, 0f, 0.25f)
					: new Color(0f, 0f, 0f, 0.92f);
			}

			if (portalBackgroundImage != null)
			{
				portalBackgroundImage.sprite = portalBackgroundSprite;
				portalBackgroundImage.color = portalBackgroundSprite != null ? Color.white : Color.black;
			}

			if (portalBackplate != null)
			{
				portalBackplate.color = new Color(0.04f, 0.04f, 0.04f, 0.92f);
			}

			if (portalInnerImage != null)
			{
				bool hasBiomeImage = destinationBiomeSprite != null;

				portalInnerImage.sprite = hasBiomeImage ? destinationBiomeSprite : GetCircleSprite();
				portalInnerImage.color = hasBiomeImage ? Color.white : GetBiomePlaceholderColor(destinationBiome);
			}

			if (portalOuterRing != null)
			{
				if (portalRingSprite != null)
				{
					portalOuterRing.sprite = portalRingSprite;
					portalOuterRing.color = Color.white;
				}
				else
				{
					portalOuterRing.sprite = GetRingSprite();
					portalOuterRing.color = GetRingColor(destinationBiome);
				}

				portalOuterRing.rectTransform.Rotate(0f, 0f, -50f * Time.unscaledDeltaTime);
			}

			bool showDestinationText = ConfigManager.EffectiveShowPortalDestinationText;

			if (portalBiomeText != null)
			{
				portalBiomeText.enabled = showDestinationText;

				if (showDestinationText)
				{
					portalBiomeText.text = GetBiomeDisplayName(destinationBiome);
				}
			}

			if (portalAltBiomeText != null)
			{
				portalAltBiomeText.enabled = showDestinationText;

				if (showDestinationText)
				{
					portalAltBiomeText.text = destinationAltBiomes != "None" ? GetAltBiomeDisplayName(destinationAltBiomes, destinationBiome) : string.Empty;
				}
			}
		}

		private static void HidePortalOverlay()
		{
			if (portalOverlayRoot == null) return;
			if (!portalOverlayRoot.activeSelf) return;

			portalOverlayRoot.SetActive(false);
		}

		private static void DisablePortalTransition()
		{
			HidePortalOverlay();

			hasPendingDestination = false;
			portalSessionActive = false;

			destinationBiome = string.Empty;
			destinationAltBiomes = "None";
			destinationBiomeSprite = null;
		}

		private static void UpdateVanillaTeleportVisuals(Hud hud)
		{
			if (hud == null || hud.m_teleportingProgress == null) return;

			bool hideVanillaEffect = ConfigManager.EffectivePortalTransitionEnabled && (hasPendingDestination || portalSessionActive);

			if (!hideVanillaEffect && !vanillaTeleportVisualsHidden) return;

			if (vanillaTeleportCanvasGroup == null || vanillaTeleportCanvasGroup.gameObject != hud.m_teleportingProgress)
			{
				vanillaTeleportCanvasGroup = hud.m_teleportingProgress.GetComponent<CanvasGroup>();

				if (vanillaTeleportCanvasGroup == null)
				{
					vanillaTeleportCanvasGroup = hud.m_teleportingProgress.AddComponent<CanvasGroup>();
				}

				vanillaTeleportOriginalAlpha = vanillaTeleportCanvasGroup.alpha;
				vanillaTeleportVisualsHidden = false;
			}

			if (hideVanillaEffect)
			{
				if (!vanillaTeleportVisualsHidden)
				{
					vanillaTeleportOriginalAlpha = vanillaTeleportCanvasGroup.alpha;
					vanillaTeleportVisualsHidden = true;

					log.Info("Vanilla teleport swirl hidden.");
				}

				vanillaTeleportCanvasGroup.alpha = 0f;
			}
			else if (vanillaTeleportVisualsHidden)
			{
				vanillaTeleportCanvasGroup.alpha = vanillaTeleportOriginalAlpha;
				vanillaTeleportVisualsHidden = false;

				log.Info("Vanilla teleport swirl restored.");
			}
		}

		private static string GetAltBiomesString(WorldGenerator generator, Vector3 pos)
		{
			if (generator == null) return "None";

			BiomeSector sector = generator.GetBiomeSector(pos, true);
			if (sector == null || sector.AltBiomes == null || sector.AltBiomes.Count == 0) return "None";

			List<string> altBiomeNames = new List<string>();

			foreach (AltBiome altBiome in sector.AltBiomes)
			{
				if (altBiome == null || string.IsNullOrEmpty(altBiome.m_name)) continue;

				altBiomeNames.Add(altBiome.m_name);
			}

			return altBiomeNames.Count > 0 ? string.Join(", ", altBiomeNames) : "None";
		}

		private static string GetAltBiomeDisplayName(string altBiomes, string biome)
		{
			string suffix = " " + GetBiomeDisplayName(biome);
			string[] names = altBiomes.Split(',');

			for (int i = 0; i < names.Length; i++)
			{
				string name = names[i].Trim();

				if (name.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
				{
					name = name.Substring(0, name.Length - suffix.Length);
				}

				names[i] = name;
			}

			// Sort alternate names using the shared biome priority.
			names = names.OrderBy(name => BiomePriority.GetAlternatePriority(biome, name)).ToArray();

			return "Alternate: " + string.Join(", ", names);
		}

		private static Image CreateFullStretchImage(string objectName, GameObject parent, Sprite sprite, Color color)
		{
			GameObject imageObject = new GameObject(objectName);
			imageObject.layer = 5;
			imageObject.transform.SetParent(parent.transform, false);

			RectTransform rect = imageObject.AddComponent<RectTransform>();
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			rect.localScale = Vector3.one;

			Image image = imageObject.AddComponent<Image>();
			image.sprite = sprite;
			image.color = color;
			image.raycastTarget = false;
			image.type = Image.Type.Simple;
			image.preserveAspect = false;

			return image;
		}

		private static Image CreateCenteredImage(string objectName, GameObject parent, Sprite sprite, Vector2 position, Vector2 size, Color color)
		{
			GameObject imageObject = new GameObject(objectName);
			imageObject.layer = 5;
			imageObject.transform.SetParent(parent.transform, false);

			RectTransform rect = imageObject.AddComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.5f, 0.5f);
			rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = position;
			rect.sizeDelta = size;
			rect.localScale = Vector3.one;

			Image image = imageObject.AddComponent<Image>();
			image.sprite = sprite;
			image.color = color;
			image.raycastTarget = false;
			image.type = Image.Type.Simple;
			image.preserveAspect = true;

			return image;
		}

		private static Sprite GetCircleSprite()
		{
			if (circleSprite != null) return circleSprite;

			circleSprite = CreateCircleSprite("PortalCircleSprite", 256, 0f);
			return circleSprite;
		}

		private static Sprite GetRingSprite()
		{
			if (ringSprite != null) return ringSprite;

			ringSprite = CreateCircleSprite("PortalRingSprite", 256, 0.76f);
			return ringSprite;
		}

		private static Sprite CreateCircleSprite(string textureName, int size, float innerRadiusNormalized)
		{
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
			texture.name = textureName;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.filterMode = FilterMode.Bilinear;

			float center = (size - 1) * 0.5f;
			float outerRadius = size * 0.5f;
			float innerRadius = outerRadius * innerRadiusNormalized;

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float dx = x - center;
					float dy = y - center;
					float distance = Mathf.Sqrt(dx * dx + dy * dy);

					bool insideOuter = distance <= outerRadius;
					bool outsideInner = distance >= innerRadius;

					float alpha = insideOuter && outsideInner ? 1f : 0f;

					texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
				}
			}

			texture.Apply();

			return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
		}

		private static Color GetBiomePlaceholderColor(string biome)
		{
			switch (biome)
			{
				case "Meadows":
					return new Color(0.55f, 0.82f, 0.42f, 1f);

				case "BlackForest":
					return new Color(0.12f, 0.32f, 0.20f, 1f);

				case "Swamp":
					return new Color(0.23f, 0.35f, 0.18f, 1f);

				case "Mountain":
					return new Color(0.75f, 0.80f, 0.86f, 1f);

				case "Plains":
					return new Color(0.80f, 0.70f, 0.35f, 1f);

				case "Mistlands":
					return new Color(0.52f, 0.48f, 0.63f, 1f);

				case "AshLands":
					return new Color(0.70f, 0.25f, 0.16f, 1f);

				case "DeepNorth":
					return new Color(0.75f, 0.90f, 1.00f, 1f);

				case "Ocean":
					return new Color(0.15f, 0.42f, 0.72f, 1f);

				default:
					return new Color(0.50f, 0.50f, 0.50f, 1f);
			}
		}

		private static Color GetRingColor(string biome)
		{
			switch (biome)
			{
				case "Meadows":
					return new Color(0.50f, 0.90f, 1.00f, 0.95f);

				case "BlackForest":
					return new Color(0.35f, 0.80f, 1.00f, 0.95f);

				case "Swamp":
					return new Color(0.55f, 0.90f, 0.55f, 0.95f);

				case "Mountain":
					return new Color(0.90f, 0.95f, 1.00f, 0.95f);

				case "Plains":
					return new Color(1.00f, 0.88f, 0.50f, 0.95f);

				case "Mistlands":
					return new Color(0.75f, 0.72f, 1.00f, 0.95f);

				case "AshLands":
					return new Color(1.00f, 0.45f, 0.25f, 0.95f);

				case "DeepNorth":
					return new Color(0.75f, 0.95f, 1.00f, 0.95f);

				case "Ocean":
					return new Color(0.45f, 0.78f, 1.00f, 0.95f);

				default:
					return new Color(0.85f, 0.85f, 0.85f, 0.95f);
			}
		}

		private static string GetBiomeDisplayName(string biome)
		{
			switch (biome)
			{
				case "BlackForest":
					return "Black Forest";

				case "AshLands":
					return "Ashlands";

				case "DeepNorth":
					return "Deep North";

				default:
					return biome ?? string.Empty;
			}
		}
	}
}