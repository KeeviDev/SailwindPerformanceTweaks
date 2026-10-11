using System;
using System.Collections.Generic;
using HarmonyLib;
using PerformanceTweaks.Configuration;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Takes over a rope's per-frame update while a rope setting is on and 3D ropes are enabled in the game settings.
    /// Runs the game's own rope steps in the game's order and only decides when a 3D rope can keep its last shape and
    /// when to draw the game's flat rope instead.
    /// </summary>
    internal static class RopeLod
    {
        private const float MinRangeCheckInterval = 5f;
        private const float MaxRangeCheckInterval = 8f;
        private const float FlatRopeDeadZone = 1f;

        private static readonly Action<RopeEffect> UpdateAttachments = RopeMethod("UpdateAttachments");
        private static readonly Action<RopeEffect> ApplyLimits = RopeMethod("ApplyLimits");
        private static readonly Action<RopeEffect> UpdateTension = RopeMethod("UpdateTension");
        private static readonly Action<RopeEffect> DisplayRope = RopeMethod("DisplayRope");
        private static readonly Action<RopeEffect> CheckDistance = RopeMethod("CheckDistance");

        private static readonly Dictionary<RopeEffect, RopeState> states = new Dictionary<RopeEffect, RopeState>();
        private static int cameraFrame = -1;
        private static Vector3 cameraPosition;

        private static bool IsAnySettingOn
        {
            get
            {
                return ModConfig.SkipUnchangedRopes.Value
                    || ModConfig.SimplifyDistantRopes.Value != DistantRopeSimplification.Off;
            }
        }

        /// <summary>
        /// Performs this frame's update of <paramref name="rope"/>. Returns false, touching nothing, when the game's own
        /// update has to run instead: no rope setting is on, 3D ropes are off, or the rope is in a state the game
        /// handles specially.
        /// </summary>
        public static bool TryUpdate(RopeEffect rope, LineRenderer line, ClothRope cloth, ref float rangeCheckTimer, ref bool isOutOfRange)
        {
            if (!IsAnySettingOn || !Settings.clothRopes || cloth == null || rope.attachment == null
                || GameState.shipyardUpdatingOrder || Refs.observerMirror == null)
                return false;

            RunRangeCheck(rope, ref rangeCheckTimer);
            RopeState state = StateOf(rope, cloth);
            if (isOutOfRange)
                Hide(line, cloth, state);
            else if (WantsFlatRope(rope, state))
                DrawFlat(rope, line, cloth, state);
            else
                Draw3D(rope, line, cloth, state);
            return true;
        }

        /// <summary>
        /// Makes every rope rebuild its shape on its next update. Call when a rope setting changes.
        /// </summary>
        public static void RebuildAll()
        {
            foreach (RopeState state in states.Values)
                state.NeedsRebuild = true;
        }

        public static void Forget(RopeEffect rope)
        {
            states.Remove(rope);
        }

        private static Action<RopeEffect> RopeMethod(string name)
        {
            return AccessTools.MethodDelegate<Action<RopeEffect>>(AccessTools.Method(typeof(RopeEffect), name));
        }

        private static void RunRangeCheck(RopeEffect rope, ref float rangeCheckTimer)
        {
            if (rangeCheckTimer <= 0f)
            {
                CheckDistance(rope);
                rangeCheckTimer = Random.Range(MinRangeCheckInterval, MaxRangeCheckInterval);
            }

            rangeCheckTimer -= Time.deltaTime;
        }

        private static RopeState StateOf(RopeEffect rope, ClothRope cloth)
        {
            if (!states.TryGetValue(rope, out RopeState state) || state.Cloth != cloth)
            {
                state = new RopeState(cloth, rope.GetComponent<PickupableBoatMooringRope>());
                states[rope] = state;
            }

            return state;
        }

        private static void Hide(LineRenderer line, ClothRope cloth, RopeState state)
        {
            if (line.enabled)
                line.enabled = false;
            if (cloth.gameObject.activeSelf)
                cloth.gameObject.SetActive(false);
            state.NeedsRebuild = true;
        }

        /// <summary>
        /// Tells whether the rope is far enough from the viewer to be drawn flat, and records the answer in
        /// <paramref name="state"/>. A flat rope stays flat until the viewer comes <see cref="FlatRopeDeadZone"/> closer
        /// than the switching distance.
        /// </summary>
        private static bool WantsFlatRope(RopeEffect rope, RopeState state)
        {
            float flatBeyond = FlatRopeDistance(ModConfig.SimplifyDistantRopes.Value);
            if (state.IsFlat)
                flatBeyond -= FlatRopeDeadZone;
            state.IsFlat = DistanceFromViewer(rope) > flatBeyond;
            return state.IsFlat;
        }

        private static float FlatRopeDistance(DistantRopeSimplification simplification)
        {
            switch (simplification)
            {
                case DistantRopeSimplification.Quality:
                    return 50f;
                case DistantRopeSimplification.Default:
                    return 30f;
                case DistantRopeSimplification.Performance:
                    return 15f;
                default:
                    return float.PositiveInfinity;
            }
        }

        /// <summary>
        /// Returns the distance from the viewer to the nearest point of the straight line between the rope's two ends.
        /// </summary>
        private static float DistanceFromViewer(RopeEffect rope)
        {
            Vector3 start = rope.transform.position;
            Vector3 span = rope.attachment.position - start;
            Vector3 viewer = ViewerPosition();
            float spanLengthSquared = span.sqrMagnitude;
            float along = spanLengthSquared > 0f ? Mathf.Clamp01(Vector3.Dot(viewer - start, span) / spanLengthSquared) : 0f;
            return Vector3.Distance(viewer, start + span * along);
        }

        /// <summary>
        /// Returns the position rope distances are measured from this frame. Falls back to the player's character while
        /// there is no main camera.
        /// </summary>
        private static Vector3 ViewerPosition()
        {
            if (ModConfig.RopeDistanceMeasuredFrom.Value == RopeDistanceOrigin.Player)
                return Refs.observerMirror.transform.position;

            if (cameraFrame != Time.frameCount)
            {
                Camera camera = Camera.main;
                cameraPosition = camera != null ? camera.transform.position : Refs.observerMirror.transform.position;
                cameraFrame = Time.frameCount;
            }

            return cameraPosition;
        }

        /// <summary>
        /// Draws the rope as the game's flat rope.
        /// </summary>
        private static void DrawFlat(RopeEffect rope, LineRenderer line, ClothRope cloth, RopeState state)
        {
            if (!line.enabled)
                line.enabled = true;
            if (cloth.gameObject.activeSelf)
                cloth.gameObject.SetActive(false);

            RunGameRopeSteps(rope);
            state.NeedsRebuild = true;
        }

        private static void Draw3D(RopeEffect rope, LineRenderer line, ClothRope cloth, RopeState state)
        {
            if (line.enabled)
                line.enabled = false;
            if (!cloth.gameObject.activeSelf)
            {
                cloth.gameObject.SetActive(true);
                state.NeedsRebuild = true;
            }

            bool skipsUnchanged = ModConfig.SkipUnchangedRopes.Value;
            bool isStowedLine = skipsUnchanged && IsStowed(state.MooringLine);
            RopeShapeKey key = skipsUnchanged ? RopeShapeKey.Capture(rope, isStowedLine) : default(RopeShapeKey);
            if (skipsUnchanged && !state.NeedsRebuild && key.Matches(state.Key))
            {
                KeepEndRotations(rope, state);
                return;
            }

            if (isStowedLine)
                UprightRope.Build(rope, state.MooringLine.GetBoatRigidbody().transform.up);
            else
                RunGameRopeSteps(rope);
            cloth.UpdateRope();
            state.Remember(rope, key);
        }

        /// <summary>
        /// Tells whether <paramref name="line"/> is a mooring line lying in its place aboard, neither held nor tied.
        /// </summary>
        private static bool IsStowed(PickupableBoatMooringRope line)
        {
            return line != null && !line.held && !line.IsMoored() && line.IsAtInitialPos();
        }

        private static void RunGameRopeSteps(RopeEffect rope)
        {
            UpdateAttachments(rope);
            ApplyLimits(rope);
            UpdateTension(rope);
            DisplayRope(rope);
        }

        /// <summary>
        /// Puts the rope's end objects back into the orientation the last rebuild gave them.
        /// </summary>
        private static void KeepEndRotations(RopeEffect rope, RopeState state)
        {
            if (rope.transform.localRotation != state.RopeEndRotation)
                rope.transform.localRotation = state.RopeEndRotation;
            if (rope.attachment.localRotation != state.AttachmentRotation)
                rope.attachment.localRotation = state.AttachmentRotation;
        }

        private sealed class RopeState
        {
            public readonly ClothRope Cloth;

            public readonly PickupableBoatMooringRope MooringLine;

            public RopeShapeKey Key;

            public Quaternion RopeEndRotation;

            public Quaternion AttachmentRotation;

            public bool NeedsRebuild = true;

            public bool IsFlat;

            public RopeState(ClothRope cloth, PickupableBoatMooringRope mooringLine)
            {
                Cloth = cloth;
                MooringLine = mooringLine;
            }

            /// <summary>
            /// Records the rope as it was just rebuilt.
            /// </summary>
            public void Remember(RopeEffect rope, RopeShapeKey key)
            {
                Key = key;
                RopeEndRotation = rope.transform.localRotation;
                AttachmentRotation = rope.attachment.localRotation;
                NeedsRebuild = false;
            }
        }
    }
}
