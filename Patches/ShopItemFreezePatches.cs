using System;
using HarmonyLib;
using PerformanceTweaks.Configuration;
using PerformanceTweaks.Runtime;

namespace PerformanceTweaks.Patches
{
    [HarmonyPatch(typeof(ShopItemSpawner), "SpawnItem")]
    internal static class ShopItemSpawnPatch
    {
        private static void Postfix(ShipItem ___item)
        {
            if (TweakGuard.IsStopped(ModConfig.FreezeIdleShopItems))
                return;

            try
            {
                ShopItemFreezer.Register(___item);
            }
            catch (Exception ex)
            {
                TweakGuard.Stop(ModConfig.FreezeIdleShopItems, ex, ShopItemFreezer.ThawAll);
            }
        }
    }

    /// <summary>
    /// Thaws a frozen shop item as soon as a pointer looks at it.
    /// </summary>
    [HarmonyPatch(typeof(GoPointerButton), nameof(GoPointerButton.Look))]
    internal static class ShopItemLookPatch
    {
        private static void Postfix(GoPointerButton __instance)
        {
            if (TweakGuard.IsStopped(ModConfig.FreezeIdleShopItems))
                return;

            try
            {
                ShopItemFreezer.Thaw(__instance);
            }
            catch (Exception ex)
            {
                TweakGuard.Stop(ModConfig.FreezeIdleShopItems, ex, ShopItemFreezer.ThawAll);
            }
        }
    }
}
