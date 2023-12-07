using HarmonyLib;

namespace BetterSongList.HarmonyPatches {
	[HarmonyPatch(typeof(SearchFilterParamsViewController), nameof(SearchFilterParamsViewController.Refresh))]
	static class PresetFilter {
		static void Prefix(ref LevelFilter filter) {
			filter.songOwned = filter.songOwned || Config.Instance.AutoFilterUnowned;
		}
	}
}
