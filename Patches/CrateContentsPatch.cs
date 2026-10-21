using HarmonyLib;
using PerformanceTweaks.Runtime;

namespace PerformanceTweaks.Patches
{
    /// <summary>
    /// Skips a crate's per-frame placement of its contents when <see cref="UnmovedCrateContents"/> finds it would
    /// change nothing.
    /// </summary>
    [HarmonyPatch(typeof(CrateInventory), nameof(CrateInventory.LateUpdate))]
    internal static class CrateContentsPatch
    {
        private static bool Prefix(CrateInventory __instance)
        {
            return !UnmovedCrateContents.CanSkip(__instance);
        }
    }
}
