using System;
using HarmonyLib;
using PerformanceTweaks.Configuration;
using PerformanceTweaks.Runtime;

namespace PerformanceTweaks.Patches
{
    /// <summary>
    /// Hands the per-step placement of an item aboard its boat to <see cref="ItemPlacement"/> and skips the game's own
    /// placement when it took over.
    /// </summary>
    [HarmonyPatch(typeof(ItemRigidbody), "MoveItemToWalkColRigidbody")]
    internal static class ItemPlacementPatch
    {
        private static bool Prefix(ItemRigidbody __instance, ShipItem ___item)
        {
            if (TweakGuard.IsStopped(ModConfig.SkipRedundantItemUpdates))
                return true;

            try
            {
                return !ItemPlacement.TryPlace(__instance.transform, ___item);
            }
            catch (Exception ex)
            {
                TweakGuard.Stop(ModConfig.SkipRedundantItemUpdates, ex);
                return true;
            }
        }
    }
}
