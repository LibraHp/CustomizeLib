using HarmonyLib;

namespace CustomizeLib.BepInEx.Extra.PlantExtra.IPlantEvent
{
    // Kept in line with the reference implementation. The native constructor
    // hook remains disabled because CoreOnLoad does not install native hooks.
    [HarmonyPatch(typeof(SavePlantData))]
    [HarmonyPriority(Priority.First)]
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
