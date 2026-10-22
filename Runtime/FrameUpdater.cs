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
            HiddenRopePauser.Update();
            ShopItemFreezer.Update();
        }
    }
}
