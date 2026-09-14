using HarmonyLib;
using MarsarahUI.Managers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace MarsarahUI.Patches.UI
{
	internal class UIGlobalChat
	{
		private static readonly LogManager log = new LogManager("UI Global Chat", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(Chat), "InputText")]
		private static class ChatInputTextPatch
		{
			private static void Prefix(Chat __instance)
			{
				if (!ConfigManager.EffectiveGlobalChatByDefault || __instance?.m_input == null) return;

				string text = __instance.m_input.text;

				if (string.IsNullOrEmpty(text) || text[0] == '/') return;

				__instance.m_input.text = $"/s {text}";
			}
		}

		[HarmonyPatch]
		private static class TerminalAddStringPatch
		{
			private static IEnumerable<MethodBase> TargetMethods()
			{
				foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(Terminal)))
				{
					if (method.Name != "AddString") continue;

					ParameterInfo[] parameters = method.GetParameters();

					if (parameters.Length != 4) continue;
					if (parameters[1].ParameterType != typeof(string)) continue;
					if (parameters[2].ParameterType != typeof(Talker.Type)) continue;
					if (parameters[3].ParameterType != typeof(bool)) continue;

					yield return method;
				}
			}

			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				MethodInfo preserveCaseMethod = AccessTools.Method(typeof(UIGlobalChat), nameof(FormatShoutText));

				foreach (CodeInstruction instruction in instructions)
				{
					if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) &&
						instruction.operand is MethodInfo method &&
						method.DeclaringType == typeof(string) &&
						method.Name == nameof(string.ToUpper) &&
						method.GetParameters().Length == 0)
					{
						instruction.opcode = OpCodes.Call;
						instruction.operand = preserveCaseMethod;
					}

					yield return instruction;
				}
			}
		}

		[HarmonyPatch(typeof(Chat), "AddInworldText")]
		private static class ChatAddInworldTextPatch
		{
			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				MethodInfo toUpperMethod = AccessTools.Method(typeof(string), nameof(string.ToUpper), Type.EmptyTypes);
				MethodInfo preserveCaseMethod = AccessTools.Method(typeof(UIGlobalChat), nameof(FormatShoutText));

				bool replaced = false;

				foreach (CodeInstruction instruction in instructions)
				{
					if (instruction.Calls(toUpperMethod))
					{
						instruction.opcode = OpCodes.Call;
						instruction.operand = preserveCaseMethod;
						replaced = true;
					}

					yield return instruction;
				}

				if (!replaced)
					log.Warn("Could not find the vanilla Shout capitalization call in Chat.AddInworldText.");
			}
		}

		private static string FormatShoutText(string text)
		{
			return ConfigManager.EffectiveGlobalChatByDefault ? text : text.ToUpper();
		}
	}
}