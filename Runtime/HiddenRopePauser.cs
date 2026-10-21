using System;
using System.Collections.Generic;
using HarmonyLib;
using PerformanceTweaks.Configuration;
using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Pauses the update of every 3D rope that no camera drew in the previous frame while
    /// <see cref="ModConfig.PauseHiddenRopes"/> is on, letting each paused rope update once every
    /// <see cref="RefreshInterval"/> frames. Pauses a rope by disabling its component.
    /// </summary>
    internal static class HiddenRopePauser
    {
        private const int RefreshInterval = 20;
        private const int PruneEveryRegistrations = 256;

        private static readonly AccessTools.FieldRef<RopeEffect, ClothRope> ClothOf =
            AccessTools.FieldRefAccess<RopeEffect, ClothRope>("clothRope");

        private static readonly AccessTools.FieldRef<RopeEffect, bool> IsOutOfRange =
            AccessTools.FieldRefAccess<RopeEffect, bool>("outOfRange");

        private static readonly Predicate<PausableRope> IsDestroyed = entry => entry.Rope == null;

        private static readonly List<PausableRope> ropes = new List<PausableRope>();
        private static bool hasPausedRopes;

        public static void Register(RopeEffect rope)
        {
            ropes.Add(new PausableRope(rope));
            if (ropes.Count % PruneEveryRegistrations == 0)
                ropes.RemoveAll(IsDestroyed);
        }

        /// <summary>
        /// Enables or disables every registered rope for this frame. Must run every frame before the rope updates.
        /// </summary>
        public static void Update()
        {
            bool isPausing = ModConfig.PauseHiddenRopes.Value;
            if (!isPausing && !hasPausedRopes)
                return;

            hasPausedRopes = false;
            for (int index = ropes.Count - 1; index >= 0; index--)
            {
                PausableRope entry = ropes[index];
                if (entry.Rope == null)
                {
                    ropes.RemoveAt(index);
                    continue;
                }

                bool shouldUpdate = !isPausing || NeedsUpdate(entry);
                if (entry.Rope.enabled != shouldUpdate)
                    entry.Rope.enabled = shouldUpdate;
                if (!shouldUpdate)
                    hasPausedRopes = true;
            }
        }

        private static bool NeedsUpdate(PausableRope entry)
        {
            RopeEffect rope = entry.Rope;
            if (rope.attachment == null || IsOutOfRange(rope) || !IsHidden3DRope(rope))
            {
                entry.HiddenFrames = 0;
                return true;
            }

            entry.HiddenFrames++;
            return entry.HiddenFrames % RefreshInterval == 0;
        }

        /// <summary>
        /// Tells whether the rope is shown as a 3D rope that no camera drew in the previous frame.
        /// </summary>
        private static bool IsHidden3DRope(RopeEffect rope)
        {
            ClothRope cloth = ClothOf(rope);
            return cloth != null && cloth.skinned != null && cloth.gameObject.activeSelf && !cloth.skinned.isVisible;
        }

        private sealed class PausableRope
        {
            public readonly RopeEffect Rope;

            public int HiddenFrames;

            public PausableRope(RopeEffect rope)
            {
                Rope = rope;
            }
        }
    }
}
