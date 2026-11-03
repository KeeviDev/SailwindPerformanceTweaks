using System;
using HarmonyLib;
using PerformanceTweaks.Configuration;
using PerformanceTweaks.Runtime;
using UnityEngine;

namespace PerformanceTweaks.Patches
{
    /// <summary>
    /// Hands a rope's per-frame update to <see cref="RopeLod"/> and skips the game's own update when it took over.
    /// </summary>
    [HarmonyPatch(typeof(RopeEffect), "LateUpdate")]
    internal static class RopeEffectUpdatePatch
    {
        private static bool Prefix(
            RopeEffect __instance,
            LineRenderer ___lineRenderer,
            ClothRope ___clothRope,
            ref float ___distanceCheckTimer,
            ref bool ___outOfRange)
        {
            if (RopeLodGuard.IsStopped)
                return true;

            try
            {
                return !RopeLod.TryUpdate(__instance, ___lineRenderer, ___clothRope, ref ___distanceCheckTimer, ref ___outOfRange);
            }
            catch (Exception ex)
            {
                RopeLodGuard.Stop(ex);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(RopeEffect), "Start")]
    internal static class RopeEffectStartPatch
    {
        private static void Postfix(RopeEffect __instance)
        {
            if (TweakGuard.IsStopped(ModConfig.PauseHiddenRopes))
                return;

            try
            {
                HiddenRopePauser.Register(__instance);
            }
            catch (Exception ex)
            {
                TweakGuard.Stop(ModConfig.PauseHiddenRopes, ex, HiddenRopePauser.ResumeAll);
            }
        }
    }

    [HarmonyPatch(typeof(RopeEffect), nameof(RopeEffect.OnDestroy))]
    internal static class RopeEffectDestroyPatch
    {
        private static void Postfix(RopeEffect __instance)
        {
            if (RopeLodGuard.IsStopped)
                return;

            try
            {
                RopeLod.Forget(__instance);
            }
            catch (Exception ex)
            {
                RopeLodGuard.Stop(ex);
            }
        }
    }
}
