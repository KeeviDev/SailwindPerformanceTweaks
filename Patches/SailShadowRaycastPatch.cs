using System.Reflection;
using HarmonyLib;
using PerformanceTweaks.Configuration;
using UnityEngine;

namespace PerformanceTweaks.Patches
{
    /// <summary>
    /// Switches Unity's automatic transform sync off for each step of a sail's wind shadow check while
    /// <see cref="ModConfig.SkipSyncForSailWindShadows"/> is on, so its ray is cast against the physics positions from
    /// the last sync.
    /// </summary>
    [HarmonyPatch]
    internal static class SailShadowRaycastPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(SailShadowCol), "CheckShadow"));
        }

        private static void Prefix(out bool __state)
        {
            __state = ModConfig.SkipSyncForSailWindShadows.Value && Physics.autoSyncTransforms;
            if (__state)
                Physics.autoSyncTransforms = false;
        }

        private static void Finalizer(bool __state)
        {
            if (__state)
                Physics.autoSyncTransforms = true;
        }
    }
}
