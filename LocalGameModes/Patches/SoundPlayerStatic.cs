using HarmonyLib;
using SoundImplementation;

namespace LGM.Patches
{
    [HarmonyPatch(typeof(SoundPlayerStatic), "PlayPlayerAdded")]
    class SoundPlayerStatic_Patch_PlayPlayerAdded
    {
        static bool Prefix() {
            return LGMMod.instance.GetSoundEnabled("PlayerAdded");
        }
    }
}
