using CustomizeLib.BepInEx.Hook;
using CustomizeLib.BepInEx.Utility;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomizeLib.BepInEx.Extra.PlantExtra.IPlantEvent
{
    [HarmonyPatch(typeof(SavePlantData))]
    [HarmonyPriority(Priority.First)] // 数值越大执行顺序越靠后
    public static class SavePlantDataPatch
    {
        [HarmonyPatch(nameof(SavePlantData.LoadData))]
        [HarmonyPrefix]
        public static void PreLoadData(SavePlantData __instance, ref Plant plant)
        {
            PlantEvent.AfterDeserialized(plant, __instance, TriggerType.Pre);
        }

        [HarmonyPatch(nameof(SavePlantData.LoadData))]
        [HarmonyPostfix]
        public static void PostLoadData(SavePlantData __instance, ref Plant plant)
        {
            PlantEvent.AfterDeserialized(plant, __instance, TriggerType.Post);
        }
    }

}
