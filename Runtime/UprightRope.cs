using System;
using HarmonyLib;
using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Builds a rope's shape with the game's own rope steps, but as if <c>up</c> were the world's up, so the rope hangs
    /// on its boat as it would with the boat upright.
    /// </summary>
    internal static class UprightRope
    {
        private const float CurveRateFactor = 0.75f;
        private const float MaxSagDegrees = 180f;
        private const float UprightDegrees = 90f;

        private static readonly AccessTools.FieldRef<RopeEffect, Transform> AttachmentOne =
            AccessTools.FieldRefAccess<RopeEffect, Transform>("attachmentOne");

        private static readonly AccessTools.FieldRef<RopeEffect, Transform> AttachmentTwo =
            AccessTools.FieldRefAccess<RopeEffect, Transform>("attachmentTwo");

        private static readonly Action<RopeEffect> ApplyLimits = RopeMethod("ApplyLimits");
        private static readonly Action<RopeEffect> DisplayRope = RopeMethod("DisplayRope");

        /// <summary>
        /// Rebuilds <paramref name="rope"/>'s shape and turns its end objects as the game's rope update would with
        /// <paramref name="up"/> pointing straight up.
        /// </summary>
        public static void Build(RopeEffect rope, Vector3 up)
        {
            AssignEnds(rope, up);
            ApplyLimits(rope);
            ApplyTension(rope, up);
            DisplayRope(rope);
        }

        private static Action<RopeEffect> RopeMethod(string name)
        {
            return AccessTools.MethodDelegate<Action<RopeEffect>>(AccessTools.Method(typeof(RopeEffect), name));
        }

        private static void AssignEnds(RopeEffect rope, Vector3 up)
        {
            Transform ropeEnd = rope.transform;
            Transform attachment = rope.attachment;
            bool ropeEndIsHigher = !rope.sheet && !rope.jibSheet
                && Vector3.Dot(ropeEnd.position - attachment.position, up) >= 0f;
            AttachmentOne(rope) = ropeEndIsHigher ? ropeEnd : attachment;
            AttachmentTwo(rope) = ropeEndIsHigher ? attachment : ropeEnd;
        }

        private static void ApplyTension(RopeEffect rope, Vector3 up)
        {
            Transform one = AttachmentOne(rope);
            Transform two = AttachmentTwo(rope);
            one.LookAt(two, up);
            two.LookAt(one, up);

            float slack = 1f - rope.totalRopeLength;
            rope.curveRate = slack * rope.currentRopeLength * CurveRateFactor;
            float upwardness = (UprightDegrees - Vector3.Angle(two.forward, up)) / UprightDegrees;
            float sagDegrees = slack * MaxSagDegrees;
            one.Rotate(Vector3.right * (0.5f - 0.5f * upwardness) * sagDegrees, Space.Self);
            two.Rotate(Vector3.right * (0.5f + 0.5f * upwardness) * sagDegrees, Space.Self);
        }
    }
}
