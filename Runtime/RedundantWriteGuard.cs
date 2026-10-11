using PerformanceTweaks.Configuration;
using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Provides setters that leave the target untouched when it already holds the value, while
    /// <see cref="ModConfig.SkipRedundantItemUpdates"/> is on.
    /// </summary>
    internal static class RedundantWriteGuard
    {
        private const float PositionTolerance = 0.001f;
        private const float RotationToleranceDegrees = 0.1f;

        private static readonly float MinRotationDot = Mathf.Cos(0.5f * RotationToleranceDegrees * Mathf.Deg2Rad);

        public static bool IsSkipping
        {
            get { return ModConfig.SkipRedundantItemUpdates.Value; }
        }

        /// <summary>
        /// Sets the transform's position unless it is less than a millimetre away from the current one.
        /// </summary>
        public static void SetPosition(Transform transform, Vector3 position)
        {
            if (IsSkipping && IsSamePosition(transform, position))
                return;

            transform.position = position;
        }

        /// <summary>
        /// Sets the transform's rotation unless it is less than a tenth of a degree away from the current one.
        /// </summary>
        public static void SetRotation(Transform transform, Quaternion rotation)
        {
            if (IsSkipping && IsSameRotation(transform, rotation))
                return;

            transform.rotation = rotation;
        }

        public static void SetLocalScale(Transform transform, Vector3 scale)
        {
            if (IsSkipping && transform.localScale == scale)
                return;

            transform.localScale = scale;
        }

        public static void SetIsTrigger(Collider collider, bool isTrigger)
        {
            if (IsSkipping && collider.isTrigger == isTrigger)
                return;

            collider.isTrigger = isTrigger;
        }

        public static void SetEnabled(Collider collider, bool enabled)
        {
            if (IsSkipping && collider.enabled == enabled)
                return;

            collider.enabled = enabled;
        }

        /// <summary>
        /// Returns whether <paramref name="position"/> is less than a millimetre away from the transform's position.
        /// </summary>
        public static bool IsSamePosition(Transform transform, Vector3 position)
        {
            return (transform.position - position).sqrMagnitude < PositionTolerance * PositionTolerance;
        }

        /// <summary>
        /// Returns whether <paramref name="rotation"/> is less than a tenth of a degree away from the transform's
        /// rotation.
        /// </summary>
        public static bool IsSameRotation(Transform transform, Quaternion rotation)
        {
            return Mathf.Abs(Quaternion.Dot(transform.rotation, rotation)) >= MinRotationDot;
        }
    }
}
