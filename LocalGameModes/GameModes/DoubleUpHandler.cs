using UnboundLib;
using UnboundLib.GameModes;

namespace LGM.GameModes
{
    public class DoubleUpHandler : GameModeHandler<GM_DoubleUp>
    {
        public override string Name
        {
            get { return "Double Up"; }
        }

        public override GameSettings Settings { get; protected set; }

        public DoubleUpHandler() : base("DoubleUp")
        {
            this.Settings = new GameSettings() {
                { "pointsToWinRound", 2 },
                { "roundsToWinGame", 3 }
            };
        }

        public override void SetActive(bool active)
        {
            this.GameMode.gameObject.SetActive(active);
        }

        public override void PlayerJoined(Player player)
        {
            GM_DoubleUp.instance.PlayerJoined(player);
        }

        public override void PlayerDied(Player player, int playersAlive)
        {
            GM_DoubleUp.instance.PlayerDied(player, playersAlive);
        }

        public override TeamScore GetTeamScore(int teamID)
        {
            return new TeamScore(this.GameMode.teamPoints[teamID], this.GameMode.teamRounds[teamID]);
        }

        public override void SetTeamScore(int teamID, TeamScore score)
        {
            this.GameMode.teamPoints[teamID] = score.points;
            this.GameMode.teamRounds[teamID] = score.rounds;
        }

        public override void StartGame()
        {
            GM_DoubleUp.instance.StartGame();
        }

        public override void ResetGame()
        {
            GM_DoubleUp.instance.ResetMatch();
        }

        public override void ChangeSetting(string name, object value)
        {
            base.ChangeSetting(name, value);

            if (name == "roundsToWinGame")
            {
                UIHandler.instance.InvokeMethod("SetNumberOfRounds", (int) value);
            }
        }
    }
}