using System.Collections.Generic;
using UnityEngine;

namespace PerformanceTweaks.Runtime
{
    /// <summary>
    /// Tells whether a boat stood nearly still during the last physics step, from its pose at the start of consecutive
    /// physics steps.
    /// </summary>
    internal static class BoatMotion
    {
        private const float ReachMetres = 20f;
        private const float StillMillimetres = 5f;
        private const float MillimetresPerMetre = 1000f;
        private const float MaxStepGap = 1.5f;
        private const float ForgetAfterSeconds = 1f;

        private static readonly List<BoatState> States = new List<BoatState>();
        private static BoatState lastState;

        /// <summary>
        /// Returns whether no point of <paramref name="boat"/> within 20 m of its pivot moved 5 mm or more since the
        /// previous physics step. Returns false when the boat was not measured in the previous physics step. Must be
        /// called during a physics step, before the simulation.
        /// </summary>
        public static bool IsStill(Transform boat)
        {
            float stepTime = Time.fixedTime;
            if (lastState == null || !ReferenceEquals(lastState.Boat, boat))
                lastState = StateOf(boat, stepTime);

            if (lastState.StepTime != stepTime)
                lastState.Measure(stepTime);
            return lastState.IsStill;
        }

        private static BoatState StateOf(Transform boat, float stepTime)
        {
            foreach (BoatState state in States)
            {
                if (ReferenceEquals(state.Boat, boat))
                    return state;
            }

            ForgetStaleStates(stepTime);
            var added = new BoatState(boat);
            States.Add(added);
            return added;
        }

        private static void ForgetStaleStates(float stepTime)
        {
            for (int i = States.Count - 1; i >= 0; i--)
            {
                if (stepTime - States[i].StepTime > ForgetAfterSeconds)
                    States.RemoveAt(i);
            }
        }

        /// <summary>
        /// Holds a boat's pose at the start of the last physics step it was measured in, and whether it stood still
        /// since the step before.
        /// </summary>
        private sealed class BoatState
        {
            public readonly Transform Boat;
            public float StepTime = float.NegativeInfinity;
            public bool IsStill;

            private Vector3 position;
            private Quaternion rotation;

            public BoatState(Transform boat)
            {
                Boat = boat;
            }

            /// <summary>
            /// Measures the boat at the start of the physics step at <paramref name="stepTime"/> and updates
            /// <see cref="IsStill"/>.
            /// </summary>
            public void Measure(float stepTime)
            {
                Vector3 newPosition = Boat.position;
                Quaternion newRotation = Boat.rotation;
                IsStill = stepTime - StepTime < MaxStepGap * Time.fixedDeltaTime
                    && MovementMillimetres(newPosition, newRotation) < StillMillimetres;
                StepTime = stepTime;
                position = newPosition;
                rotation = newRotation;
            }

            private float MovementMillimetres(Vector3 newPosition, Quaternion newRotation)
            {
                float shift = Vector3.Distance(newPosition, position);
                float turn = Quaternion.Angle(newRotation, rotation) * Mathf.Deg2Rad * ReachMetres;
                return (shift + turn) * MillimetresPerMetre;
            }
        }
    }
}
