using System;
using UnboundLib;

namespace LGM
{
    public static class ArmsRaceExtensions
    {
        public static void SetPlayersNeededToStart(this GM_ArmsRace instance, int num) {
            instance.SetFieldValue("playersNeededToStart", num);
        }

        public static int GetPlayersNeededToStart(this GM_ArmsRace instance) {
            return (int) instance.GetFieldValue("playersNeededToStart");
        }
    }
}