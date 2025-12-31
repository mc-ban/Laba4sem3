namespace WPFNASTYA
{
    public class GameData
    {
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }
        public string Player1Faction { get; set; }
        public string Player2Faction { get; set; }

        public GameData(string p1Name, string p2Name, string p1Faction, string p2Faction)
        {
            Player1Name = p1Name;
            Player2Name = p2Name;
            Player1Faction = p1Faction;
            Player2Faction = p2Faction;
        }
    }
}