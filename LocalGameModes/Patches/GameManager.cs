using HarmonyLib;

namespace LGM.Patches
{
    [HarmonyPatch(typeof(MainMenuHandler), "Awake")]
    class MainMenuHandler_Patch_Awake
    {
        static void Postfix() {
            LGMMod.instance.InjectUIElements();
            LGMMod.instance.SetupGameModes();
        }
    }
}
