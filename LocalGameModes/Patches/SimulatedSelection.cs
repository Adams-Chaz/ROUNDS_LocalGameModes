using HarmonyLib;

namespace LGM.Patches
{
    [HarmonyPatch(typeof(SimulatedSelection), "Deselect")]
    class SimulatedSelection_Patch_Deselect
    {
        static bool Prefix(HoverEvent ___hoverEvent) {
            return ___hoverEvent != null;
        }
    }
}
