using HarmonyLib;
using PerformanceTweaks.Runtime;

namespace PerformanceTweaks.Patches
{
    [HarmonyPatch(typeof(MasterPainter), "Awake")]
    internal static class MasterPainterAwakePatch
    {
        private static void Postfix(MasterPainter __instance)
        {
            DirtCameraSwitch.Apply(__instance);
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
            DirtCameraSwitch.RenderForClean(__instance);
        }
    }
}
