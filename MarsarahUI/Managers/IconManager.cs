using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MarsarahUI.Managers
{
	internal static class IconManager
	{
		private static readonly LogManager log = new LogManager("Icon Manager", LogManager.LogLevel.Warning);

		private static readonly Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();
		private const string HudIconResourcePrefix = "MarsarahUI.Assets.Icons.HUD.";

		public static Sprite LoadHudIcon(string iconName)
		{
			if (string.IsNullOrEmpty(iconName)) return null;

			return LoadEmbeddedIcon($"{HudIconResourcePrefix}{iconName}.png");
		}

		internal static Sprite LoadEmbeddedIcon(string resourceName)
		{
			if (string.IsNullOrEmpty(resourceName)) return null;
			if (icons.TryGetValue(resourceName, out Sprite cached)) return cached;

			Assembly assembly = Assembly.GetExecutingAssembly();
			using Stream stream = assembly.GetManifestResourceStream(resourceName);

			if (stream == null)
			{
				log.Warn($"Icon resource '{resourceName}' not found in assembly.");
				return null;
			}

			byte[] data;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				data = memoryStream.ToArray();
			}

			Texture2D texture = new Texture2D(2, 2);

			if (!TryLoadImageBytes(texture, data))
			{
				log.Error($"Failed to decode image bytes for '{resourceName}'.");
				UnityEngine.Object.Destroy(texture);
				return null;
			}

			texture.wrapMode = TextureWrapMode.Clamp;
			texture.filterMode = FilterMode.Bilinear;

			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
			icons[resourceName] = sprite;

			return sprite;
		}

		private static bool TryLoadImageBytes(Texture2D texture, byte[] data)
		{
			if (texture == null || data == null) return false;

			try
			{
				Assembly imageConversionAssembly = Assembly.Load("UnityEngine.ImageConversionModule");
				Type imageConversionType = imageConversionAssembly.GetType("UnityEngine.ImageConversion");

				if (imageConversionType == null)
				{
					log.Error("Could not find UnityEngine.ImageConversion type.");
					return false;
				}

				MethodInfo loadImage = imageConversionType.GetMethod("LoadImage", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }, null);

				if (loadImage == null)
				{
					log.Error("Could not find ImageConversion.LoadImage method.");
					return false;
				}

				object result = loadImage.Invoke(null, new object[] { texture, data, false });
				return result is bool success && success;
			}
			catch (Exception ex)
			{
				log.Error($"Failed to load image bytes: {ex}");
				return false;
			}
		}
	}
}