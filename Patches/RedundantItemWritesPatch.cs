using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PerformanceTweaks.Runtime;
using UnityEngine;

namespace PerformanceTweaks.Patches
{
    /// <summary>
    /// Sends the transform and collider writes that items and crates repeat every frame or physics step through
    /// <see cref="RedundantWriteGuard"/>.
    /// </summary>
    [HarmonyPatch]
    internal static class RedundantItemWritesPatch
    {
        private static readonly Dictionary<MethodInfo, MethodInfo> GuardBySetter = new Dictionary<MethodInfo, MethodInfo>
        {
            { Setter(typeof(Transform), nameof(Transform.position)), Guard(nameof(RedundantWriteGuard.SetPosition)) },
            { Setter(typeof(Transform), nameof(Transform.rotation)), Guard(nameof(RedundantWriteGuard.SetRotation)) },
            { Setter(typeof(Transform), nameof(Transform.localScale)), Guard(nameof(RedundantWriteGuard.SetLocalScale)) },
            { Setter(typeof(Collider), nameof(Collider.isTrigger)), Guard(nameof(RedundantWriteGuard.SetIsTrigger)) },
            { Setter(typeof(Collider), nameof(Collider.enabled)), Guard(nameof(RedundantWriteGuard.SetEnabled)) },
        };

        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ItemRigidbody), "FixedUpdate");
            yield return AccessTools.Method(typeof(ItemRigidbody), nameof(ItemRigidbody.LateUpdate));
            yield return AccessTools.Method(typeof(ItemRigidbody), nameof(ItemRigidbody.ToggleCollider));
            yield return AccessTools.Method(typeof(ItemRigidbody), "SetSubcollidersTriggers");
            yield return AccessTools.Method(typeof(ItemRigidbody), "MoveRigidbodyToWalkCol");
            yield return AccessTools.Method(typeof(CrateInventory), nameof(CrateInventory.LateUpdate));
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var patched = new List<CodeInstruction>(instructions);
            int guardedWrites = 0;
            foreach (CodeInstruction instruction in patched)
            {
                if (instruction.operand is MethodInfo called && GuardBySetter.TryGetValue(called, out MethodInfo guard))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = guard;
                    guardedWrites++;
                }
            }

            if (guardedWrites == 0)
                Plugin.Log.LogWarning(string.Format("Found no writes to guard in {0}.{1}.", original.DeclaringType, original.Name));

            return patched;
        }

        private static MethodInfo Setter(Type type, string property)
        {
            return AccessTools.PropertySetter(type, property);
        }

        private static MethodInfo Guard(string method)
        {
            return AccessTools.Method(typeof(RedundantWriteGuard), method);
        }
    }
}
