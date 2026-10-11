using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Places an item aboard its boat where its twin stands in the boat's walk copy, the way the game does in every
    /// physics step, but skips the write while the boat stands still and the item is already in place.
    /// </summary>
    internal static class ItemPlacement
    {
        /// <summary>
        /// Places <paramref name="item"/> from <paramref name="twin"/>, or leaves it in place when that would change
        /// nothing on a still boat. Returns false, touching nothing, when the game's own placement has to run instead:
        /// skipping is off or the item's boat moved during the last physics step.
        /// </summary>
        public static bool TryPlace(Transform twin, ShipItem item)
        {
            if (!RedundantWriteGuard.IsSkipping)
                return false;

            Transform boat = item.currentActualBoat;
            if (ReferenceEquals(boat, null) || !BoatMotion.IsStill(boat))
                return false;

            Transform walkCopy = item.currentWalkCol;
            Vector3 position = boat.TransformPoint(walkCopy.InverseTransformPoint(twin.position));
            Quaternion rotation = boat.rotation * (Quaternion.Inverse(walkCopy.rotation) * twin.rotation);
            Transform placed = item.transform;
            if (RedundantWriteGuard.IsSamePosition(placed, position) && RedundantWriteGuard.IsSameRotation(placed, rotation))
                return true;

            placed.position = position;
            placed.rotation = rotation;
            return true;
        }
    }
}
