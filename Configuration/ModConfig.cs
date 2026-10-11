using BepInEx.Configuration;

namespace PerformanceTweaks.Configuration
{
    /// <summary>
    /// Holds Performance Tweaks' user-facing settings. <see cref="Bind"/> must run before any setting is read.
    /// </summary>
    internal static class ModConfig
    {
        private const string ItemsSection = "Items";

        public static ConfigEntry<bool> SkipRedundantItemUpdates { get; private set; }

        public static void Bind(ConfigFile config)
        {
            SkipRedundantItemUpdates = config.Bind(
                ItemsSection,
                "Skip redundant item updates",
                true,
                new ConfigDescription(
                    "Skips item and crate updates that would change nothing."));
        }
    }
}
