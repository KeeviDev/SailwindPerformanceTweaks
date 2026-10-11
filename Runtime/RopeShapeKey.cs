using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Holds everything a rope's shape depends on, relative to the object the rope hangs from, so that two matching
    /// keys produce the same rope as seen on its boat.
    /// </summary>
    internal struct RopeShapeKey
    {
        private const float PositionTolerance = 0.001f;
        private const float SagSwingTolerance = 0.005f;
        private const float LengthTolerance = 1e-4f;
        private const float MaxTiltTolerance = 5f * Mathf.Deg2Rad;
        private const float GameCurveRateFactor = 0.75f;
        private const float BezierMidpointShare = 0.75f;
        private const float MaxHalfSagAngleDegrees = 90f;

        private Vector3 ropeEnd;
        private Vector3 attachmentEnd;
        private Vector3 worldUp;
        private float sag;
        private bool isStowedLine;
        private bool ropeEndIsHigher;
        private float currentLength;
        private float totalLength;
        private float width;
        private float sailArea;

        /// <summary>
        /// Reads the rope's current shape inputs. Requires <see cref="RopeEffect.attachment"/> to be set.
        /// </summary>
        /// <param name="isStowedLine">Whether the rope is a mooring line lying in its place aboard.</param>
        public static RopeShapeKey Capture(RopeEffect rope, bool isStowedLine)
        {
            Transform frame = rope.transform.parent;
            Vector3 ropeEndWorld = rope.transform.position;
            Vector3 attachmentWorld = rope.attachment.position;
            return new RopeShapeKey
            {
                ropeEnd = frame != null ? frame.InverseTransformPoint(ropeEndWorld) : ropeEndWorld,
                attachmentEnd = frame != null ? frame.InverseTransformPoint(attachmentWorld) : attachmentWorld,
                worldUp = frame != null ? frame.InverseTransformDirection(Vector3.up) : Vector3.up,
                sag = EstimateSag(rope, Vector3.Distance(ropeEndWorld, attachmentWorld)),
                isStowedLine = isStowedLine,
                ropeEndIsHigher = ropeEndWorld.y >= attachmentWorld.y,
                currentLength = rope.currentRopeLength,
                totalLength = rope.totalRopeLength,
                width = rope.ropeWidth,
                sailArea = rope.attachedSailArea,
            };
        }

        /// <summary>
        /// Tells whether the rope would look the same: its ends within a millimetre, its sag swung by the boat's tilt
        /// within five millimetres, and the same lengths, width and sail area. The tilt of a mooring line that stayed
        /// stowed is not compared.
        /// </summary>
        public bool Matches(RopeShapeKey other)
        {
            return IsNear(ropeEnd, other.ropeEnd)
                && IsNear(attachmentEnd, other.attachmentEnd)
                && isStowedLine == other.isStowedLine
                && (isStowedLine || (ropeEndIsHigher == other.ropeEndIsHigher && IsTiltNear(other)))
                && Mathf.Abs(currentLength - other.currentLength) < LengthTolerance
                && Mathf.Abs(totalLength - other.totalLength) < LengthTolerance
                && Mathf.Abs(width - other.width) < LengthTolerance
                && Mathf.Abs(sailArea - other.sailArea) < LengthTolerance;
        }

        private bool IsTiltNear(RopeShapeKey other)
        {
            float minTiltCosine = Mathf.Cos(TiltTolerance(Mathf.Max(sag, other.sag)));
            return Vector3.Dot(worldUp.normalized, other.worldUp.normalized) > minTiltCosine;
        }

        /// <summary>
        /// Returns an upper bound, in metres, for how far the middle of the rope hangs from the straight line between
        /// its ends.
        /// </summary>
        /// <param name="span">Distance between the rope's ends, in metres.</param>
        private static float EstimateSag(RopeEffect rope, float span)
        {
            float slack = 1f - rope.totalRopeLength;
            float curveRate = slack * Mathf.Clamp01(rope.currentRopeLength) * GameCurveRateFactor;
            float halfSagAngle = Mathf.Clamp(slack * MaxHalfSagAngleDegrees, -MaxHalfSagAngleDegrees, MaxHalfSagAngleDegrees);
            return Mathf.Abs(BezierMidpointShare * span * curveRate * Mathf.Sin(halfSagAngle * Mathf.Deg2Rad));
        }

        /// <summary>
        /// Returns the change of the boat's tilt, in radians, that swings a sag of <paramref name="sag"/> metres by
        /// <see cref="SagSwingTolerance"/>, at most <see cref="MaxTiltTolerance"/>.
        /// </summary>
        private static float TiltTolerance(float sag)
        {
            return sag * MaxTiltTolerance > SagSwingTolerance ? SagSwingTolerance / sag : MaxTiltTolerance;
        }

        private static bool IsNear(Vector3 point, Vector3 other)
        {
            return (point - other).sqrMagnitude < PositionTolerance * PositionTolerance;
        }
    }
}
