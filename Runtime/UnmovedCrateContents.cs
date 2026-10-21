using System.Collections.Generic;
using PerformanceTweaks.Configuration;
using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Tells when a crate's per-frame placement of its contents would change nothing, while
    /// <see cref="ModConfig.SkipUnmovedCrateContents"/> is on.
    /// </summary>
    internal static class UnmovedCrateContents
    {
        private const float PositionTolerance = 0.0001f;
        private const float MinRotationDot = 0.9999999f;
        private const int PruneEveryChecks = 4096;

        private static readonly Dictionary<CrateInventory, CratePose> lastPoseByCrate = new Dictionary<CrateInventory, CratePose>();
        private static readonly List<CrateInventory> destroyedCrates = new List<CrateInventory>();
        private static int checksSincePrune;

        /// <summary>
        /// Returns true when the crate, its parent and its contents are the same as in its previous update and every
        /// item in it shares the crate's parent. Records the crate's current state either way.
        /// </summary>
        public static bool CanSkip(CrateInventory crate)
        {
            if (!ModConfig.SkipUnmovedCrateContents.Value)
                return false;

            PruneDestroyedCratesNowAndThen();
            if (IsOpenInUi(crate) || !CratePose.TryCapture(crate, out CratePose pose))
            {
                lastPoseByCrate.Remove(crate);
                return false;
            }

            bool isUnchanged = lastPoseByCrate.TryGetValue(crate, out CratePose last) && last.Matches(pose);
            lastPoseByCrate[crate] = pose;
            return isUnchanged;
        }

        /// <summary>
        /// Forgets the recorded state of every destroyed crate once every <see cref="PruneEveryChecks"/> calls.
        /// </summary>
        private static void PruneDestroyedCratesNowAndThen()
        {
            if (++checksSincePrune < PruneEveryChecks)
                return;

            checksSincePrune = 0;
            foreach (CrateInventory crate in lastPoseByCrate.Keys)
            {
                if (crate == null)
                    destroyedCrates.Add(crate);
            }

            foreach (CrateInventory crate in destroyedCrates)
                lastPoseByCrate.Remove(crate);
            destroyedCrates.Clear();
        }

        private static bool IsOpenInUi(CrateInventory crate)
        {
            CrateInventoryUI ui = CrateInventoryUI.instance;
            return ui != null && ui.showingUI && ui.currentCrate == crate;
        }

        private struct CratePose
        {
            private Transform parent;
            private Vector3 localPosition;
            private Quaternion localRotation;
            private int contentsHash;

            /// <summary>
            /// Reads the crate's state. Fails when an item in it no longer exists or does not share the crate's
            /// parent.
            /// </summary>
            public static bool TryCapture(CrateInventory crate, out CratePose pose)
            {
                Transform transform = crate.transform;
                pose = new CratePose
                {
                    parent = transform.parent,
                    localPosition = transform.localPosition,
                    localRotation = transform.localRotation,
                    contentsHash = crate.containedItems.Count,
                };

                foreach (ShipItem item in crate.containedItems)
                {
                    if (item == null || item.transform.parent != pose.parent)
                        return false;
                    pose.contentsHash = pose.contentsHash * 31 + item.GetInstanceID();
                }

                return true;
            }

            public bool Matches(CratePose other)
            {
                return parent == other.parent
                    && contentsHash == other.contentsHash
                    && (localPosition - other.localPosition).sqrMagnitude < PositionTolerance * PositionTolerance
                    && Mathf.Abs(Quaternion.Dot(localRotation, other.localRotation)) >= MinRotationDot;
            }
        }
    }
}
