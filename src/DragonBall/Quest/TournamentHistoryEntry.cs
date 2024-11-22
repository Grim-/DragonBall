using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{
    public class TournamentHistoryEntry : IExposable
    {
        public string fighterName;
        public int totalFights;
        public int victories;
        public float totalExperience;
        public int dragonBallsWon;
        public int goldWon;
        public List<Tournament> tournaments = new List<Tournament>();  // Changed from matchHistory to tournaments

        public float winRate => totalFights > 0 ? (float)victories / totalFights : 0f;

        public void ExposeData()
        {
            Scribe_Values.Look(ref fighterName, "fighterName");
            Scribe_Values.Look(ref totalFights, "totalFights");
            Scribe_Values.Look(ref victories, "victories");
            Scribe_Values.Look(ref totalExperience, "totalExperience");
            Scribe_Values.Look(ref dragonBallsWon, "dragonBallsWon");
            Scribe_Values.Look(ref goldWon, "goldWon");
            Scribe_Collections.Look(ref tournaments, "tournaments", LookMode.Deep);
        }
    }

    public class Tournament : IExposable
    {
        public int TournamentID;
        public List<TournamentMatchResult> Matches = new List<TournamentMatchResult>();

        public void ExposeData()
        {
            Scribe_Values.Look(ref TournamentID, "TournamentID");
            Scribe_Collections.Look(ref Matches, "Matches", LookMode.Deep);
        }
    }
}
