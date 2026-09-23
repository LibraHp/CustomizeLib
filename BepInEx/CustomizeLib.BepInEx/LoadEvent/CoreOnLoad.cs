using CustomizeLib.BepInEx.Hook;
using CustomizeLib.BepInEx.ToolInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.LoadEvent
{
    internal static class CoreOnLoad
    {
        public static void OnLoad()
        {
            // Native IL2CPP hooks are disabled on the Android CoreCLR launcher.
            // SavePlantData has no stable managed constructor entry point, and
            // detouring it can abort the JIT during GameAPP.Start.
        }
    }
}
