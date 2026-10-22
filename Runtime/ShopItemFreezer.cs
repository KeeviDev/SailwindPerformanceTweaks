using System.Collections.Generic;
using cakeslice;
using HarmonyLib;
using PerformanceTweaks.Configuration;
using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Switches off the per-frame components of every registered shop item that nobody holds, looks at or bought,
    /// while <see cref="ModConfig.FreezeIdleShopItems"/> is on, and switches them back on as soon as that changes.
    /// Items with a component named <see cref="KeepUpdatingMarker"/> are never frozen.
    /// </summary>
    internal static class ShopItemFreezer
    {
        private const string KeepUpdatingMarker = "KeepUpdatingInShop";
        private const int IdleFramesBeforeFreezing = 10;
        private const float IdleSecondsAfterHolding = 1f;

        private static readonly AccessTools.FieldRef<GoPointerButton, bool> IsClicked =
            AccessTools.FieldRefAccess<GoPointerButton, bool>("isClicked");

        private static readonly AccessTools.FieldRef<GoPointerButton, GoPointer> StickyClickedBy =
            AccessTools.FieldRefAccess<GoPointerButton, GoPointer>("stickyClickedBy");

        private static readonly AccessTools.FieldRef<GoPointerButton, float> FlashOutlineTimer =
            AccessTools.FieldRefAccess<GoPointerButton, float>("flashOutlineTimer");

        private static readonly AccessTools.FieldRef<GoPointerButton, Outline> OutlineOf =
            AccessTools.FieldRefAccess<GoPointerButton, Outline>("outline");

        private static readonly AccessTools.FieldRef<ShipItemLight, bool> IsLightOn =
            AccessTools.FieldRefAccess<ShipItemLight, bool>("on");

        private static readonly List<FreezableItem> items = new List<FreezableItem>();
        private static readonly Dictionary<ShipItem, FreezableItem> itemsByShipItem = new Dictionary<ShipItem, FreezableItem>();
        private static bool hasFrozenItems;

        public static void Register(ShipItem item)
        {
            if (item == null || itemsByShipItem.ContainsKey(item))
                return;

            var entry = new FreezableItem(item);
            items.Add(entry);
            itemsByShipItem[item] = entry;
        }

        /// <summary>
        /// Freezes the registered items that have been idle for <see cref="IdleFramesBeforeFreezing"/> frames and thaws
        /// the frozen ones that are no longer idle. Must run every frame.
        /// </summary>
        public static void Update()
        {
            bool isFreezing = ModConfig.FreezeIdleShopItems.Value;
            if (!isFreezing && !hasFrozenItems)
                return;

            hasFrozenItems = false;
            for (int index = items.Count - 1; index >= 0; index--)
            {
                FreezableItem entry = items[index];
                if (entry.Item == null)
                {
                    items.RemoveAt(index);
                    itemsByShipItem.Remove(entry.Item);
                    continue;
                }

                UpdateEntry(entry, isFreezing);
                hasFrozenItems |= entry.IsFrozen;
            }
        }

        /// <summary>
        /// Thaws <paramref name="button"/> if it is a frozen shop item.
        /// </summary>
        public static void Thaw(GoPointerButton button)
        {
            if (button is ShipItem item && itemsByShipItem.TryGetValue(item, out FreezableItem entry) && entry.IsFrozen)
                entry.Thaw();
        }

        private static void UpdateEntry(FreezableItem entry, bool isFreezing)
        {
            if (entry.Item.held)
                entry.LastHeldTime = Time.time;

            if (!isFreezing || !IsIdle(entry))
            {
                if (entry.IsFrozen)
                    entry.Thaw();
                entry.IdleFrames = 0;
                return;
            }

            if (!entry.IsFrozen && ++entry.IdleFrames >= IdleFramesBeforeFreezing)
                entry.Freeze();
        }

        /// <summary>
        /// Tells whether the entry's item is an unsold shop item that nobody held for <see cref="IdleSecondsAfterHolding"/>
        /// and nobody looks at or clicks, that shows no outline and that may freeze.
        /// </summary>
        private static bool IsIdle(FreezableItem entry)
        {
            ShipItem item = entry.Item;
            if (item.sold || item.held || Time.time - entry.LastHeldTime < IdleSecondsAfterHolding)
                return false;
            if (item.IsLookedAt() || IsClicked(item) || StickyClickedBy(item) != null)
                return false;
            if (FlashOutlineTimer(item) > 0f || item.enableRedOutline)
                return false;

            Outline outline = OutlineOf(item);
            return (outline == null || !outline.enabled) && entry.MayFreeze && !AnimatesWhileIdle(item);
        }

        private static bool AnimatesWhileIdle(ShipItem item)
        {
            return item is ShipItemClock
                || item is ShipItemTotem
                || (item is ShipItemLight light && IsLightOn(light));
        }

        private static bool HasKeepUpdatingMarker(ShipItem item)
        {
            foreach (MonoBehaviour behaviour in item.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour != null && behaviour.GetType().Name == KeepUpdatingMarker)
                    return true;
            }

            return false;
        }

        private sealed class FreezableItem
        {
            public readonly ShipItem Item;

            public int IdleFrames;

            public float LastHeldTime = float.NegativeInfinity;

            private readonly List<Behaviour> switchedOff = new List<Behaviour>();

            private bool? mayFreeze;

            public FreezableItem(ShipItem item)
            {
                Item = item;
            }

            public bool IsFrozen { get; private set; }

            /// <summary>
            /// Gets whether the item carries no <see cref="KeepUpdatingMarker"/> component.
            /// </summary>
            public bool MayFreeze
            {
                get
                {
                    if (mayFreeze == null)
                        mayFreeze = !HasKeepUpdatingMarker(Item);
                    return mayFreeze.Value;
                }
            }

            /// <summary>
            /// Switches off the item's per-frame components that are on, remembering which ones it switched off.
            /// </summary>
            public void Freeze()
            {
                SwitchOff(Item);
                SwitchOff(Item.itemRigidbodyC);
                SwitchOff(Item.colChecker);
                SwitchOff(Item.GetComponent<FoodState>());
                SwitchOff(Item.GetComponent<CookableFood>());
                IsFrozen = true;
            }

            /// <summary>
            /// Switches back on every component <see cref="Freeze"/> switched off.
            /// </summary>
            public void Thaw()
            {
                foreach (Behaviour behaviour in switchedOff)
                {
                    if (behaviour != null)
                        behaviour.enabled = true;
                }

                switchedOff.Clear();
                IdleFrames = 0;
                IsFrozen = false;
            }

            private void SwitchOff(Behaviour behaviour)
            {
                if (behaviour == null || !behaviour.enabled)
                    return;

                behaviour.enabled = false;
                switchedOff.Add(behaviour);
            }
        }
    }
}
