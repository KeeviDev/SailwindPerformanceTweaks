using System.Collections.Generic;
using cakeslice;
using HarmonyLib;
using PerformanceTweaks.Configuration;

namespace PerformanceTweaks.Patches
{
    /// <summary>
    /// Skips rendering an outline camera while nothing is outlined and <see cref="ModConfig.OutlineCamerasOnDemand"/>
    /// is on, except for the first idle frame.
    /// </summary>
    [HarmonyPatch(typeof(OutlineEffect), nameof(OutlineEffect.OnPreRender))]
    internal static class OutlineEffectPatch
    {
        private static readonly AccessTools.FieldRef<LinkedSet<Outline>, LinkedList<Outline>> OutlineList =
            AccessTools.FieldRefAccess<LinkedSet<Outline>, LinkedList<Outline>>("list");

        private static readonly HashSet<OutlineEffect> clearedWhileIdle = new HashSet<OutlineEffect>();

        /// <summary>
        /// Makes every outline camera render its next idle frame again.
        /// </summary>
        public static void Forget()
        {
            clearedWhileIdle.Clear();
        }

        private static bool Prefix(OutlineEffect __instance, LinkedSet<Outline> ___outlines)
        {
            if (!ModConfig.OutlineCamerasOnDemand.Value || ___outlines == null || OutlineList(___outlines).Count > 0)
            {
                clearedWhileIdle.Remove(__instance);
                return true;
            }

            return clearedWhileIdle.Add(__instance);
        }
    }
}
