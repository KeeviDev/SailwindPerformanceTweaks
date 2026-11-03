using System;
using HarmonyLib;
using PerformanceTweaks.Configuration;
using PerformanceTweaks.Runtime;

namespace PerformanceTweaks.Patches
{
    [HarmonyPatch(typeof(MasterPainter), "Awake")]
    internal static class MasterPainterAwakePatch
    {
        private static void Postfix(MasterPainter __instance)
        {
            if (TweakGuard.IsStopped(ModConfig.DirtCameraOnDemand))
                return;

            try
            {
                DirtCameraSwitch.Apply(__instance);
            }
            catch (Exception ex)
            {
                DirtCameraSwitch.Stop(ex);
            }
        }
    }

    /// <summary>
    /// Switches the dirt camera on when a full hull clean starts.
    /// </summary>
    [HarmonyPatch(typeof(MasterPainter), "DoApplyRenderTex")]
    internal static class MasterPainterFullCleanPatch
    {
        private static void Prefix(MasterPainter __instance)
        {
            if (TweakGuard.IsStopped(ModConfig.DirtCameraOnDemand))
                return;

            try
            {
                DirtCameraSwitch.RenderForClean(__instance);
            }
            catch (Exception ex)
            {
                DirtCameraSwitch.Stop(ex);
            }
        }
    }
}
