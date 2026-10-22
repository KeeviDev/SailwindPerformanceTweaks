using HarmonyLib;
using PerformanceTweaks.Runtime;

namespace PerformanceTweaks.Patches
{
    [HarmonyPatch(typeof(ShopItemSpawner), "SpawnItem")]
    internal static class ShopItemSpawnPatch
    {
        private static void Postfix(ShipItem ___item)
        {
            ShopItemFreezer.Register(___item);
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
            ShopItemFreezer.Thaw(__instance);
        }
    }
}
