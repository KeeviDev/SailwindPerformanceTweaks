using System.Collections;
using PerformanceTweaks.Configuration;
using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Keeps the hull dirt camera switched off while <see cref="ModConfig.DirtCameraOnDemand"/> is on, except for the
    /// frames around a full hull clean.
    /// </summary>
    internal static class DirtCameraSwitch
    {
        private const int FramesRenderedPerClean = 2;

        private static int renderUntilFrame = -1;

        /// <summary>
        /// Switches the camera of <paramref name="painter"/> on or off to match the setting.
        /// </summary>
        public static void Apply(MasterPainter painter)
        {
            if (painter == null)
                return;

            Camera camera = painter.GetComponent<Camera>();
            bool shouldRender = !ModConfig.DirtCameraOnDemand.Value || Time.frameCount <= renderUntilFrame;
            if (camera != null && camera.enabled != shouldRender)
                camera.enabled = shouldRender;
        }

        /// <summary>
        /// Lets the camera of <paramref name="painter"/> render this frame and the next, then switches it off again.
        /// </summary>
        public static void RenderForClean(MasterPainter painter)
        {
            renderUntilFrame = Time.frameCount + FramesRenderedPerClean - 1;
            Apply(painter);
            painter.StartCoroutine(ApplyAfterClean(painter));
        }

        private static IEnumerator ApplyAfterClean(MasterPainter painter)
        {
            while (Time.frameCount <= renderUntilFrame)
                yield return null;

            Apply(painter);
        }
    }
}
