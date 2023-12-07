//using HarmonyLib;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Reflection.Emit;

//namespace BetterSongList.HarmonyPatches {
//	[HarmonyPatch(typeof(BeatmapLevelFilterModel), nameof(BeatmapLevelFilterModel.LevelContainsText))]
//	static class ImproveBasegameSearch {
//		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
//			if(!Config.Instance.ModBasegameSearch)
//				return instructions;

//			// This appends a space and the levelAuthorName to the levelString variable.
//			var matcher = new CodeMatcher(instructions)
//				.MatchForward(true,
//					new CodeMatch(OpCodes.Ldloc_S, null, "L_previewBeatmapLevel"),
//					new CodeMatch(),
//					new CodeMatch(OpCodes.Stelem_Ref),
//					new CodeMatch(x => x.opcode == OpCodes.Call && (x.operand as MethodInfo)?.Name == nameof(string.Concat), "Call_Concat"),
//					new CodeMatch(OpCodes.Stloc_S, null, "L_levelStringSt")
//				);

//			matcher.Advance(1).Insert(
//				new CodeInstruction(OpCodes.Ldc_I4_3),
//				new CodeInstruction(OpCodes.Newarr, typeof(string)),

//				new CodeInstruction(OpCodes.Dup),
//				new CodeInstruction(OpCodes.Ldc_I4_0),
//				new CodeInstruction(OpCodes.Ldloc_S, matcher.NamedMatch("L_levelStringSt").operand),
//				new CodeInstruction(OpCodes.Stelem_Ref),

//				new CodeInstruction(OpCodes.Dup),
//				new CodeInstruction(OpCodes.Ldc_I4_1),
//				new CodeInstruction(OpCodes.Ldstr, " "),
//				new CodeInstruction(OpCodes.Stelem_Ref),

//				new CodeInstruction(OpCodes.Dup),
//				new CodeInstruction(OpCodes.Ldc_I4_2),
//				matcher.NamedMatch("L_previewBeatmapLevel"),
//				new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(IPreviewBeatmapLevel), nameof(IPreviewBeatmapLevel.levelAuthorName))),
//				new CodeInstruction(OpCodes.Stelem_Ref),

//				matcher.NamedMatch("Call_Concat"),
//				matcher.NamedMatch("L_levelStringSt")
//			);

//			return matcher.InstructionEnumeration();
//		}
//	}
//}

using System;
using System.Collections.Generic;
using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(LevelFilter), nameof(LevelFilter.FilterLevelByText))]
	internal static class ImproveBasegameSearch {
		private static bool Prefix(List<IPreviewBeatmapLevel> levels, string[] searchTerms, ref List<IPreviewBeatmapLevel> __result) {
			static int CalculateMatchScore(string levelString, string[] searchTerms) {
				var num = 0;
				foreach(var text in searchTerms) {
					var num2 = levelString.IndexOf(text, StringComparison.CurrentCultureIgnoreCase);
					if(num2 >= 0) {
						var num3 = ((num2 == 0 || char.IsWhiteSpace(levelString[num2 - 1])) ? 1 : 0) + (50 * text.Length);
						num += num3;
					}
				}
				return num;
			}

			var list = new List<(int score, IPreviewBeatmapLevel level)>(levels.Count);
			foreach(var previewBeatmapLevel in levels) {
				var num = CalculateMatchScore(string.Concat(new string[]
				{
				previewBeatmapLevel.songName,
				" ",
				previewBeatmapLevel.songSubName,
				" ",
				previewBeatmapLevel.songAuthorName,
				" ",
				previewBeatmapLevel.levelAuthorName
				}), searchTerms);
				if(num > 50) {
					list.Add(new ValueTuple<int, IPreviewBeatmapLevel>(num, previewBeatmapLevel));
				}
			}
			list.Sort((x, y) => y.score.CompareTo(x.score));
			levels.Clear();
			for(var i = 0; i < list.Count; i++) {
				levels.Add(list[i].level);
			}

			__result = levels;

			return false;
		}
	}
}
