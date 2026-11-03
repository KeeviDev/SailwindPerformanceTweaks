using System;
using HarmonyLib;
using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Takes the sun and moon volumetric fog out of rendering while the fog is turned off in the game settings, and
    /// puts it back when it is turned on.
    /// </summary>
    internal static class FogRemoval
    {
        public const string TweakName = "Remove volumetric fog while it is off";

        private static readonly AccessTools.FieldRef<DirectionalLightAttachment, bool> IsInitialized =
            AccessTools.FieldRefAccess<DirectionalLightAttachment, bool>("Initialized");

        private static readonly Action<DirectionalLightAttachment> UpdateAttachment =
            AccessTools.MethodDelegate<Action<DirectionalLightAttachment>>(
                AccessTools.Method(typeof(DirectionalLightAttachment), "Update"));

        private static readonly Action<SimpleLightScattering> UpdateEffect =
            AccessTools.MethodDelegate<Action<SimpleLightScattering>>(
                AccessTools.Method(typeof(SimpleLightScattering), "Update"));

        /// <summary>
        /// Removes or restores the fog to match the game setting. Must run every frame.
        /// </summary>
        public static void Update()
        {
            VolufogManager manager = VolufogManager.instance;
            if (manager == null)
                return;

            bool isOff = !Settings.volufogOn;
            Match(manager.sunFog, isOff);
            Match(manager.moonFog, isOff);
        }

        /// <summary>
        /// Puts back the fog this tweak removed.
        /// </summary>
        public static void RestoreAll()
        {
            VolufogManager manager = VolufogManager.instance;
            if (manager == null)
                return;

            Restore(manager.sunFog);
            Restore(manager.moonFog);
        }

        private static void Match(SimpleLightScattering effect, bool isOff)
        {
            DirectionalLightAttachment attachment = effect.attachment;
            if (isOff && attachment.enabled && IsInitialized(attachment))
                Remove(effect);
            else if (!isOff && !attachment.enabled)
                Restore(effect);
        }

        /// <summary>
        /// Turns the effect off and takes its rendering off the camera and the light.
        /// </summary>
        private static void Remove(SimpleLightScattering effect)
        {
            effect.enabled = false;
            effect.attachment.enabled = false;
        }

        /// <summary>
        /// Rebuilds the effect's rendering on the camera and the light if it was taken off, gives its new materials
        /// this frame's fog parameters, and turns the effect on.
        /// </summary>
        private static void Restore(SimpleLightScattering effect)
        {
            DirectionalLightAttachment attachment = effect.attachment;
            if (!attachment.enabled)
            {
                attachment.enabled = true;
                attachment.InitRendering(effect.Downsample, effect.GetComponent<Camera>());
                UpdateAttachment(attachment);
                UpdateEffect(effect);
            }

            if (!effect.enabled)
                effect.enabled = true;
        }
    }
}
