using CustomizeLib.BepInEx.Hook;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// native update hooks are intentionally not used; PlantEventDriver dispatches updates.
using UnityEngine;

namespace CustomizeLib.BepInEx.Extra.PlantExtra.IPlantEvent
{
    #region HarmonyPatch
    public static class PlantPatches
    {
        [HarmonyPatch]
        [HarmonyPriority(Priority.First)] // 数值越大执行顺序越靠后
        public static class PlantDiePatch
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetTargetMethods()
            {
                return SystemTools.GetAllMethods(SystemTools.GetAllDerivedTypes(typeof(Plant)), nameof(Plant.Die),
                    BindingFlags.Default.AddAllAccess().AddInstance().AddDeclaredOnly());
            }

            [HarmonyPrefix]
            public static void PreDie(Plant __instance, Plant.DieReason __0)
            {
                if (__instance != null && PlantEvent.HasEventComp(__instance))
                    PlantEvent.DieEvent(__instance, __0, TriggerType.Pre);
            }

            [HarmonyPostfix]
            public static void PostDie(Plant __instance, Plant.DieReason __0)
            {
                if (__instance != null && PlantEvent.HasEventComp(__instance))
                    PlantEvent.DieEvent(__instance, __0, TriggerType.Post);
            }
        }

        [HarmonyPatch]
        [HarmonyPriority(Priority.First)] // 数值越大执行顺序越靠后
        public static class Plant_PlantUpdatePatch
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetTargetMethods()
            {
                return SystemTools.GetAllMethods(SystemTools.GetAllDerivedTypes(typeof(Plant)), nameof(Plant.PlantUpdate),
                    BindingFlags.Default.AddAllAccess().AddInstance().AddDeclaredOnly());
            }

            [HarmonyPrefix]
            public static void PrePlantUpdate(Plant __instance, ref bool __state)
            {
                if (__instance != null && PlantEvent.HasEventComp(__instance))
                {
                    // OnUpdate
                    // _ = PlantEvent.Resolvers.PlantResolver.PreUpdate.Update(__instance);
                    //_ = PlantEvent.Resolvers.Run(() =>
                    //{
                    //    if (__instance != null && PlantEvent.HasEventComp(__instance))
                    //        PlantEvent.OnUpdate(__instance, TriggerType.Pre);
                    //});
                    // AttributeEvent
                    if (__instance.attributeCountdown > 0f && __instance.attributeCountdown - Time.deltaTime * __instance.attributeSpeed <= 0f)
                    {
                        __state = true;
                        PlantEvent.AttributeEvent(__instance, TriggerType.Pre);
                    }
                }
            }

            [HarmonyPostfix]
            public static void PostPlantUpdate(Plant __instance, bool __state)
            {
                if (__instance != null && PlantEvent.HasEventComp(__instance))
                {
                    // OnUpdate
                    // _ = PlantEvent.Resolvers.PlantResolver.PostUpdate.Update(__instance);
                    //_ = PlantEvent.Resolvers.Run(() =>
                    //{
                    //    if (__instance != null && PlantEvent.HasEventComp(__instance))
                    //        PlantEvent.OnUpdate(__instance, TriggerType.Post);
                    //});
                    // AttributeEvent
                    if (__state)
                    {
                        PlantEvent.AttributeEvent(__instance, TriggerType.Post);
                    }
                }
            }
        }

        //[HarmonyPatch]
        //[HarmonyPriority(Priority.First)] // 数值越大执行顺序越靠后
        //public static class Plant_UpdatePatch
        //{
        //    [HarmonyTargetMethods]
        //    public static IEnumerable<MethodBase> GetTargetMethods()
        //    {
        //        return SystemTools.GetAllMethods(SystemTools.GetAllDerivedTypes<Plant>(), nameof(Plant.Update),
        //            BindingFlags.Default.AddAllAccess().AddInstance().AddDeclaredOnly());
        //    }

        //    [HarmonyPrefix]
        //    public static void PreUpdate(Plant __instance)
        //    {
        //        _ = LocalMethod(__instance, TriggerType.Pre, UPDATE);
        //    }

        //    [HarmonyPostfix]
        //    public static void PostUpdate(Plant __instance)
        //    {
        //        _ = LocalMethod(__instance, TriggerType.Post, UPDATE);
        //    }
        //}

        //[HarmonyPatch]
        //[HarmonyPriority(Priority.First)] // 数值越大执行顺序越靠后
        //public static class Plant_FixedUpdatePatch
        //{
        //    [HarmonyTargetMethods]
        //    public static IEnumerable<MethodBase> GetTargetMethods()
        //    {
        //        return SystemTools.GetAllMethods(SystemTools.GetAllDerivedTypes<Plant>(), nameof(Plant.FixedUpdate),
        //            BindingFlags.Default.AddAllAccess().AddInstance().AddDeclaredOnly());
        //    }

        //    [HarmonyPrefix]
        //    public static void PreFixedUpdate(Plant __instance)
        //    {
        //        _ = LocalMethod(__instance, TriggerType.Pre, FIXEDUPDATE);
        //    }

        //    [HarmonyPostfix]
        //    public static void PostFixedUpdate(Plant __instance)
        //    {
        //        _ = LocalMethod(__instance, TriggerType.Post, FIXEDUPDATE);
        //    }
        //}
    }
    #endregion
}
