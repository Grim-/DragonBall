using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{
    public class TournamentHistoryEntry : IExposable
    {
        public string fighterName;
        public string fighterID;
        public List<Tournament> tournaments = new List<Tournament>();
        public int totalFights;
        public int victories;
        public float totalExperience;
        public int dragonBallsWon;
        public int goldWon;

        public float winRate => totalFights > 0 ? (float)victories / totalFights : 0f;

        public void ExposeData()
        {
            Scribe_Values.Look(ref fighterName, "fighterName");
            Scribe_Values.Look(ref fighterID, "fighterID");

            Scribe_Collections.Look(ref tournaments, "tournaments", LookMode.Deep);
            Scribe_Values.Look(ref totalFights, "totalFights");
            Scribe_Values.Look(ref victories, "victories");
            Scribe_Values.Look(ref totalExperience, "totalExperience");
            Scribe_Values.Look(ref dragonBallsWon, "dragonBallsWon");
            Scribe_Values.Look(ref goldWon, "goldWon");
        }
    }

    public class Tournament : IExposable
    {
        public int TournamentID;
        public string FighterID;
        public string WinnerID;
        public List<TournamentMatchResult> Matches = new List<TournamentMatchResult>();

        public void ExposeData()
        {
            Scribe_Values.Look(ref TournamentID, "TournamentID");
            Scribe_Values.Look(ref FighterID, "FighterID");
            Scribe_Values.Look(ref WinnerID, "winnerID");
            Scribe_Collections.Look(ref Matches, "Matches", LookMode.Deep);
        }
    }
}
