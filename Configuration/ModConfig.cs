using BepInEx.Configuration;

namespace PerformanceTweaks.Configuration
{
    /// <summary>
    /// Holds Performance Tweaks' user-facing settings. <see cref="Bind"/> must run before any setting is read.
    /// </summary>
    internal static class ModConfig
    {
        private const string ItemsSection = "Items";
        private const string BoatsSection = "Boats";
        private const string RenderingSection = "Rendering";

        public static ConfigEntry<bool> SkipRedundantItemUpdates { get; private set; }
        public static ConfigEntry<bool> SkipUnchangedRopes { get; private set; }
        public static ConfigEntry<DistantRopeSimplification> SimplifyDistantRopes { get; private set; }
        public static ConfigEntry<RopeDistanceOrigin> RopeDistanceMeasuredFrom { get; private set; }
        public static ConfigEntry<bool> DirtCameraOnDemand { get; private set; }

        public static void Bind(ConfigFile config)
        {
            SkipRedundantItemUpdates = config.Bind(
                ItemsSection,
                "Skip redundant item updates",
                true,
                new ConfigDescription(
                    "Skips item and crate updates that would change nothing."));

            SkipUnchangedRopes = config.Bind(
                BoatsSection,
                "Skip unchanged ropes",
                true,
                new ConfigDescription(
                    "Rebuilds a 3D rope only when it has moved on its boat."));

            SimplifyDistantRopes = config.Bind(
                BoatsSection,
                "Simplify distant ropes",
                DistantRopeSimplification.Default,
                new ConfigDescription(
                    "Draws distant ropes as the game's flat ropes instead of 3D ropes."));

            RopeDistanceMeasuredFrom = config.Bind(
                BoatsSection,
                "Rope distance measured from",
                RopeDistanceOrigin.Camera,
                new ConfigDescription(
                    "Where the distance for Simplify distant ropes is measured from: the camera or your character."));

            DirtCameraOnDemand = config.Bind(
                RenderingSection,
                "Dirt camera on demand",
                true,
                new ConfigDescription(
                    "Turns the hidden dirt camera on only when it is really needed."));
        }
    }
}
