using System.Collections;
using System.Linq;
using UnboundLib.GameModes;

namespace LGM
{
    internal static class WinByTwo
    {
        // win by two code, courtesy of Slicimus
        private static int teams = 0;
        private static int syncRounds = 5;
        private static int syncPoints = 2;
        private static int tiedRounds = 0;
        private static int tiedPoints = 0;
        private static int initRounds = 0;
        private static int initPoints = 0;
        private static bool SW = true;

        internal static IEnumerator RoundX2(IGameModeHandler gm)
        {
            teams = PlayerManager.instance.players.Select(p => p.teamID).Distinct().Count();
            if (LGMMod.WinByTwoRounds)
            {
                tiedRounds = 0;
                for (int i = 0; i < teams; i++)
                {
                    if (gm.GetTeamScore(i).rounds == syncRounds - 1)
                    {
                        tiedRounds++;
                    }
                }
                syncRounds = (int) GameModeManager.CurrentHandler.Settings["roundsToWinGame"];
                if (tiedRounds >= 2)
                {
                    syncRounds++;
                    gm.ChangeSetting("roundsToWinGame", syncRounds);
                }
            }
            yield break;
        }
        internal static IEnumerator PointX2(IGameModeHandler gm)
        {
            if (LGMMod.WinByTwoPoints)
            {
                syncRounds = (int) GameModeManager.CurrentHandler.Settings["roundsToWinGame"];
                syncPoints = (int) GameModeManager.CurrentHandler.Settings["pointsToWinRound"];
                tiedRounds = 0;
                for (int i = 0; i < teams; i++)
                {
                    if (gm.GetTeamScore(i).rounds == syncRounds - 1)
                    {
                        tiedRounds++;
                    }
                }
                tiedPoints = 0;
                for (int i = 0; i < teams; i++)
                {
                    if (gm.GetTeamScore(i).points == syncPoints - 1 && gm.GetTeamScore(i).rounds == syncRounds - 1)
                    {
                        tiedPoints++;
                    }
                }
                if (tiedRounds >= 2 && tiedPoints >= 2)
                {
                    syncPoints++;
                    gm.ChangeSetting("pointsToWinRound", syncPoints);
                }
            }
            yield break;
        }
        internal static IEnumerator ResetPoints(IGameModeHandler gm)
        {
            if (SW)
            {
                initPoints = (int) GameModeManager.CurrentHandler.Settings["pointsToWinRound"];
                initRounds = (int) GameModeManager.CurrentHandler.Settings["roundsToWinGame"];
                SW = false;
            }
            else
            {
                gm.ChangeSetting("pointsToWinRound", initPoints);
                gm.ChangeSetting("roundsToWinGame", initRounds);
            }
            yield break;
        }
    }
}
