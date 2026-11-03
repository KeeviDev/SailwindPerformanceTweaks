using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace PerformanceTweaks.Configuration
{
    /// <summary>Turns a tweak off until the game restarts once it throws, so the game goes on without it.</summary>
    internal static class TweakGuard
    {
        private static readonly HashSet<string> Stopped = new HashSet<string>();

        /// <summary>Gets whether <paramref name="tweak"/> is turned on and has not been stopped by an error.</summary>
        public static bool IsOn(ConfigEntry<bool> tweak)
        {
            return tweak.Value && !IsStopped(tweak);
        }

        public static bool IsStopped(ConfigEntryBase tweak)
        {
            return Stopped.Count > 0 && Stopped.Contains(tweak.Definition.Key);
        }

        /// <summary>Gets whether the tweak named <paramref name="tweak"/> has been stopped by an error.</summary>
        public static bool IsStopped(string tweak)
        {
            return Stopped.Count > 0 && Stopped.Contains(tweak);
        }

        /// <inheritdoc cref="Stop(string, Exception, Action)"/>
        public static void Stop(ConfigEntryBase tweak, Exception error, Action undo = null)
        {
            Stop(tweak.Definition.Key, error, undo);
        }

        /// <summary>
        /// Turns the tweak named <paramref name="tweak"/> off until the game restarts, logs <paramref name="error"/>
        /// and runs <paramref name="undo"/>, the first time it is called for that tweak.
        /// </summary>
        /// <param name="undo">Puts back what the tweak changed in the game; its own errors are logged.</param>
        public static void Stop(string tweak, Exception error, Action undo = null)
        {
            if (!Stopped.Add(tweak))
                return;

            Plugin.Log.LogError(string.Format(
                "\"{0}\" ran into an error and is off until the game restarts: {1}",
                tweak,
                error));
            if (undo == null)
                return;

            try
            {
                undo();
            }
            catch (Exception undoError)
            {
                Plugin.Log.LogError(string.Format("\"{0}\" could not undo its changes: {1}", tweak, undoError));
            }
        }
    }
}
