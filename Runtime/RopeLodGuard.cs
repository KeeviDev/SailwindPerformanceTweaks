using System;
using PerformanceTweaks.Configuration;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Turns off together the rope settings that <see cref="RopeLod"/> serves, once it throws.
    /// </summary>
    internal static class RopeLodGuard
    {
        public static bool IsStopped
        {
            get { return TweakGuard.IsStopped(ModConfig.SkipUnchangedRopes); }
        }

        public static void Stop(Exception error)
        {
            TweakGuard.Stop(ModConfig.SkipUnchangedRopes, error);
            TweakGuard.Stop(ModConfig.SimplifyDistantRopes, error);
        }
    }
}
