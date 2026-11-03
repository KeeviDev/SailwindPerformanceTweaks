using System;
using PerformanceTweaks.Configuration;
using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Runs the tweaks that need to act every frame.
    /// </summary>
    internal sealed class FrameUpdater : MonoBehaviour
    {
        private void Update()
        {
            PauseHiddenRopes();
            FreezeIdleShopItems();
            RemoveFogWhileOff();
        }

        private static void PauseHiddenRopes()
        {
            if (TweakGuard.IsStopped(ModConfig.PauseHiddenRopes))
                return;

            try
            {
                HiddenRopePauser.Update();
            }
            catch (Exception ex)
            {
                TweakGuard.Stop(ModConfig.PauseHiddenRopes, ex, HiddenRopePauser.ResumeAll);
            }
        }

        private static void FreezeIdleShopItems()
        {
            if (TweakGuard.IsStopped(ModConfig.FreezeIdleShopItems))
                return;

            try
            {
                ShopItemFreezer.Update();
            }
            catch (Exception ex)
            {
                TweakGuard.Stop(ModConfig.FreezeIdleShopItems, ex, ShopItemFreezer.ThawAll);
            }
        }

        private static void RemoveFogWhileOff()
        {
            if (TweakGuard.IsStopped(FogRemoval.TweakName))
                return;

            try
            {
                FogRemoval.Update();
            }
            catch (Exception ex)
            {
                TweakGuard.Stop(FogRemoval.TweakName, ex, FogRemoval.RestoreAll);
            }
        }
    }
}
