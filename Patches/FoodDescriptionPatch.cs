using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PerformanceTweaks.Configuration;
using UnityEngine;

namespace PerformanceTweaks.Patches
{
    /// <summary>
    /// Routes the per-frame description refreshes in <c>FoodState.Update</c> through <see cref="RefreshIfNeeded"/>.
    /// The refresh is called through its patched entry point, so every patch on it runs or is skipped together.
    /// </summary>
    [HarmonyPatch(typeof(FoodState), nameof(FoodState.Update))]
    internal static class FoodDescriptionPatch
    {
        private const int RefreshIntervalFrames = 15;

        private static readonly MethodInfo UpdateLookTextMethod = AccessTools.Method(typeof(FoodState), "UpdateLookText");
        private static readonly Action<FoodState> UpdateLookText = AccessTools.MethodDelegate<Action<FoodState>>(UpdateLookTextMethod);
        private static readonly AccessTools.FieldRef<FoodState, ShipItemFood> FoodOf = AccessTools.FieldRefAccess<FoodState, ShipItemFood>("food");

        /// <summary>
        /// Refreshes the description of <paramref name="state"/>'s food, every frame while
        /// <see cref="ModConfig.FoodDescriptionsOnDemand"/> is off and otherwise only while the food is looked
        /// at or held, or on its turn every <see cref="RefreshIntervalFrames"/> frames.
        /// </summary>
        public static void RefreshIfNeeded(FoodState state)
        {
            if (!ModConfig.FoodDescriptionsOnDemand.Value || IsShown(FoodOf(state)) || IsOnTurn(state))
                UpdateLookText(state);
        }

        private static bool IsShown(ShipItemFood food)
        {
            return food == null || food.IsLookedAt() || food.held;
        }

        private static bool IsOnTurn(FoodState state)
        {
            int phase = state.GetInstanceID() & int.MaxValue;
            return (Time.frameCount + phase) % RefreshIntervalFrames == 0;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patched = new List<CodeInstruction>(instructions);
            MethodInfo refresh = AccessTools.Method(typeof(FoodDescriptionPatch), nameof(RefreshIfNeeded));
            int routed = 0;
            foreach (CodeInstruction instruction in patched)
            {
                if (instruction.Calls(UpdateLookTextMethod))
                {
                    instruction.operand = refresh;
                    routed++;
                }
            }

            if (routed == 0)
                Plugin.Log.LogWarning("Found no description refresh to route in FoodState.Update.");
            return patched;
        }
    }
}
