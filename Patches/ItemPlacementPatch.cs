using HarmonyLib;
using PerformanceTweaks.Runtime;
using UnityEngine;

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
            return !ItemPlacement.TryPlace(__instance.transform, ___item);
        }
    }
}
