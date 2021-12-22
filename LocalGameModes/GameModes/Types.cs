namespace LGM.GameModes
{
    public class GameModeType
    { 
        public GameModeType(string key, string name, int minPlayers = 2, int maxPlayers = 4, int maxTeams = 2)
        {
            this.Key = key;
            this.Name = name;
            this.MinPlayers = minPlayers;
            this.MaxPlayers = maxPlayers;
            this.MaxTeams = maxTeams;
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int MaxTeams { get; set; }
    }

    public static class Types
    {
        private const string _deathmatch = "Deathmatch";
        private const string _armsRace = "ArmsRace";
        private const string _doubleUp = "DoubleUp";

        /// <summary>
        /// Free for All battle royal.
        /// </summary>
        public static GameModeType Deathmatch 
        { 
            get => new GameModeType(_deathmatch, "FREE FOR ALL", 2, 8, 8); 
        }
        /// <summary>
        /// 2v2v2v2, Team Battles.
        /// </summary>
        public static GameModeType ArmsRace 
        { 
            get => new GameModeType(_armsRace, "SWAT", 2, 8, 4); 
        }
        /// <summary>
        /// Double the cards per round.
        /// </summary>
        public static GameModeType DoubleUp 
        { 
            get => new GameModeType(_doubleUp, "DOUBLE UP"); 
        }

        public static GameModeType GetCurrentGameMode(string gameModeKey)
        {
            switch (gameModeKey)
            {
                case _deathmatch:
                    return Deathmatch;
                case _armsRace:
                    return ArmsRace;
                case _doubleUp:
                default:
                    return DoubleUp;
            }
        }

        public static GameModeType GetNextGameMode(string gameModeKey)
        {
            switch (gameModeKey)
            {
                case _deathmatch:
                    return ArmsRace;
                case _armsRace:
                    //return DoubleUp;
                case _doubleUp:
                default:
                    return Deathmatch;
            }
        }
    }
}
