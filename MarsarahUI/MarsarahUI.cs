using BepInEx;
using HarmonyLib;
using MarsarahUI.Managers;
using MarsarahUI.Patches.UI;

namespace MarsarahUI
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class MarsarahUI : BaseUnityPlugin
	{
		internal const string ModName = "MarsarahUI";
		internal const string ModVersion = "1.0.0";
		internal const string Author = "Marsarah";
		public const string ModGUID = Author + "." + ModName;

		private readonly Harmony harmony = new Harmony(ModGUID);
		private static readonly LogManager log = new LogManager("Main", LogManager.LogLevel.Info);

		private void Awake()
		{
			LogManager.SetGlobalLogLevel(LogManager.LogLevel.Info);
			ConfigManager.Init(Config);

			harmony.PatchAll();

			log.Info($"{ModName} v{ModVersion} loaded.");
		}

		private void Update()
		{
			UIController.UpdateUIDisplay();
		}

		private void OnDestroy()
		{
			Config.Save();
		}
	}
}