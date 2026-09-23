using UnityEngine;

namespace CustomizeLib.BepInEx.Extra.PlantExtra.IPlantEvent
{
    /// <summary>
    /// Dispatches plant events from one managed Unity component.
    /// </summary>
    public class PlantEventDriver : MonoBehaviour
    {
        public void Update()
        {
            PlantEvent.DriveUpdate(TriggerType.Pre);
            PlantEvent.DriveUpdate(TriggerType.Post);
        }

        public void FixedUpdate()
        {
            PlantEvent.DriveFixedUpdate(TriggerType.Pre);
            PlantEvent.DriveFixedUpdate(TriggerType.Post);
        }
    }
}
