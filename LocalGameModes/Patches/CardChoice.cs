using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LGM.Patches
{
    [Serializable]
    [HarmonyPatch(typeof(CardChoice), "DoPick")]
    class CardChoicePatchDoPick
    {
        internal static Dictionary<Player, bool> playerHasPicked = new Dictionary<Player, bool>() { };
        private static bool Prefix(CardChoice __instance, int picketIDToSet)
        {
            Player player = (Player) typeof(PlayerManager).InvokeMember("GetPlayerWithID",
                BindingFlags.Instance | BindingFlags.InvokeMethod |
                BindingFlags.NonPublic, null, PlayerManager.instance, new object[] { picketIDToSet });
            // skip pick phase if the player has passed
            if (
                (
                    LGMMod.MaxCards > 0 && 
                    ModdingUtils.Utils.CardBarUtils.instance.GetCardBarSquares(player.teamID).Length - 1 >= LGMMod.MaxCards && 
                    !LGMMod.DiscardAfterPick && 
                    LGMMod.PassDiscard && 
                    !playerHasPicked[player]
                ) 
                || PreGamePickBanHandler.skipFirstPickPhase)
            {
                playerHasPicked[player] = false;
                __instance.IsPicking = false;
                return false;
            }
            else
            {
                playerHasPicked[player] = true;
                return true;
            }
        }
    }
}
