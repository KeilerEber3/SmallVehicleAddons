using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SmallVehicleAddons
{
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches_SmallVehicleAddons
    {
        private const string SMAId = "Inoshishi3.SmallVehicleAddons";
        static HarmonyPatches_SmallVehicleAddons()
        {
            Harmony harmony = new Harmony(SMAId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        [HarmonyPatch(typeof(GlowGrid), "GroundGlowAt")]
        private static class HarmonyPatches_GroundGlowAt
        {
            [HarmonyPostfix]
            private static void Postfix(GlowGrid __instance, IntVec3 c, ref float __result)
            {
                if (SmallVehicleAddons_Utility.TryGetLights(__instance, out List<CompLights> lights))
                {
                    float extra = 0f;
                    for (int i = 0; i < lights.Count; i++)
                    {
                        CompLights comp = lights[i];
                        if (comp != null)
                        {
                            extra += comp.GetCellGroundGlow(c);
                        }
                    }
                    if (extra > 0f)
                    {
                        __result = Mathf.Clamp01(__result + extra);
                    }
                }
            }
        }
        //[HarmonyPatch(typeof(ThingGrid), "RegisterInCell")]
        //private static class HarmonyPatches_ThingGrid_RegisterInCell
        //{
        //    [HarmonyPostfix]
        //    private static void Postfix(ThingGrid __instance, Thing t, IntVec3 c, Map ___map)
        //    {
        //        if (t.def.blockLight && c.InBounds(___map))
        //        {
        //            SmallVehicleAddons_Utility.NotifyBlockerChanged(___map.glowGrid, c);
        //        }
        //    }
        //}
        //[HarmonyPatch(typeof(ThingGrid), "DeregisterInCell")]
        //private static class HarmonyPatches_ThingGrid_DeregisterInCell
        //{
        //    [HarmonyPostfix]
        //    private static void Postfix(ThingGrid __instance, Thing t, IntVec3 c, Map ___map)
        //    {
        //        if (t.def.blockLight && c.InBounds(___map))
        //        {
        //            SmallVehicleAddons_Utility.NotifyBlockerChanged(___map.glowGrid, c);
        //        }
        //    }
        //}
        [HarmonyPatch(typeof(GlowGrid), "GetAccumulatedGlowAt", new Type[]
{
            typeof(int),
            typeof(bool)
})]
        private static class HarmonyPatches_GetAccumulatedGlowAt
        {
            [HarmonyPostfix]
            private static void Postfix(GlowGrid __instance, int index, ref Color32 __result, Map ___map)
            {
                if (SmallVehicleAddons_Utility.TryGetLights(__instance, out List<CompLights> lights))
                {
                    int r = __result.r;
                    int g = __result.g;
                    int b = __result.b;
                    for (int i = 0; i < lights.Count; i++)
                    {
                        CompLights comp = lights[i];
                        if (comp == null)
                        {
                            continue;
                        }
                        Color contribution;
                        if (!comp.TryGetCellVisualContribution(index, out contribution))
                        {
                            continue;
                        }
                        r += Mathf.RoundToInt(Mathf.Max(0f, contribution.r) * 255f);
                        g += Mathf.RoundToInt(Mathf.Max(0f, contribution.g) * 255f);
                        b += Mathf.RoundToInt(Mathf.Max(0f, contribution.b) * 255f);
                    }
                    __result = new Color32((byte)Mathf.Clamp(r, 0, 255), (byte)Mathf.Clamp(g, 0, 255), (byte)Mathf.Clamp(b, 0, 255), __result.a);
                }
            }
        }
        //        [HarmonyPatch(typeof(GlowGrid), "VisualGlowAt", new Type[]
        //{
        //            typeof(int)
        //})]
        //        private static class HarmonyPatches_VisualGlowAt
        //        {
        //            [HarmonyPostfix]
        //            private static void Postfix(GlowGrid __instance, int index, ref Color32 __result)
        //            {
        //                if (SmallVehicleAddons_Utility.TryGetLights(__instance, out List<CompLights> lights))
        //                {
        //                    int r = __result.r;
        //                    int g = __result.g;
        //                    int b = __result.b;
        //                    for (int i = 0; i < lights.Count; i++)
        //                    {
        //                        CompLights comp = lights[i];
        //                        if (comp == null)
        //                        {
        //                            continue;
        //                        }
        //                        Color contribution;
        //                        if (!comp.TryGetCellVisualContribution(index, out contribution))
        //                        {
        //                            continue;
        //                        }
        //                        r += Mathf.RoundToInt(Mathf.Max(0f, contribution.r) * 255f);
        //                        g += Mathf.RoundToInt(Mathf.Max(0f, contribution.g) * 255f);
        //                        b += Mathf.RoundToInt(Mathf.Max(0f, contribution.b) * 255f);
        //                    }
        //                    __result = new Color32((byte)Mathf.Clamp(r, 0, 255), (byte)Mathf.Clamp(g, 0, 255), (byte)Mathf.Clamp(b, 0, 255), __result.a);
        //                }
        //            }
        //        }
        [HarmonyPatch(typeof(Building_Door), "DoorOpen")]
        private static class HarmonyPatches_DoorOpen
        {
            [HarmonyPostfix]
            private static void Postfix(Building_Door __instance)
            {
                if (__instance.Open && __instance.Spawned)
                {
                    SmallVehicleAddons_Utility.NotifyBlockerChanged(__instance.Map.glowGrid, __instance.PositionHeld);
                }
            }
        }
        [HarmonyPatch(typeof(Building_Door), "DoorTryClose")]
        private static class HarmonyPatches_DoorTryClose
        {
            [HarmonyPostfix]
            private static void Postfix(Building_Door __instance, ref bool __result)
            {
                if (__result && __instance.Spawned)
                {
                    SmallVehicleAddons_Utility.NotifyBlockerChanged(__instance.Map.glowGrid, __instance.PositionHeld);
                }
            }
        }
    }
}