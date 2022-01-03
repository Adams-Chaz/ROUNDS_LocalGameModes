using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LGM.Patches
{
    [HarmonyPatch(typeof(CardChoiceVisuals), "Show")]
    class CardChoiceVisuals_Patch_Show
    {
        internal static Dictionary<Player, bool> playerHasPicked = new Dictionary<Player, bool>() { };

        static bool Prefix(CardChoiceVisuals __instance, int pickerID)
        {
            Player player = (Player) typeof(PlayerManager).InvokeMember("GetPlayerWithID",
                BindingFlags.Instance | BindingFlags.InvokeMethod |
                BindingFlags.NonPublic, null, PlayerManager.instance, new object[] { pickerID });
            // skip pick phase if the player has passed
            if (
                (
                    LGMMod.MaxCards > 0 && 
                    ModdingUtils.Utils.CardBarUtils.instance.GetCardBarSquares(player.teamID).Length - 1 >= LGMMod.MaxCards && 
                    !LGMMod.DiscardAfterPick &&
                    LGMMod.PassDiscard && 
                    playerHasPicked[player]
                ) || PreGamePickBanHandler.skipFirstPickPhase)
            {
                playerHasPicked[player] = false;
                return false;
            }
            else
            {
                playerHasPicked[player] = true;
                return true;
            }
        }

        static void Postfix(CardChoiceVisuals __instance, ref GameObject ___currentSkin, int pickerID) {
            if (___currentSkin) {
                GameObject.Destroy(___currentSkin);
            }

            // Show team color instead of individual player color
            var child = __instance.transform.GetChild(0);
            var player = PlayerManager.instance.players[pickerID];
            ___currentSkin = Object.Instantiate(PlayerSkinBank.GetPlayerSkinColors(player.teamID).gameObject, child.position, Quaternion.identity, child);
            ___currentSkin.GetComponentInChildren<ParticleSystem>().Play();
        }
    }
}
